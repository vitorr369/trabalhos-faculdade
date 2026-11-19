using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasmorraDFS
{
    public enum ConteudoSala
    {
        Vazio,
        Entrada,
        Chefe,
        Tesouro,
        Armadilha
    }

    public class Vertice
    {
        public int Id { get; }
        public int Linha { get; }
        public int Coluna { get; }
        public ConteudoSala Conteudo { get; set; }
        public int Profundidade { get; set; }
        public int TempoDescoberta { get; set; }
        public int TempoFinalizacao { get; set; }

        public Vertice(int id, int linha, int coluna)
        {
            Id = id;
            Linha = linha;
            Coluna = coluna;
            Conteudo = ConteudoSala.Vazio;
        }

        public override string ToString() => $"V{Id}({Linha},{Coluna})";
    }

    public class ResultadoBuscaDFS
    {
        public bool Encontrou { get; }
        public List<Vertice> Caminho { get; }
        public HashSet<Vertice> Visitados { get; }
        public int TotalBacktracks { get; }
        public int SalasDescartadas => Math.Max(0, Visitados.Count - Caminho.Count);

        public ResultadoBuscaDFS(bool encontrou, List<Vertice> caminho, HashSet<Vertice> visitados, int totalBacktracks)
        {
            Encontrou = encontrou;
            Caminho = caminho;
            Visitados = visitados;
            TotalBacktracks = totalBacktracks;
        }
    }

    public class EstatisticasGrafo
    {
        public int TotalVertices { get; set; }
        public int TotalArestas { get; set; }
        public bool EhArvoreGeradora { get; set; }
        public int GrauMinimo { get; set; }
        public int GrauMaximo { get; set; }
        public double GrauMedio { get; set; }
        public int TotalFolhasBecos { get; set; }
        public int ProfundidadeMaxima { get; set; }
        public int BacktracksNaGeracao { get; set; }
        public int TotalTesouros { get; set; }
        public int TotalArmadilhas { get; set; }
        public int TempoFinalizacaoRaiz { get; set; }
    }

    public class GrafoMasmorra
    {
        public int Linhas { get; }
        public int Colunas { get; }
        public int Seed { get; }
        public Vertice[,] Grade { get; }
        public Dictionary<Vertice, List<Vertice>> ListaAdjacencia { get; }
        public Vertice Entrada { get; private set; }
        public Vertice Chefe { get; private set; }
        public int TotalArestas { get; private set; }
        public int ProfundidadeMaxima { get; private set; }
        public int TotalBacktracksGeracao { get; private set; }
        public List<Vertice> OrdemDescoberta { get; }

        private readonly Random _random;

        public GrafoMasmorra(int linhas, int colunas, int? seed = null)
        {
            Linhas = Math.Clamp(linhas, 4, 18);
            Colunas = Math.Clamp(colunas, 4, 28);
            Seed = seed ?? Random.Shared.Next(100000, 999999);
            _random = new Random(Seed);

            Grade = new Vertice[Linhas, Colunas];
            ListaAdjacencia = new Dictionary<Vertice, List<Vertice>>();
            OrdemDescoberta = new List<Vertice>();

            int contadorId = 0;
            for (int l = 0; l < Linhas; l++)
            {
                for (int c = 0; c < Colunas; c++)
                {
                    var vertice = new Vertice(contadorId++, l, c);
                    Grade[l, c] = vertice;
                    ListaAdjacencia[vertice] = new List<Vertice>();
                }
            }

            Entrada = Grade[0, 0];
            Chefe = Grade[0, 0];
        }

        public void AdicionarAresta(Vertice origem, Vertice destino)
        {
            if (!ListaAdjacencia[origem].Contains(destino))
            {
                ListaAdjacencia[origem].Add(destino);
                ListaAdjacencia[destino].Add(origem);
                TotalArestas++;
            }
        }

        public bool SaoAdjacentes(Vertice a, Vertice b)
        {
            return ListaAdjacencia.TryGetValue(a, out var vizinhos) && vizinhos.Contains(b);
        }

        public Vertice? ObterVertice(int linha, int coluna)
        {
            if (linha < 0 || linha >= Linhas || coluna < 0 || coluna >= Colunas)
                return null;

            return Grade[linha, coluna];
        }

        public void GerarComDFSIterativo(
            bool animado = false,
            int delayMs = 35,
            Action<GrafoMasmorra, Vertice, Stack<Vertice>, bool>? aoAtualizarFrame = null)
        {
            TotalArestas = 0;
            TotalBacktracksGeracao = 0;
            foreach (var lista in ListaAdjacencia.Values)
                lista.Clear();

            var visitados = new HashSet<Vertice>();
            var pilha = new Stack<Vertice>();

            Entrada = Grade[0, 0];
            visitados.Add(Entrada);
            pilha.Push(Entrada);

            bool pularAnimacao = false;

            while (pilha.Count > 0)
            {
                if (animado && !pularAnimacao && Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    pularAnimacao = true;
                }

                Vertice atual = pilha.Peek();
                var vizinhosDisponiveis = ObterVizinhosDeGradeNaoVisitados(atual, visitados);

                if (vizinhosDisponiveis.Count > 0)
                {
                    Vertice proximo = vizinhosDisponiveis[_random.Next(vizinhosDisponiveis.Count)];
                    AdicionarAresta(atual, proximo);
                    visitados.Add(proximo);
                    pilha.Push(proximo);

                    if (animado && !pularAnimacao)
                    {
                        aoAtualizarFrame?.Invoke(this, proximo, pilha, false);
                        Thread.Sleep(delayMs);
                    }
                }
                else
                {
                    pilha.Pop();
                    TotalBacktracksGeracao++;

                    if (animado && !pularAnimacao && pilha.Count > 0)
                    {
                        aoAtualizarFrame?.Invoke(this, pilha.Peek(), pilha, true);
                        Thread.Sleep(delayMs);
                    }
                }
            }

            MapearTemposEProfundidadeDFSRecursivo();
            PopularConteudo();
        }

        private List<Vertice> ObterVizinhosDeGradeNaoVisitados(Vertice v, HashSet<Vertice> visitados)
        {
            var resultado = new List<Vertice>(4);
            int[] dLinha = { -1, 1, 0, 0 };
            int[] dColuna = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nl = v.Linha + dLinha[i];
                int nc = v.Coluna + dColuna[i];

                if (nl >= 0 && nl < Linhas && nc >= 0 && nc < Colunas)
                {
                    Vertice candidato = Grade[nl, nc];
                    if (!visitados.Contains(candidato))
                        resultado.Add(candidato);
                }
            }

            return resultado;
        }

        private void MapearTemposEProfundidadeDFSRecursivo()
        {
            var visitados = new HashSet<Vertice>();
            int relogio = 0;
            ProfundidadeMaxima = -1;
            OrdemDescoberta.Clear();

            void Visitar(Vertice v, int profundidade)
            {
                visitados.Add(v);
                relogio++;
                v.TempoDescoberta = relogio;
                v.Profundidade = profundidade;
                OrdemDescoberta.Add(v);

                if (profundidade > ProfundidadeMaxima)
                {
                    ProfundidadeMaxima = profundidade;
                    Chefe = v;
                }

                foreach (var vizinho in ListaAdjacencia[v])
                {
                    if (!visitados.Contains(vizinho))
                    {
                        Visitar(vizinho, profundidade + 1);
                    }
                }

                relogio++;
                v.TempoFinalizacao = relogio;
            }

            Visitar(Entrada, 0);
        }

        private void PopularConteudo()
        {
            foreach (var v in ListaAdjacencia.Keys)
                v.Conteudo = ConteudoSala.Vazio;

            Entrada.Conteudo = ConteudoSala.Entrada;
            Chefe.Conteudo = ConteudoSala.Chefe;

            var solucaoPrincipal = BuscarCaminhoDFS(Entrada, Chefe);
            var caminhoCritico = new HashSet<Vertice>(solucaoPrincipal.Caminho);

            var becos = ListaAdjacencia
                .Where(kv => kv.Value.Count == 1 && kv.Key != Entrada && kv.Key != Chefe)
                .Select(kv => kv.Key)
                .OrderBy(_ => _random.Next())
                .ToList();

            int qtdTesouros = Math.Max(2, (int)Math.Ceiling(becos.Count * 0.55));
            for (int i = 0; i < Math.Min(qtdTesouros, becos.Count); i++)
            {
                becos[i].Conteudo = ConteudoSala.Tesouro;
            }

            var candidatosArmadilha = ListaAdjacencia.Keys
                .Where(v => v.Conteudo == ConteudoSala.Vazio && !caminhoCritico.Contains(v))
                .OrderBy(_ => _random.Next())
                .ToList();

            int qtdArmadilhas = Math.Max(1, (int)(Linhas * Colunas * 0.05));
            for (int i = 0; i < Math.Min(qtdArmadilhas, candidatosArmadilha.Count); i++)
            {
                candidatosArmadilha[i].Conteudo = ConteudoSala.Armadilha;
            }
        }

        public ResultadoBuscaDFS BuscarCaminhoDFS(Vertice origem, Vertice destino)
        {
            var visitados = new HashSet<Vertice>();
            var caminho = new List<Vertice>();
            int backtracks = 0;

            bool Explorar(Vertice atual)
            {
                visitados.Add(atual);
                caminho.Add(atual);

                if (atual == destino)
                    return true;

                foreach (var vizinho in ListaAdjacencia[atual])
                {
                    if (!visitados.Contains(vizinho))
                    {
                        if (Explorar(vizinho))
                            return true;
                    }
                }

                caminho.RemoveAt(caminho.Count - 1);
                backtracks++;
                return false;
            }

            bool encontrou = Explorar(origem);
            return new ResultadoBuscaDFS(encontrou, caminho, visitados, backtracks);
        }

        public EstatisticasGrafo CalcularEstatisticas()
        {
            var graus = ListaAdjacencia.Values.Select(v => v.Count).ToList();
            int totalVertices = Linhas * Colunas;

            return new EstatisticasGrafo
            {
                TotalVertices = totalVertices,
                TotalArestas = TotalArestas,
                EhArvoreGeradora = TotalArestas == totalVertices - 1,
                GrauMinimo = graus.Min(),
                GrauMaximo = graus.Max(),
                GrauMedio = Math.Round(graus.Average(), 2),
                TotalFolhasBecos = graus.Count(g => g == 1),
                ProfundidadeMaxima = ProfundidadeMaxima,
                BacktracksNaGeracao = TotalBacktracksGeracao,
                TotalTesouros = ListaAdjacencia.Keys.Count(v => v.Conteudo == ConteudoSala.Tesouro),
                TotalArmadilhas = ListaAdjacencia.Keys.Count(v => v.Conteudo == ConteudoSala.Armadilha),
                TempoFinalizacaoRaiz = Entrada.TempoFinalizacao
            };
        }

        public string ExportarRelatorioTxt(string caminhoArquivo)
        {
            var stats = CalcularEstatisticas();
            var busca = BuscarCaminhoDFS(Entrada, Chefe);
            var sb = new StringBuilder();

            sb.AppendLine("================================================================");
            sb.AppendLine("        RELATORIO DO GRAFO - MASMORRA PROCEDURAL (DFS)");
            sb.AppendLine("================================================================");
            sb.AppendLine($"Data/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Dimensoes: {Linhas} linhas x {Colunas} colunas | Seed: {Seed}");
            sb.AppendLine();
            sb.AppendLine("--- METRICAS DE TEORIA DOS GRAFOS ---");
            sb.AppendLine($"Total de Vertices |V|:          {stats.TotalVertices}");
            sb.AppendLine($"Total de Arestas  |E|:          {stats.TotalArestas}");
            sb.AppendLine($"Arvore Geradora (|E| = |V|-1):  {(stats.EhArvoreGeradora ? "SIM (Conexo e Aciclico)" : "NAO")}");
            sb.AppendLine($"Grau Minimo / Medio / Maximo:   {stats.GrauMinimo} / {stats.GrauMedio:F2} / {stats.GrauMaximo}");
            sb.AppendLine($"Becos sem Saida (Grau 1):       {stats.TotalFolhasBecos}");
            sb.AppendLine($"Profundidade Maxima (DFS):      {stats.ProfundidadeMaxima} passos");
            sb.AppendLine($"Backtracks na Geracao:          {stats.BacktracksNaGeracao}");
            sb.AppendLine($"Tempo Finalizacao Raiz f[E]:    {stats.TempoFinalizacaoRaiz} (esperado: 2*|V| = {stats.TotalVertices * 2})");
            sb.AppendLine();
            sb.AppendLine("--- RESULTADO DA BUSCA DFS (ENTRADA -> CHEFE) ---");
            sb.AppendLine($"Origem: {Entrada} -> Destino: {Chefe}");
            sb.AppendLine($"Comprimento do caminho:         {busca.Caminho.Count - 1} arestas ({busca.Caminho.Count} vertices)");
            sb.AppendLine($"Vertices visitados na busca:    {busca.Visitados.Count}");
            sb.AppendLine($"Backtracks durante a busca:     {busca.TotalBacktracks}");
            sb.AppendLine($"Caminho: {string.Join(" -> ", busca.Caminho)}");
            sb.AppendLine();
            sb.AppendLine("--- MAPA VISUAL (ASCII COM SOLUCAO DFS) ---");
            sb.AppendLine(RenderizadorConsole.GerarMapaTextoPlano(this, busca));
            sb.AppendLine("Legenda: [E] Entrada | [B] Chefe | [T] Tesouro | [X] Armadilha | [*] Caminho DFS | [.] Visitado/Backtrack");
            sb.AppendLine();
            sb.AppendLine("--- LISTA DE ADJACENCIA COMPLETA ---");

            foreach (var par in ListaAdjacencia.OrderBy(p => p.Key.Id))
            {
                string vizinhos = string.Join(", ", par.Value.Select(v => v.ToString()));
                sb.AppendLine($"{par.Key,-12} [grau={par.Value.Count}, prof={par.Key.Profundidade,2}, d={par.Key.TempoDescoberta,3}, f={par.Key.TempoFinalizacao,3}] -> [{vizinhos}]");
            }

            File.WriteAllText(caminhoArquivo, sb.ToString(), Encoding.UTF8);
            return Path.GetFullPath(caminhoArquivo);
        }
    }

    public static class RenderizadorConsole
    {
        public static bool UsarBlocosUnicode { get; set; } = true;

        private static string Parede => UsarBlocosUnicode ? "██" : "##";
        private static string Vazio => "  ";

        public static void ExibirMapa(
            GrafoMasmorra grafo,
            Vertice? cursorAnimacao = null,
            Vertice? jogador = null,
            ResultadoBuscaDFS? solucao = null,
            bool mostrarVisitados = true,
            bool mostrarArmadilhas = true,
            HashSet<Vertice>? tesourosColetados = null)
        {
            int alturaVisual = grafo.Linhas * 2 + 1;
            int larguraVisual = grafo.Colunas * 2 + 1;

            var conjuntoCaminho = solucao != null ? new HashSet<Vertice>(solucao.Caminho) : null;
            var conjuntoVisitados = (solucao != null && mostrarVisitados) ? solucao.Visitados : null;

            for (int l = 0; l < alturaVisual; l++)
            {
                ConsoleColor? corAtual = null;
                var buffer = new StringBuilder();

                void Emitir(string texto, ConsoleColor cor)
                {
                    if (corAtual != cor)
                    {
                        if (buffer.Length > 0)
                        {
                            Console.ForegroundColor = corAtual ?? ConsoleColor.Gray;
                            Console.Write(buffer.ToString());
                            buffer.Clear();
                        }
                        corAtual = cor;
                    }
                    buffer.Append(texto);
                }

                for (int c = 0; c < larguraVisual; c++)
                {
                    bool linhaSala = l % 2 == 1;
                    bool colunaSala = c % 2 == 1;

                    if (linhaSala && colunaSala)
                    {
                        Vertice v = grafo.Grade[l / 2, c / 2];

                        if (cursorAnimacao != null && v == cursorAnimacao)
                        {
                            Emitir("@@", ConsoleColor.Cyan);
                        }
                        else if (jogador != null && v == jogador)
                        {
                            Emitir("@@", ConsoleColor.Cyan);
                        }
                        else if (v.Conteudo == ConteudoSala.Entrada)
                        {
                            Emitir("E ", ConsoleColor.Green);
                        }
                        else if (v.Conteudo == ConteudoSala.Chefe)
                        {
                            Emitir("B ", ConsoleColor.Red);
                        }
                        else if (v.Conteudo == ConteudoSala.Tesouro && (tesourosColetados == null || !tesourosColetados.Contains(v)))
                        {
                            Emitir("T ", ConsoleColor.Yellow);
                        }
                        else if (mostrarArmadilhas && v.Conteudo == ConteudoSala.Armadilha)
                        {
                            Emitir("X ", ConsoleColor.Magenta);
                        }
                        else if (conjuntoCaminho != null && conjuntoCaminho.Contains(v))
                        {
                            Emitir("* ", ConsoleColor.Cyan);
                        }
                        else if (conjuntoVisitados != null && conjuntoVisitados.Contains(v))
                        {
                            Emitir(". ", ConsoleColor.DarkYellow);
                        }
                        else if (grafo.ListaAdjacencia[v].Count == 0 && cursorAnimacao != null)
                        {
                            Emitir(Parede, ConsoleColor.DarkGray);
                        }
                        else
                        {
                            Emitir(Vazio, ConsoleColor.Gray);
                        }
                    }
                    else if (linhaSala && !colunaSala && c > 0 && c < larguraVisual - 1)
                    {
                        Vertice esquerda = grafo.Grade[l / 2, (c - 1) / 2];
                        Vertice direita = grafo.Grade[l / 2, (c + 1) / 2];

                        if (grafo.SaoAdjacentes(esquerda, direita))
                        {
                            if (conjuntoCaminho != null && conjuntoCaminho.Contains(esquerda) && conjuntoCaminho.Contains(direita))
                                Emitir("* ", ConsoleColor.Cyan);
                            else if (conjuntoVisitados != null && conjuntoVisitados.Contains(esquerda) && conjuntoVisitados.Contains(direita))
                                Emitir(". ", ConsoleColor.DarkYellow);
                            else
                                Emitir(Vazio, ConsoleColor.Gray);
                        }
                        else
                        {
                            Emitir(Parede, ConsoleColor.DarkGray);
                        }
                    }
                    else if (!linhaSala && colunaSala && l > 0 && l < alturaVisual - 1)
                    {
                        Vertice cima = grafo.Grade[(l - 1) / 2, c / 2];
                        Vertice baixo = grafo.Grade[(l + 1) / 2, c / 2];

                        if (grafo.SaoAdjacentes(cima, baixo))
                        {
                            if (conjuntoCaminho != null && conjuntoCaminho.Contains(cima) && conjuntoCaminho.Contains(baixo))
                                Emitir("* ", ConsoleColor.Cyan);
                            else if (conjuntoVisitados != null && conjuntoVisitados.Contains(cima) && conjuntoVisitados.Contains(baixo))
                                Emitir(". ", ConsoleColor.DarkYellow);
                            else
                                Emitir(Vazio, ConsoleColor.Gray);
                        }
                        else
                        {
                            Emitir(Parede, ConsoleColor.DarkGray);
                        }
                    }
                    else
                    {
                        Emitir(Parede, ConsoleColor.DarkGray);
                    }
                }

                if (buffer.Length > 0)
                {
                    Console.ForegroundColor = corAtual ?? ConsoleColor.Gray;
                    Console.Write(buffer.ToString());
                }

                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static string GerarMapaTextoPlano(GrafoMasmorra grafo, ResultadoBuscaDFS? solucao = null)
        {
            int alturaVisual = grafo.Linhas * 2 + 1;
            int larguraVisual = grafo.Colunas * 2 + 1;
            var caminho = solucao != null ? new HashSet<Vertice>(solucao.Caminho) : null;
            var visitados = solucao?.Visitados;
            var sb = new StringBuilder();

            for (int l = 0; l < alturaVisual; l++)
            {
                for (int c = 0; c < larguraVisual; c++)
                {
                    bool linhaSala = l % 2 == 1;
                    bool colunaSala = c % 2 == 1;

                    if (linhaSala && colunaSala)
                    {
                        Vertice v = grafo.Grade[l / 2, c / 2];
                        if (v.Conteudo == ConteudoSala.Entrada) sb.Append("E ");
                        else if (v.Conteudo == ConteudoSala.Chefe) sb.Append("B ");
                        else if (v.Conteudo == ConteudoSala.Tesouro) sb.Append("T ");
                        else if (v.Conteudo == ConteudoSala.Armadilha) sb.Append("X ");
                        else if (caminho != null && caminho.Contains(v)) sb.Append("* ");
                        else if (visitados != null && visitados.Contains(v)) sb.Append(". ");
                        else sb.Append("  ");
                    }
                    else if (linhaSala && !colunaSala && c > 0 && c < larguraVisual - 1)
                    {
                        Vertice esq = grafo.Grade[l / 2, (c - 1) / 2];
                        Vertice dir = grafo.Grade[l / 2, (c + 1) / 2];
                        if (grafo.SaoAdjacentes(esq, dir))
                        {
                            if (caminho != null && caminho.Contains(esq) && caminho.Contains(dir)) sb.Append("* ");
                            else if (visitados != null && visitados.Contains(esq) && visitados.Contains(dir)) sb.Append(". ");
                            else sb.Append("  ");
                        }
                        else sb.Append("##");
                    }
                    else if (!linhaSala && colunaSala && l > 0 && l < alturaVisual - 1)
                    {
                        Vertice cima = grafo.Grade[(l - 1) / 2, c / 2];
                        Vertice baixo = grafo.Grade[(l + 1) / 2, c / 2];
                        if (grafo.SaoAdjacentes(cima, baixo))
                        {
                            if (caminho != null && caminho.Contains(cima) && caminho.Contains(baixo)) sb.Append("* ");
                            else if (visitados != null && visitados.Contains(cima) && visitados.Contains(baixo)) sb.Append(". ");
                            else sb.Append("  ");
                        }
                        else sb.Append("##");
                    }
                    else
                    {
                        sb.Append("##");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void ExibirLegenda(bool incluirBusca = false, bool incluirJogador = false)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green; Console.Write("[E] "); Console.ResetColor(); Console.Write("Entrada  ");
            Console.ForegroundColor = ConsoleColor.Red; Console.Write("[B] "); Console.ResetColor(); Console.Write("Chefe (mais profundo)  ");
            Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("[T] "); Console.ResetColor(); Console.Write("Tesouro (beco)  ");
            Console.ForegroundColor = ConsoleColor.Magenta; Console.Write("[X] "); Console.ResetColor(); Console.Write("Armadilha");

            if (incluirJogador)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[@@] "); Console.ResetColor(); Console.Write("Jogador");
            }

            if (incluirBusca)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[*] "); Console.ResetColor(); Console.Write("Caminho DFS  ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("[.] "); Console.ResetColor(); Console.Write("Explorado e descartado (Backtracking)");
            }

            Console.WriteLine();
        }
    }

    public static class ModoExploracao
    {
        public static void Executar(GrafoMasmorra grafo)
        {
            Vertice atual = grafo.Entrada;
            int vida = 3;
            int passos = 0;
            int pontos = 0;
            bool bussolaAtiva = false;
            var tesourosColetados = new HashSet<Vertice>();
            var armadilhasAtivadas = new HashSet<Vertice>();
            string mensagem = "Use W/A/S/D ou Setas para navegar pelas arestas do grafo. [H] Bussola DFS | [ESC] Sair";

            int totalTesouros = grafo.ListaAdjacencia.Keys.Count(v => v.Conteudo == ConteudoSala.Tesouro);
            var caminhoIdealInicial = grafo.BuscarCaminhoDFS(grafo.Entrada, grafo.Chefe);

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("==================== MODO EXPLORACAO DA MASMORRA ====================");
                Console.ResetColor();
                Console.WriteLine($"Vida: {vida}/3 | Pontos: {pontos} | Passos: {passos} | Tesouros: {tesourosColetados.Count}/{totalTesouros} | Posicao: {atual}");
                Console.WriteLine($"Bussola DFS: {(bussolaAtiva ? "ATIVADA" : "desativada")} | Vizinhos conectados (grau): {grafo.ListaAdjacencia[atual].Count}\n");

                ResultadoBuscaDFS? dica = bussolaAtiva ? grafo.BuscarCaminhoDFS(atual, grafo.Chefe) : null;
                RenderizadorConsole.ExibirMapa(
                    grafo,
                    jogador: atual,
                    solucao: dica,
                    mostrarVisitados: false,
                    mostrarArmadilhas: false,
                    tesourosColetados: tesourosColetados);

                RenderizadorConsole.ExibirLegenda(incluirBusca: bussolaAtiva, incluirJogador: true);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n> {mensagem}");
                Console.ResetColor();

                if (atual == grafo.Chefe)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nVITORIA! Voce alcancou o Chefe no vertice mais profundo da masmorra!");
                    Console.ResetColor();
                    Console.WriteLine($"Passos dados: {passos} | Distancia exata pelo DFS: {caminhoIdealInicial.Caminho.Count - 1} arestas.");
                    Console.WriteLine($"Pontuacao final: {pontos + (vida * 50)} pts.");
                    Console.Write("\nPressione qualquer tecla para voltar ao menu...");
                    Console.ReadKey(true);
                    return;
                }

                if (vida <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nDERROTA! Voce perdeu todas as vidas nas armadilhas das ramificacoes.");
                    Console.ResetColor();
                    Console.Write("\nPressione qualquer tecla para voltar ao menu...");
                    Console.ReadKey(true);
                    return;
                }

                var tecla = Console.ReadKey(true).Key;
                if (tecla == ConsoleKey.Escape || tecla == ConsoleKey.Q)
                    return;

                if (tecla == ConsoleKey.H)
                {
                    bussolaAtiva = !bussolaAtiva;
                    mensagem = bussolaAtiva
                        ? "Bussola DFS ativada! Caminho calculado em tempo real pela Lista de Adjacencia."
                        : "Bussola DFS desativada.";
                    continue;
                }

                int dL = 0, dC = 0;
                if (tecla == ConsoleKey.W || tecla == ConsoleKey.UpArrow) dL = -1;
                else if (tecla == ConsoleKey.S || tecla == ConsoleKey.DownArrow) dL = 1;
                else if (tecla == ConsoleKey.A || tecla == ConsoleKey.LeftArrow) dC = -1;
                else if (tecla == ConsoleKey.D || tecla == ConsoleKey.RightArrow) dC = 1;
                else continue;

                Vertice? destino = grafo.ObterVertice(atual.Linha + dL, atual.Coluna + dC);

                if (destino != null && grafo.SaoAdjacentes(atual, destino))
                {
                    atual = destino;
                    passos++;
                    mensagem = $"Moveu para {atual} (Profundidade: {atual.Profundidade}).";

                    if (atual.Conteudo == ConteudoSala.Tesouro && !tesourosColetados.Contains(atual))
                    {
                        tesourosColetados.Add(atual);
                        pontos += 100;
                        mensagem = "Voce encontrou um Tesouro no beco sem saida! (+100 pontos)";
                    }
                    else if (atual.Conteudo == ConteudoSala.Armadilha && !armadilhasAtivadas.Contains(atual))
                    {
                        armadilhasAtivadas.Add(atual);
                        vida--;
                        mensagem = "CUIDADO! Voce caiu em uma armadilha fora da rota principal! (-1 vida)";
                    }
                }
                else
                {
                    mensagem = "Parede bloqueando! Nao existe aresta conectando esses dois vertices.";
                }
            }
        }
    }

    class Program
    {
        private static int _linhasAtual = 10;
        private static int _colunasAtual = 18;
        private static int _velocidadeAnimacaoMs = 25;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Gerador e Analisador de Masmorras com DFS em Grafos";

            var grafoAtual = new GrafoMasmorra(_linhasAtual, _colunasAtual);
            grafoAtual.GerarComDFSIterativo();

            while (true)
            {
                Console.Clear();
                ExibirCabecalho(grafoAtual);
                RenderizadorConsole.ExibirMapa(grafoAtual);
                RenderizadorConsole.ExibirLegenda();

                Console.WriteLine("\n------------------------- MENU PRINCIPAL -------------------------");
                Console.WriteLine(" [1] Gerar nova masmorra (aleatoria ou com tamanho/seed)");
                Console.WriteLine(" [2] Assistir geracao passo a passo (Animacao DFS + Backtracking)");
                Console.WriteLine(" [3] Resolver masmorra com DFS (Entrada -> Chefe + Backtracking)");
                Console.WriteLine(" [4] Jogar / Explorar a masmorra (com Bussola DFS)");
                Console.WriteLine(" [5] Inspecionar Grafo (Lista de Adjacencia e Vertice especifico)");
                Console.WriteLine(" [6] Estatisticas completas de Teoria dos Grafos");
                Console.WriteLine(" [7] Exportar mapa e relatorio completo para arquivo .TXT");
                Console.WriteLine(" [8] Configuracoes (Velocidade animacao / Blocos Unicode ou ASCII)");
                Console.WriteLine(" [0] Sair");
                Console.WriteLine("------------------------------------------------------------------");
                Console.Write("Escolha uma opcao: ");

                string? entrada = Console.ReadLine()?.Trim();

                switch (entrada)
                {
                    case "1":
                        grafoAtual = MenuGerarNovaMasmorra();
                        break;
                    case "2":
                        grafoAtual = ExecutarGeracaoAnimada(_linhasAtual, _colunasAtual);
                        break;
                    case "3":
                        ExibirResolucaoDFS(grafoAtual);
                        break;
                    case "4":
                        ModoExploracao.Executar(grafoAtual);
                        break;
                    case "5":
                        MenuInspecionarGrafo(grafoAtual);
                        break;
                    case "6":
                        ExibirEstatisticasCompletas(grafoAtual);
                        break;
                    case "7":
                        ExportarArquivo(grafoAtual);
                        break;
                    case "8":
                        MenuConfiguracoes();
                        break;
                    case "0":
                        Console.WriteLine("\nEncerrando sistema...");
                        return;
                    default:
                        break;
                }
            }
        }

        private static void ExibirCabecalho(GrafoMasmorra grafo)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================");
            Console.WriteLine("     GERADOR PROCEDURAL DE MASMORRAS - BUSCA EM PROFUNDIDADE");
            Console.WriteLine("==================================================================");
            Console.ResetColor();
            Console.WriteLine($"Grafo: {grafo.Linhas}x{grafo.Colunas} | Vertices |V|={grafo.Linhas * grafo.Colunas} | Arestas |E|={grafo.TotalArestas} | Seed: {grafo.Seed} | Prof. Max: {grafo.ProfundidadeMaxima}\n");
        }

        private static GrafoMasmorra MenuGerarNovaMasmorra()
        {
            Console.Clear();
            Console.WriteLine("=== CONFIGURAR NOVA MASMORRA ===\n");
            Console.WriteLine(" [1] Pequena   (7 x 11)  - 77 vertices");
            Console.WriteLine(" [2] Media     (10 x 18) - 180 vertices");
            Console.WriteLine(" [3] Grande    (14 x 24) - 336 vertices");
            Console.WriteLine(" [4] Tamanho personalizado");
            Console.WriteLine(" [5] Informar uma Seed especifica (reproduzir mapa anterior)");
            Console.Write("\nOpcao (Enter para manter tamanho atual): ");

            string? escolha = Console.ReadLine()?.Trim();
            int? seedEscolhida = null;

            if (escolha == "1") { _linhasAtual = 7; _colunasAtual = 11; }
            else if (escolha == "2") { _linhasAtual = 10; _colunasAtual = 18; }
            else if (escolha == "3") { _linhasAtual = 14; _colunasAtual = 24; }
            else if (escolha == "4")
            {
                _linhasAtual = LerInteiro("Linhas (4 a 18): ", 4, 18, _linhasAtual);
                _colunasAtual = LerInteiro("Colunas (4 a 28): ", 4, 28, _colunasAtual);
            }
            else if (escolha == "5")
            {
                int seedDigitada = LerInteiro("Digite a Seed numerica (ex: 482910): ", 1, int.MaxValue, 123456);
                seedEscolhida = seedDigitada;
            }

            var novoGrafo = new GrafoMasmorra(_linhasAtual, _colunasAtual, seedEscolhida);
            novoGrafo.GerarComDFSIterativo();
            return novoGrafo;
        }

        private static GrafoMasmorra ExecutarGeracaoAnimada(int linhas, int colunas)
        {
            var novoGrafo = new GrafoMasmorra(linhas, colunas);
            Console.Clear();
            bool cursorAnterior = true;

            try
            {
                cursorAnterior = Console.CursorVisible;
                Console.CursorVisible = false;
            }
            catch { }

            int passosAnimacao = 0;

            novoGrafo.GerarComDFSIterativo(
                animado: true,
                delayMs: _velocidadeAnimacaoMs,
                aoAtualizarFrame: (g, atual, pilha, fazendoBacktrack) =>
                {
                    passosAnimacao++;
                    try { Console.SetCursorPosition(0, 0); }
                    catch { Console.Clear(); }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("=== ANIMACAO AO VIVO: DFS ITERATIVO COM BACKTRACKING ===");
                    Console.ResetColor();

                    Console.Write($"Passo: {passosAnimacao,4} | Arestas: {g.TotalArestas,3}/{g.Linhas * g.Colunas - 1} | Pilha DFS: {pilha.Count,3} | Acao: ");
                    if (fazendoBacktrack)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("BACKTRACKING (voltando)   ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("AVANCANDO (abrindo aresta)");
                    }
                    Console.ResetColor();
                    Console.WriteLine("\nPressione qualquer tecla para pular a animacao...\n");

                    RenderizadorConsole.ExibirMapa(g, cursorAnimacao: atual, mostrarArmadilhas: false);
                });

            try { Console.CursorVisible = cursorAnterior; } catch { }

            Console.Clear();
            ExibirCabecalho(novoGrafo);
            RenderizadorConsole.ExibirMapa(novoGrafo);
            RenderizadorConsole.ExibirLegenda();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nGeracao concluida! Total de backtracks realizados: {novoGrafo.TotalBacktracksGeracao}.");
            Console.ResetColor();
            Pausar();

            return novoGrafo;
        }

        private static void ExibirResolucaoDFS(GrafoMasmorra grafo)
        {
            Console.Clear();
            var resultado = grafo.BuscarCaminhoDFS(grafo.Entrada, grafo.Chefe);

            ExibirCabecalho(grafo);
            RenderizadorConsole.ExibirMapa(grafo, solucao: resultado, mostrarVisitados: true);
            RenderizadorConsole.ExibirLegenda(incluirBusca: true);

            Console.WriteLine("\n--- ANALISE DA BUSCA EM PROFUNDIDADE (ENTRADA -> CHEFE) ---");
            Console.WriteLine($"Origem: {grafo.Entrada}  --->  Destino (Chefe): {grafo.Chefe}");
            Console.WriteLine($"Comprimento do caminho encontrado:   {resultado.Caminho.Count - 1} arestas ({resultado.Caminho.Count} vertices)");
            Console.WriteLine($"Total de vertices visitados no DFS:  {resultado.Visitados.Count} de {grafo.Linhas * grafo.Colunas}");
            Console.WriteLine($"Vertices explorados em becos [.]:    {resultado.SalasDescartadas}");
            Console.WriteLine($"Quantidade de backtracks na busca:   {resultado.TotalBacktracks}");

            Console.WriteLine("\nSequencia dos primeiros vertices do caminho:");
            var resumoCaminho = resultado.Caminho.Take(12).Select(v => v.ToString());
            Console.WriteLine(string.Join(" -> ", resumoCaminho) + (resultado.Caminho.Count > 12 ? " -> ..." : ""));

            Pausar();
        }

        private static void MenuInspecionarGrafo(GrafoMasmorra grafo)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== INSPETOR DO GRAFO (LISTA DE ADJACENCIA) ===\n");
                Console.WriteLine(" [1] Ver Lista de Adjacencia de uma faixa de vertices");
                Console.WriteLine(" [2] Consultar um vertice especifico por (linha, coluna)");
                Console.WriteLine(" [3] Ver ordem de descoberta DFS (primeiros 25 vertices)");
                Console.WriteLine(" [0] Voltar ao menu principal");
                Console.Write("\nOpcao: ");

                string? op = Console.ReadLine()?.Trim();
                if (op == "0") return;

                if (op == "1")
                {
                    int inicio = LerInteiro($"ID inicial (0 a {grafo.Linhas * grafo.Colunas - 1}): ", 0, grafo.Linhas * grafo.Colunas - 1, 0);
                    int quantidade = LerInteiro("Quantos vertices listar (1 a 40): ", 1, 40, 20);

                    Console.WriteLine("\nVertice       | Grau | Prof | d[v] | f[v] | Vizinhos Conectados (Arestas)");
                    Console.WriteLine("--------------+------+------+------+------+---------------------------------------");

                    foreach (var par in grafo.ListaAdjacencia.OrderBy(p => p.Key.Id).Skip(inicio).Take(quantidade))
                    {
                        string vizinhos = string.Join(", ", par.Value.Select(v => v.ToString()));
                        Console.WriteLine($"{par.Key,-13} | {par.Value.Count,4} | {par.Key.Profundidade,4} | {par.Key.TempoDescoberta,4} | {par.Key.TempoFinalizacao,4} | {vizinhos}");
                    }
                    Pausar();
                }
                else if (op == "2")
                {
                    int l = LerInteiro($"Linha (0 a {grafo.Linhas - 1}): ", 0, grafo.Linhas - 1, 0);
                    int c = LerInteiro($"Coluna (0 a {grafo.Colunas - 1}): ", 0, grafo.Colunas - 1, 0);
                    Vertice v = grafo.Grade[l, c];
                    var vizinhos = grafo.ListaAdjacencia[v];

                    Console.WriteLine($"\n--- DETALHES DO VERTICE {v} ---");
                    Console.WriteLine($"Conteudo da sala:        {v.Conteudo}");
                    Console.WriteLine($"Grau do vertice:         {vizinhos.Count} {(vizinhos.Count == 1 ? "(Folha / Beco sem saida)" : "")}");
                    Console.WriteLine($"Profundidade (raiz E):   {v.Profundidade}");
                    Console.WriteLine($"Tempo de descoberta d:   {v.TempoDescoberta}");
                    Console.WriteLine($"Tempo de finalizacao f:  {v.TempoFinalizacao}");
                    Console.WriteLine($"Vizinhos adjacentes:     {string.Join(", ", vizinhos)}");

                    var rotaAteAqui = grafo.BuscarCaminhoDFS(grafo.Entrada, v);
                    Console.WriteLine($"Distancia da Entrada:    {rotaAteAqui.Caminho.Count - 1} arestas");
                    Pausar();
                }
                else if (op == "3")
                {
                    Console.WriteLine("\n--- ORDEM DE DESCOBERTA DO DFS RECURSIVO (A PARTIR DA ENTRADA) ---\n");
                    var primeiros = grafo.OrdemDescoberta.Take(25).Select(v => $"{v}[d={v.TempoDescoberta},f={v.TempoFinalizacao}]");
                    Console.WriteLine(string.Join(" -> ", primeiros));
                    if (grafo.OrdemDescoberta.Count > 25)
                        Console.WriteLine($"\n... e mais {grafo.OrdemDescoberta.Count - 25} vertices.");
                    Pausar();
                }
            }
        }

        private static void ExibirEstatisticasCompletas(GrafoMasmorra grafo)
        {
            Console.Clear();
            var stats = grafo.CalcularEstatisticas();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================");
            Console.WriteLine("            ESTATISTICAS DE TEORIA DOS GRAFOS (DFS)");
            Console.WriteLine("==================================================================\n");
            Console.ResetColor();

            Console.WriteLine($"Dimensoes da Grade:              {grafo.Linhas} x {grafo.Colunas} (Seed: {grafo.Seed})");
            Console.WriteLine($"Total de Vertices |V|:           {stats.TotalVertices}");
            Console.WriteLine($"Total de Arestas  |E|:           {stats.TotalArestas}");
            Console.WriteLine($"Tipo de Grafo:                   Nao-direcionado, Nao-ponderado, Conexo");
            Console.WriteLine($"Propriedade Arvore Geradora:     {(stats.EhArvoreGeradora ? "VALIDA (|E| = |V| - 1, sem ciclos)" : "INVALIDA")}");
            Console.WriteLine($"Grau Minimo dos Vertices:        {stats.GrauMinimo}");
            Console.WriteLine($"Grau Medio dos Vertices:         {stats.GrauMedio:F2}");
            Console.WriteLine($"Grau Maximo dos Vertices:        {stats.GrauMaximo}");
            Console.WriteLine($"Becos sem Saida (Nos Folha):     {stats.TotalFolhasBecos} ({(double)stats.TotalFolhasBecos / stats.TotalVertices * 100:F1}% do grafo)");
            Console.WriteLine($"Profundidade Maxima do DFS:      {stats.ProfundidadeMaxima} passos (onde esta o Chefe {grafo.Chefe})");
            Console.WriteLine($"Backtracks na Geracao:           {stats.BacktracksNaGeracao}");
            Console.WriteLine($"Tempo Finalizacao da Raiz f[E]:  {stats.TempoFinalizacaoRaiz} (propriedade DFS: 2 * |V| = {stats.TotalVertices * 2})");
            Console.WriteLine($"Itens gerados:                   {stats.TotalTesouros} Tesouros | {stats.TotalArmadilhas} Armadilhas");

            Console.WriteLine("\nObservacao teorica:");
            Console.WriteLine("Como o DFS nunca abre aresta para um vertice ja visitado, o grafo final");
            Console.WriteLine("nao possui ciclos e garante um caminho unico entre quaisquer duas salas.");

            Pausar();
        }

        private static void ExportarArquivo(GrafoMasmorra grafo)
        {
            string nomeArquivo = $"masmorra_seed_{grafo.Seed}.txt";
            try
            {
                string caminhoCompleto = grafo.ExportarRelatorioTxt(nomeArquivo);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nRelatorio exportado com sucesso!");
                Console.ResetColor();
                Console.WriteLine($"Arquivo salvo em: {caminhoCompleto}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nErro ao exportar arquivo: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void MenuConfiguracoes()
        {
            Console.Clear();
            Console.WriteLine("=== CONFIGURACOES ===\n");
            Console.WriteLine($" [1] Alterar velocidade da animacao (atual: {_velocidadeAnimacaoMs} ms)");
            Console.WriteLine($" [2] Alternar estilo das paredes (atual: {(RenderizadorConsole.UsarBlocosUnicode ? "Blocos Unicode [██]" : "ASCII Puro [##]")})");
            Console.WriteLine(" [0] Voltar");
            Console.Write("\nOpcao: ");

            string? op = Console.ReadLine()?.Trim();
            if (op == "1")
            {
                _velocidadeAnimacaoMs = LerInteiro("Digite o atraso em milissegundos (5 = rapido, 80 = lento): ", 5, 300, _velocidadeAnimacaoMs);
            }
            else if (op == "2")
            {
                RenderizadorConsole.UsarBlocosUnicode = !RenderizadorConsole.UsarBlocosUnicode;
            }
        }

        private static int LerInteiro(string prompt, int min, int max, int padrao)
        {
            Console.Write(prompt);
            string? texto = Console.ReadLine()?.Trim();
            if (int.TryParse(texto, out int valor))
                return Math.Clamp(valor, min, max);

            return padrao;
        }

        private static void Pausar()
        {
            Console.Write("\nPressione qualquer tecla para continuar...");
            Console.ReadKey(true);
        }
    }
}