using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundGraph
{
    public enum TipoRelacionamento
    {
        AfinidadeSonora = 1,
        MesmoArtista = 2,
        MesmoAlbum = 3,
        MesmoGenero = 4,
        ColaboracaoFeat = 5,
        TransicaoFila = 6,
        PlaylistTreino = 7,
        PlaylistFoco = 8,
        PlaylistFesta = 9,
        SampleOuCover = 10,
        BpmCompativel = 11,
        RadioLoop = 12,
        ModoRepeat = 13,
        Personalizado = 14
    }

    public static class RelacionamentoHelper
    {
        public static string ObterNome(TipoRelacionamento tipo) => tipo switch
        {
            TipoRelacionamento.AfinidadeSonora => "Afinidade Sonora",
            TipoRelacionamento.MesmoArtista => "Mesmo Artista",
            TipoRelacionamento.MesmoAlbum => "Mesmo Álbum",
            TipoRelacionamento.MesmoGenero => "Mesmo Gênero",
            TipoRelacionamento.ColaboracaoFeat => "Colaboração / Feat",
            TipoRelacionamento.TransicaoFila => "Transição de Fila",
            TipoRelacionamento.PlaylistTreino => "Playlist Treino",
            TipoRelacionamento.PlaylistFoco => "Playlist Foco",
            TipoRelacionamento.PlaylistFesta => "Playlist Festa",
            TipoRelacionamento.SampleOuCover => "Sample / Cover",
            TipoRelacionamento.BpmCompativel => "BPM Compatível",
            TipoRelacionamento.RadioLoop => "Rádio Loop",
            TipoRelacionamento.ModoRepeat => "Modo Repeat (Laço)",
            TipoRelacionamento.Personalizado => "Personalizado",
            _ => "Conexão"
        };
    }

    public class FaixaMusical
    {
        public string Titulo { get; }
        public string Artista { get; }
        public string Genero { get; }
        public string Album { get; }
        public int Ano { get; }
        public int Bpm { get; }

        public FaixaMusical(
            string titulo,
            string artista,
            string genero,
            string album = "Single",
            int ano = 2020,
            int bpm = 120)
        {
            Titulo = titulo;
            Artista = artista;
            Genero = genero;
            Album = album;
            Ano = ano;
            Bpm = bpm;
        }

        public override string ToString() => $"{Titulo} - {Artista} [{Genero} | {Album} ({Ano}) | {Bpm} BPM]";
    }

    public class ConexaoMusical
    {
        public string Destino { get; }
        public int Peso { get; }
        public TipoRelacionamento Tipo { get; }
        public string Contexto { get; }

        public ConexaoMusical(
            string destino,
            int peso = 1,
            TipoRelacionamento tipo = TipoRelacionamento.AfinidadeSonora,
            string? contextoPersonalizado = null)
        {
            Destino = destino;
            Peso = peso;
            Tipo = tipo;
            Contexto = string.IsNullOrWhiteSpace(contextoPersonalizado)
                ? RelacionamentoHelper.ObterNome(tipo)
                : contextoPersonalizado.Trim();
        }
    }

    public class DiagnosticoGrafo
    {
        public int TotalVertices { get; init; }
        public int TotalArestas { get; init; }
        public bool EhDirecionado { get; init; }
        public bool EhPonderado { get; init; }
        public bool PossuiLacos { get; init; }
        public bool PossuiArestasParalelas { get; init; }
        public string TipoEstrutural { get; init; } = "";
        public string Conectividade { get; init; } = "";
        public int ComponentesConexas { get; init; }
        public bool PossuiCiclo { get; init; }
        public bool EhCompleto { get; init; }
        public double GrauMedio { get; init; }
        public double DensidadePercentual { get; init; }
        public Dictionary<TipoRelacionamento, int> ContagemPorTipo { get; init; } = new();
    }

    public class GrafoMusical
    {
        public string NomeCenario { get; }
        public bool EhDirecionado { get; }
        public bool EhPonderado { get; }

        private readonly Dictionary<string, FaixaMusical> _faixas =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, List<ConexaoMusical>> _listaAdjacencia =
            new(StringComparer.OrdinalIgnoreCase);

        public GrafoMusical(string nomeCenario, bool ehDirecionado, bool ehPonderado)
        {
            NomeCenario = nomeCenario;
            EhDirecionado = ehDirecionado;
            EhPonderado = ehPonderado;
        }

        public IReadOnlyList<FaixaMusical> ObterFaixasOrdenadas()
        {
            return _faixas.Values
                .OrderBy(f => f.Titulo, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string? ObterTituloCanonico(string titulo)
        {
            string limpo = titulo.Trim();
            foreach (string chave in _faixas.Keys)
            {
                if (chave.Equals(limpo, StringComparison.OrdinalIgnoreCase))
                    return chave;
            }
            return null;
        }

        public FaixaMusical? ObterFaixa(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            return canonico is null ? null : _faixas[canonico];
        }

        public bool AdicionarFaixa(
            string titulo,
            string artista,
            string genero,
            string album = "Single",
            int ano = 2020,
            int bpm = 120)
        {
            string tituloLimpo = titulo.Trim();
            if (string.IsNullOrWhiteSpace(tituloLimpo) || _faixas.ContainsKey(tituloLimpo))
                return false;

            string artistaLimpo = string.IsNullOrWhiteSpace(artista) ? "Desconhecido" : artista.Trim();
            string generoLimpo = string.IsNullOrWhiteSpace(genero) ? "Geral" : genero.Trim();
            string albumLimpo = string.IsNullOrWhiteSpace(album) ? "Single" : album.Trim();
            int anoValido = ano is < 1900 or > 2100 ? 2020 : ano;
            int bpmValido = Math.Clamp(bpm, 40, 260);

            _faixas[tituloLimpo] = new FaixaMusical(tituloLimpo, artistaLimpo, generoLimpo, albumLimpo, anoValido, bpmValido);
            _listaAdjacencia[tituloLimpo] = new List<ConexaoMusical>();
            return true;
        }

        public bool RemoverFaixa(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return false;

            _faixas.Remove(canonico);
            _listaAdjacencia.Remove(canonico);

            foreach (List<ConexaoMusical> conexoes in _listaAdjacencia.Values)
            {
                conexoes.RemoveAll(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        public bool ExisteConexaoMesmoTipo(string origem, string destino, TipoRelacionamento tipo)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);

            if (orig is null || dest is null)
                return false;

            return _listaAdjacencia[orig].Any(c =>
                c.Destino.Equals(dest, StringComparison.OrdinalIgnoreCase) && c.Tipo == tipo);
        }

        public (bool Sucesso, string Mensagem) AdicionarConexao(
            string origem,
            string destino,
            int peso = 1,
            TipoRelacionamento tipo = TipoRelacionamento.AfinidadeSonora,
            string? contextoPersonalizado = null)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);

            if (orig is null || dest is null)
                return (false, "Erro: uma ou ambas as músicas não estão cadastradas.");

            bool ehLaco = orig.Equals(dest, StringComparison.OrdinalIgnoreCase);
            int pesoFinal = EhPonderado ? Math.Clamp(peso, 1, 100) : 1;
            TipoRelacionamento tipoFinal = ehLaco && tipo == TipoRelacionamento.AfinidadeSonora
                ? TipoRelacionamento.ModoRepeat
                : tipo;

            var novaConexao = new ConexaoMusical(dest, pesoFinal, tipoFinal, contextoPersonalizado);
            _listaAdjacencia[orig].Add(novaConexao);

            if (!EhDirecionado && !ehLaco)
            {
                _listaAdjacencia[dest].Add(new ConexaoMusical(orig, pesoFinal, tipoFinal, contextoPersonalizado));
            }

            string seta = EhDirecionado ? "→" : "↔";
            string textoPeso = EhPonderado ? $" [Peso: {pesoFinal}%]" : "";
            return (true, $"Conexão criada: {orig} {seta} {dest} ({novaConexao.Contexto}){textoPeso}");
        }

        public bool RemoverConexao(string origem, string destino, TipoRelacionamento? filtroTipo = null)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);

            if (orig is null || dest is null)
                return false;

            List<ConexaoMusical> listaOrig = _listaAdjacencia[orig];
            int idx = listaOrig.FindIndex(c =>
                c.Destino.Equals(dest, StringComparison.OrdinalIgnoreCase) &&
                (!filtroTipo.HasValue || c.Tipo == filtroTipo.Value));

            if (idx == -1)
                return false;

            ConexaoMusical removida = listaOrig[idx];
            listaOrig.RemoveAt(idx);

            if (!EhDirecionado && !orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
            {
                List<ConexaoMusical> listaDest = _listaAdjacencia[dest];
                int idxReverso = listaDest.FindIndex(c =>
                    c.Destino.Equals(orig, StringComparison.OrdinalIgnoreCase) &&
                    c.Peso == removida.Peso &&
                    c.Tipo == removida.Tipo &&
                    c.Contexto.Equals(removida.Contexto, StringComparison.OrdinalIgnoreCase));

                if (idxReverso == -1)
                    idxReverso = listaDest.FindIndex(c => c.Destino.Equals(orig, StringComparison.OrdinalIgnoreCase));

                if (idxReverso != -1)
                    listaDest.RemoveAt(idxReverso);
            }

            return true;
        }

        public int AutoConectarPorRegra(int tipoRegra)
        {
            IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();
            int criadas = 0;

            for (int i = 0; i < faixas.Count; i++)
            {
                for (int j = i + 1; j < faixas.Count; j++)
                {
                    FaixaMusical a = faixas[i];
                    FaixaMusical b = faixas[j];

                    if (tipoRegra == 1 && a.Genero.Equals(b.Genero, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ExisteConexaoMesmoTipo(a.Titulo, b.Titulo, TipoRelacionamento.MesmoGenero))
                        {
                            AdicionarConexao(a.Titulo, b.Titulo, 78, TipoRelacionamento.MesmoGenero);
                            criadas++;
                        }
                    }
                    else if (tipoRegra == 2 && a.Artista.Equals(b.Artista, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ExisteConexaoMesmoTipo(a.Titulo, b.Titulo, TipoRelacionamento.MesmoArtista))
                        {
                            AdicionarConexao(a.Titulo, b.Titulo, 92, TipoRelacionamento.MesmoArtista);
                            criadas++;
                        }
                    }
                    else if (tipoRegra == 3 && Math.Abs(a.Bpm - b.Bpm) <= 8)
                    {
                        FaixaMusical origem = EhDirecionado && a.Bpm > b.Bpm ? b : a;
                        FaixaMusical destino = EhDirecionado && a.Bpm > b.Bpm ? a : b;
                        int afinidadeBpm = Math.Clamp(100 - Math.Abs(a.Bpm - b.Bpm) * 3, 65, 98);

                        if (!ExisteConexaoMesmoTipo(origem.Titulo, destino.Titulo, TipoRelacionamento.BpmCompativel))
                        {
                            AdicionarConexao(origem.Titulo, destino.Titulo, afinidadeBpm, TipoRelacionamento.BpmCompativel);
                            criadas++;
                        }
                    }
                }
            }

            return criadas;
        }

        public List<(string Origem, ConexaoMusical Conexao)> FiltrarConexoesPorTipo(TipoRelacionamento tipo)
        {
            var resultado = new List<(string, ConexaoMusical)>();
            var paresRegistrados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (FaixaMusical faixa in ObterFaixasOrdenadas())
            {
                foreach (ConexaoMusical conexao in _listaAdjacencia[faixa.Titulo])
                {
                    if (conexao.Tipo != tipo)
                        continue;

                    if (!EhDirecionado && !faixa.Titulo.Equals(conexao.Destino, StringComparison.OrdinalIgnoreCase))
                    {
                        string menor = string.Compare(faixa.Titulo, conexao.Destino, StringComparison.OrdinalIgnoreCase) <= 0
                            ? faixa.Titulo
                            : conexao.Destino;
                        string maior = menor.Equals(faixa.Titulo, StringComparison.OrdinalIgnoreCase)
                            ? conexao.Destino
                            : faixa.Titulo;

                        string chave = $"{menor}|{maior}|{conexao.Peso}|{conexao.Contexto}";
                        if (!paresRegistrados.Add(chave))
                            continue;
                    }

                    resultado.Add((faixa.Titulo, conexao));
                }
            }

            return resultado;
        }

        public int ContarArestas()
        {
            if (EhDirecionado)
                return _listaAdjacencia.Values.Sum(lista => lista.Count);

            int lacos = 0;
            int naoLacos = 0;

            foreach (var par in _listaAdjacencia)
            {
                foreach (ConexaoMusical conexao in par.Value)
                {
                    if (conexao.Destino.Equals(par.Key, StringComparison.OrdinalIgnoreCase))
                        lacos++;
                    else
                        naoLacos++;
                }
            }

            return (naoLacos / 2) + lacos;
        }

        public Dictionary<TipoRelacionamento, int> ContarPorTipoRelacionamento()
        {
            var totais = new Dictionary<TipoRelacionamento, int>();

            foreach (TipoRelacionamento tipo in Enum.GetValues<TipoRelacionamento>())
            {
                int qtd = FiltrarConexoesPorTipo(tipo).Count;
                if (qtd > 0)
                    totais[tipo] = qtd;
            }

            return totais;
        }

        public int ObterGrauNaoDirecionado(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return 0;

            int grau = 0;
            foreach (ConexaoMusical conexao in _listaAdjacencia[canonico])
            {
                grau += conexao.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase) ? 2 : 1;
            }
            return grau;
        }

        public int ObterGrauSaida(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            return canonico is null ? 0 : _listaAdjacencia[canonico].Count;
        }

        public int ObterGrauEntrada(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return 0;

            return _listaAdjacencia.Values.Sum(lista =>
                lista.Count(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase)));
        }

        public bool PossuiLacos()
        {
            return _listaAdjacencia.Any(par =>
                par.Value.Any(c => c.Destino.Equals(par.Key, StringComparison.OrdinalIgnoreCase)));
        }

        public bool PossuiArestasParalelas()
        {
            return _listaAdjacencia.Values.Any(lista =>
                lista.GroupBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                     .Any(grupo => grupo.Count() > 1));
        }

        public bool EhCompleto()
        {
            int n = _faixas.Count;
            if (n < 2)
                return false;

            if (PossuiLacos() || PossuiArestasParalelas())
                return false;

            return _listaAdjacencia.Values.All(lista =>
                lista.Select(c => c.Destino)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Count() == n - 1);
        }

        public (string Classificacao, int Componentes) VerificarConectividade()
        {
            int n = _faixas.Count;
            if (n == 0)
                return ("Grafo Vazio", 0);
            if (n == 1)
                return ("Conexo (trivial - 1 vértice)", 1);

            var subjacente = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (string v in _faixas.Keys)
                subjacente[v] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var par in _listaAdjacencia)
            {
                foreach (ConexaoMusical c in par.Value)
                {
                    subjacente[par.Key].Add(c.Destino);
                    subjacente[c.Destino].Add(par.Key);
                }
            }

            var visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int componentes = 0;

            foreach (string vertice in _faixas.Keys)
            {
                if (visitados.Contains(vertice))
                    continue;

                componentes++;
                var fila = new Queue<string>();
                fila.Enqueue(vertice);
                visitados.Add(vertice);

                while (fila.Count > 0)
                {
                    string atual = fila.Dequeue();
                    foreach (string vizinho in subjacente[atual])
                    {
                        if (visitados.Add(vizinho))
                            fila.Enqueue(vizinho);
                    }
                }
            }

            if (!EhDirecionado)
                return componentes == 1 ? ("Conexo", 1) : ("Desconexo", componentes);

            if (componentes > 1)
                return ("Desconexo", componentes);

            string inicio = _faixas.Keys.First();
            bool alcancaTodosOriginal = BfsDirecionado(inicio, _listaAdjacencia).Count == n;

            var transposto = new Dictionary<string, List<ConexaoMusical>>(StringComparer.OrdinalIgnoreCase);
            foreach (string v in _faixas.Keys)
                transposto[v] = new List<ConexaoMusical>();

            foreach (var par in _listaAdjacencia)
            {
                foreach (ConexaoMusical c in par.Value)
                    transposto[c.Destino].Add(new ConexaoMusical(par.Key, c.Peso, c.Tipo, c.Contexto));
            }

            bool alcancaTodosTransposto = BfsDirecionado(inicio, transposto).Count == n;

            return (alcancaTodosOriginal && alcancaTodosTransposto)
                ? ("Fortemente Conexo", 1)
                : ("Fracamente Conexo", 1);
        }

        private static HashSet<string> BfsDirecionado(string inicio, Dictionary<string, List<ConexaoMusical>> adjacencia)
        {
            var visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { inicio };
            var fila = new Queue<string>();
            fila.Enqueue(inicio);

            while (fila.Count > 0)
            {
                string atual = fila.Dequeue();
                foreach (ConexaoMusical c in adjacencia[atual])
                {
                    if (visitados.Add(c.Destino))
                        fila.Enqueue(c.Destino);
                }
            }

            return visitados;
        }

        public bool PossuiCiclo()
        {
            if (PossuiLacos())
                return true;

            if (!EhDirecionado)
            {
                if (PossuiArestasParalelas())
                    return true;

                var visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string v in _faixas.Keys)
                {
                    if (!visitados.Contains(v) && DfsCicloNaoDirecionado(v, null, visitados))
                        return true;
                }
                return false;
            }
            else
            {
                var estado = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (string v in _faixas.Keys)
                {
                    if (estado.GetValueOrDefault(v, 0) == 0 && DfsCicloDirecionado(v, estado))
                        return true;
                }
                return false;
            }
        }

        private bool DfsCicloNaoDirecionado(string atual, string? pai, HashSet<string> visitados)
        {
            visitados.Add(atual);

            foreach (ConexaoMusical c in _listaAdjacencia[atual])
            {
                if (!visitados.Contains(c.Destino))
                {
                    if (DfsCicloNaoDirecionado(c.Destino, atual, visitados))
                        return true;
                }
                else if (pai is null || !c.Destino.Equals(pai, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool DfsCicloDirecionado(string atual, Dictionary<string, int> estado)
        {
            estado[atual] = 1;

            foreach (ConexaoMusical c in _listaAdjacencia[atual])
            {
                int estadoVizinho = estado.GetValueOrDefault(c.Destino, 0);
                if (estadoVizinho == 1)
                    return true;

                if (estadoVizinho == 0 && DfsCicloDirecionado(c.Destino, estado))
                    return true;
            }

            estado[atual] = 2;
            return false;
        }

        public DiagnosticoGrafo GerarDiagnostico()
        {
            bool lacos = PossuiLacos();
            bool paralelas = PossuiArestasParalelas();

            string tipoEstrutural;
            if (lacos && paralelas)
                tipoEstrutural = "Pseudografo (possui auto-laço/Repeat e arestas paralelas)";
            else if (lacos)
                tipoEstrutural = "Pseudografo (possui auto-laço / modo Repeat)";
            else if (paralelas)
                tipoEstrutural = "Multigrafo (possui múltiplos relacionamentos entre o mesmo par)";
            else
                tipoEstrutural = "Grafo Simples (sem laços e sem arestas paralelas)";

            var (conectividade, componentes) = VerificarConectividade();
            int n = _faixas.Count;
            int e = ContarArestas();

            double somaGraus = EhDirecionado
                ? _faixas.Keys.Sum(v => ObterGrauEntrada(v) + ObterGrauSaida(v))
                : _faixas.Keys.Sum(ObterGrauNaoDirecionado);

            double grauMedio = n == 0 ? 0 : somaGraus / n;
            double maxArestasSimples = n < 2 ? 0 : (EhDirecionado ? (double)n * (n - 1) : (double)n * (n - 1) / 2.0);
            double densidade = maxArestasSimples == 0 ? 0 : (e / maxArestasSimples) * 100.0;

            return new DiagnosticoGrafo
            {
                TotalVertices = n,
                TotalArestas = e,
                EhDirecionado = EhDirecionado,
                EhPonderado = EhPonderado,
                PossuiLacos = lacos,
                PossuiArestasParalelas = paralelas,
                TipoEstrutural = tipoEstrutural,
                Conectividade = conectividade,
                ComponentesConexas = componentes,
                PossuiCiclo = PossuiCiclo(),
                EhCompleto = EhCompleto(),
                GrauMedio = grauMedio,
                DensidadePercentual = densidade,
                ContagemPorTipo = ContarPorTipoRelacionamento()
            };
        }

        public (List<ConexaoMusical> Diretas, List<(string Titulo, int Pontuacao)> Descobertas)
            RecomendarAPartirDe(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return (new List<ConexaoMusical>(), new List<(string, int)>());

            List<ConexaoMusical> diretas = _listaAdjacencia[canonico]
                .OrderByDescending(c => c.Peso)
                .ThenBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var vizinhosDiretos = diretas
                .Select(c => c.Destino)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var pontuacaoSegundoGrau = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string vizinho in vizinhosDiretos)
            {
                if (vizinho.Equals(canonico, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (ConexaoMusical segundoGrau in _listaAdjacencia[vizinho])
                {
                    if (segundoGrau.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase) ||
                        vizinhosDiretos.Contains(segundoGrau.Destino))
                        continue;

                    int incremento = EhPonderado ? segundoGrau.Peso : 1;
                    pontuacaoSegundoGrau[segundoGrau.Destino] =
                        pontuacaoSegundoGrau.GetValueOrDefault(segundoGrau.Destino, 0) + incremento;
                }
            }

            var descobertas = pontuacaoSegundoGrau
                .OrderByDescending(par => par.Value)
                .ThenBy(par => par.Key, StringComparer.OrdinalIgnoreCase)
                .Select(par => (par.Key, par.Value))
                .ToList();

            return (diretas, descobertas);
        }

        public List<(string Origem, string Destino, ConexaoMusical Conexao)> GerarPlaylistAutomatica(string tituloInicial, int tamanhoMaximo)
        {
            string? atual = ObterTituloCanonico(tituloInicial);
            var playlist = new List<(string, string, ConexaoMusical)>();

            if (atual is null || tamanhoMaximo < 2)
                return playlist;

            var visitadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { atual };

            while (visitadas.Count < tamanhoMaximo)
            {
                ConexaoMusical? melhorProxima = _listaAdjacencia[atual]
                    .Where(c => !visitadas.Contains(c.Destino))
                    .OrderByDescending(c => c.Peso)
                    .ThenBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (melhorProxima is null)
                    break;

                playlist.Add((atual, melhorProxima.Destino, melhorProxima));
                visitadas.Add(melhorProxima.Destino);
                atual = melhorProxima.Destino;
            }

            return playlist;
        }

        public List<string>? GerarFilaTransicao(string origem, string destino)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);

            if (orig is null || dest is null)
                return null;

            if (orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
                return new List<string> { orig };

            var fila = new Queue<string>();
            var anterior = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { [orig] = null };
            fila.Enqueue(orig);

            while (fila.Count > 0)
            {
                string atual = fila.Dequeue();
                foreach (ConexaoMusical c in _listaAdjacencia[atual])
                {
                    if (anterior.ContainsKey(c.Destino))
                        continue;

                    anterior[c.Destino] = atual;
                    if (c.Destino.Equals(dest, StringComparison.OrdinalIgnoreCase))
                    {
                        var caminho = new List<string>();
                        string? cursor = dest;
                        while (cursor is not null)
                        {
                            caminho.Add(cursor);
                            cursor = anterior[cursor];
                        }
                        caminho.Reverse();
                        return caminho;
                    }

                    fila.Enqueue(c.Destino);
                }
            }

            return null;
        }

        public void ExibirListaAdjacencia()
        {
            IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                Console.WriteLine("  Nenhuma música cadastrada.");
                return;
            }

            string seta = EhDirecionado ? "→" : "↔";
            int larguraTitulo = Math.Max(18, faixas.Max(f => f.Titulo.Length));

            foreach (FaixaMusical faixa in faixas)
            {
                List<ConexaoMusical> conexoes = _listaAdjacencia[faixa.Titulo];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  {faixa.Titulo.PadRight(larguraTitulo)}");
                Console.ResetColor();
                Console.Write($" {seta} ");

                if (conexoes.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("(faixa isolada - sem conexões)");
                    Console.ResetColor();
                    continue;
                }

                var formatadas = conexoes.Select(c =>
                    EhPonderado
                        ? $"{c.Destino} [{c.Contexto}: {c.Peso}%]"
                        : $"{c.Destino} ({c.Contexto})");

                Console.WriteLine(string.Join(", ", formatadas));
            }
        }

        public void ExibirMatrizAdjacencia()
        {
            IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                Console.WriteLine("  Nenhuma música cadastrada.");
                return;
            }

            int n = faixas.Count;
            string[] codigos = Enumerable.Range(1, n).Select(i => $"M{i}").ToArray();
            string[,] celulas = new string[n, n];
            int larguraColuna = 5;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    List<ConexaoMusical> encontradas = _listaAdjacencia[faixas[i].Titulo]
                        .Where(c => c.Destino.Equals(faixas[j].Titulo, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (encontradas.Count == 0)
                    {
                        celulas[i, j] = EhPonderado ? "-" : "0";
                    }
                    else if (!EhPonderado)
                    {
                        celulas[i, j] = encontradas.Count.ToString();
                    }
                    else
                    {
                        celulas[i, j] = encontradas.Count == 1
                            ? encontradas[0].Peso.ToString()
                            : $"{encontradas.Max(c => c.Peso)}*";
                    }

                    larguraColuna = Math.Max(larguraColuna, celulas[i, j].Length + 2);
                }
            }

            Console.WriteLine("  Legenda das Faixas:");
            for (int i = 0; i < n; i++)
                Console.WriteLine($"    {codigos[i],-4} = {faixas[i]}");

            Console.WriteLine();
            Console.WriteLine(EhPonderado
                ? "  Valores: '-' = sem conexão | número = peso (%) | 'N*' = arestas paralelas (exibindo maior peso)"
                : "  Valores: '0' = sem conexão | '1' = conexão simples | '>1' = quantidade de arestas paralelas");
            Console.WriteLine(EhDirecionado
                ? "  Direção: Matriz ASSIMÉTRICA permitida (Dígrafo: linha → coluna). Diagonal em magenta = Repeat (laço).\n"
                : "  Direção: Matriz SIMÉTRICA (Não-Direcionado: linha ↔ coluna). Diagonal em magenta = Repeat (laço).\n");

            Console.Write("      ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (string cod in codigos)
                Console.Write(cod.PadLeft(larguraColuna));
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < n; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  {codigos[i],-4}");
                Console.ResetColor();

                for (int j = 0; j < n; j++)
                {
                    bool temConexao = celulas[i, j] != "0" && celulas[i, j] != "-";
                    if (temConexao)
                        Console.ForegroundColor = i == j ? ConsoleColor.Magenta : ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.Write(celulas[i, j].PadLeft(larguraColuna));
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }
    }

    internal static class Program
    {
        private static GrafoMusical _grafo = CriarPresetMegaCatalogoStreaming();

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            while (true)
            {
                ExibirCabecalho();
                ExibirMenu();

                Console.Write("\nEscolha uma opção: ");
                string opcao = (Console.ReadLine() ?? "").Trim();
                Console.WriteLine();

                switch (opcao)
                {
                    case "1":
                        ExibirEstruturas();
                        break;
                    case "2":
                        ExibirDiagnosticoCompleto();
                        break;
                    case "3":
                        MenuListarCatalogoDetalhado();
                        break;
                    case "4":
                        MenuAdicionarFaixa();
                        break;
                    case "5":
                        MenuAdicionarConexao();
                        break;
                    case "6":
                        MenuAtivarModoRepeat();
                        break;
                    case "7":
                        MenuAutoConectarPorRegras();
                        break;
                    case "8":
                        MenuFiltrarPorRelacionamento();
                        break;
                    case "9":
                        MenuRemoverItem();
                        break;
                    case "10":
                        MenuRecomendarMusicas();
                        break;
                    case "11":
                        MenuGerarPlaylistAutomatica();
                        break;
                    case "12":
                        MenuGerarFilaTransicao();
                        break;
                    case "13":
                        MenuCarregarPresets();
                        break;
                    case "14":
                        MenuCriarGrafoPersonalizado();
                        break;
                    case "0":
                        Console.WriteLine("Encerrando o SoundGraph. Até a próxima!");
                        return;
                    default:
                        EscreverAlerta("Opção inválida. Escolha um número do menu.");
                        break;
                }

                Console.WriteLine("\nPressione ENTER para voltar ao menu...");
                Console.ReadLine();
            }
        }

        private static void ExibirCabecalho()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================================================================");
            Console.WriteLine("          SOUNDGRAPH PRO - MOTOR DE RECOMENDAÇÃO DE MÚSICAS E PLAYLISTS        ");
            Console.WriteLine("                Demonstração de Tipos e Direções de Grafos (C#)                ");
            Console.WriteLine("===============================================================================");
            Console.ResetColor();

            string direcao = _grafo.EhDirecionado ? "Direcionado (Dígrafo - Fila/Fluxo)" : "Não-Direcionado (Afinidade Mútua)";
            string peso = _grafo.EhPonderado ? "Ponderado (Afinidade %)" : "Não-Ponderado";
            Console.WriteLine($" Cenário atual : {_grafo.NomeCenario}");
            Console.WriteLine($" Configuração  : {direcao} | {peso}");
            Console.WriteLine($" Capacidade    : {_grafo.ObterFaixasOrdenadas().Count} músicas (vértices) | {_grafo.ContarArestas()} conexões (arestas) | {_grafo.ContarPorTipoRelacionamento().Count} tipo(s) ativo(s)");
            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        private static void ExibirMenu()
        {
            Console.WriteLine(" [1]  Exibir Lista de Adjacência e Matriz de Adjacência");
            Console.WriteLine(" [2]  Diagnóstico Completo (Tipos, Direções, Relacionamentos e Graus)");
            Console.WriteLine(" [3]  Listar Catálogo Completo de Músicas (Álbum, Ano, Gênero e BPM)");
            Console.WriteLine(" [4]  Cadastrar Nova Música (Vértice)");
            Console.WriteLine(" [5]  Conectar Duas Músicas (Escolhendo entre 14 Tipos de Relacionamento)");
            Console.WriteLine(" [6]  Ativar Modo Repeat em uma Música (Criar Auto-Laço -> Pseudografo)");
            Console.WriteLine(" [7]  Auto-Conectar Catálogo por Regras (Gênero, Artista ou BPM Compatível)");
            Console.WriteLine(" [8]  Filtrar Conexões por Tipo de Relacionamento");
            Console.WriteLine(" [9]  Remover Música ou Conexão");
            Console.WriteLine(" [10] Motor de Recomendação (1º Grau Diretas + 2º Grau Descobertas)");
            Console.WriteLine(" [11] Gerar Playlist Automática Contínua (Percurso por Maior Afinidade)");
            Console.WriteLine(" [12] Gerar Fila de Transição entre Duas Músicas (Menor Caminho - BFS)");
            Console.WriteLine(" [13] Carregar Cenários Prontos (6 Presets em Alta Capacidade)");
            Console.WriteLine(" [14] Criar Novo Grafo Musical do Zero (Escolher Direção e Peso)");
            Console.WriteLine(" [0]  Sair");
        }

        private static void ExibirEstruturas()
        {
            EscreverTitulo("LISTA DE ADJACÊNCIA");
            _grafo.ExibirListaAdjacencia();

            Console.WriteLine();
            EscreverTitulo("MATRIZ DE ADJACÊNCIA");
            _grafo.ExibirMatrizAdjacencia();
        }

        private static void ExibirDiagnosticoCompleto()
        {
            DiagnosticoGrafo d = _grafo.GerarDiagnostico();

            EscreverTitulo("DIAGNÓSTICO DE TIPOS E DIREÇÕES DO GRAFO");
            Console.WriteLine($"  • Direção              : {(d.EhDirecionado ? "Direcionado (Dígrafo - transições de sentido único)" : "Não-Direcionado (afinidade mútua / bidirecional)")}");
            Console.WriteLine($"  • Ponderação           : {(d.EhPonderado ? "Ponderado (arestas possuem peso de afinidade %)" : "Não-Ponderado (peso unitário)")}");
            Console.WriteLine($"  • Tipo Estrutural      : {d.TipoEstrutural}");
            Console.WriteLine($"  • Auto-laços (Repeat)  : {(d.PossuiLacos ? "Sim" : "Não")}");
            Console.WriteLine($"  • Arestas Paralelas    : {(d.PossuiArestasParalelas ? "Sim (múltiplos relacionamentos entre o mesmo par)" : "Não")}");
            Console.WriteLine($"  • Conectividade        : {d.Conectividade} ({d.ComponentesConexas} componente(s))");
            Console.WriteLine($"  • Ciclicidade          : {(d.PossuiCiclo ? "Cíclico (rádio/loop contínuo detectado)" : d.EhDirecionado ? "Acíclico (DAG - Fluxo sem retorno)" : "Acíclico")}");
            Console.WriteLine($"  • Grafo Completo (Kn)  : {(d.EhCompleto ? $"Sim (K{d.TotalVertices} - todas as faixas conectadas entre si)" : "Não")}");
            Console.WriteLine($"  • Grau Médio           : {d.GrauMedio:F2}");
            Console.WriteLine($"  • Densidade Relativa   : {d.DensidadePercentual:F1}%");

            Console.WriteLine();
            EscreverTitulo("DISTRIBUIÇÃO POR TIPO DE RELACIONAMENTO");
            if (d.ContagemPorTipo.Count == 0)
            {
                Console.WriteLine("  Nenhuma conexão registrada.");
            }
            else
            {
                foreach (var par in d.ContagemPorTipo.OrderByDescending(p => p.Value))
                {
                    Console.WriteLine($"  • {RelacionamentoHelper.ObterNome(par.Key),-24}: {par.Value} conexão(ões)");
                }
            }

            Console.WriteLine();
            EscreverTitulo("TABELA DE GRAUS DAS FAIXAS");

            IReadOnlyList<FaixaMusical> faixas = _grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                Console.WriteLine("  Nenhuma música cadastrada.");
                return;
            }

            if (_grafo.EhDirecionado)
            {
                Console.WriteLine($"  {"Faixa Musical",-24} | {"Gênero",-12} | {"Entrada",-8} | {"Saída",-8} | {"Total",-6}");
                Console.WriteLine("  " + new string('-', 70));

                foreach (FaixaMusical f in faixas)
                {
                    int entrada = _grafo.ObterGrauEntrada(f.Titulo);
                    int saida = _grafo.ObterGrauSaida(f.Titulo);
                    Console.WriteLine($"  {f.Titulo,-24} | {f.Genero,-12} | {entrada,-8} | {saida,-8} | {entrada + saida,-6}");
                }

                FaixaMusical maisAlcancada = faixas.OrderByDescending(f => _grafo.ObterGrauEntrada(f.Titulo)).First();
                FaixaMusical maisConectadora = faixas.OrderByDescending(f => _grafo.ObterGrauSaida(f.Titulo)).First();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  Faixa mais alcançada (maior grau de entrada): {maisAlcancada.Titulo} ({_grafo.ObterGrauEntrada(maisAlcancada.Titulo)} entrada(s)).");
                Console.WriteLine($"  Faixa mais recomendadora (maior grau de saída): {maisConectadora.Titulo} ({_grafo.ObterGrauSaida(maisConectadora.Titulo)} saída(s)).");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {"Faixa Musical",-24} | {"Artista",-18} | {"Gênero",-12} | {"Grau",-6} | {"Status",-15}");
                Console.WriteLine("  " + new string('-', 87));

                foreach (FaixaMusical f in faixas)
                {
                    int grau = _grafo.ObterGrauNaoDirecionado(f.Titulo);
                    string status = grau == 0 ? "Vértice Isolado" : "Conectada";
                    Console.WriteLine($"  {f.Titulo,-24} | {f.Artista,-18} | {f.Genero,-12} | {grau,-6} | {status,-15}");
                }

                FaixaMusical hub = faixas.OrderByDescending(f => _grafo.ObterGrauNaoDirecionado(f.Titulo)).First();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  Faixa hub central (maior grau): {hub.Titulo} (grau {_grafo.ObterGrauNaoDirecionado(hub.Titulo)}).");
                Console.ResetColor();
            }
        }

        private static void MenuListarCatalogoDetalhado()
        {
            EscreverTitulo("CATÁLOGO COMPLETO DE MÚSICAS");
            IReadOnlyList<FaixaMusical> faixas = _grafo.ObterFaixasOrdenadas();

            if (faixas.Count == 0)
            {
                Console.WriteLine("  Catálogo vazio.");
                return;
            }

            Console.WriteLine($"  {"#",-4} | {"Título",-24} | {"Artista",-18} | {"Gênero",-12} | {"Ano",-5} | {"BPM",-4} | {"Álbum",-20}");
            Console.WriteLine("  " + new string('-', 102));

            for (int i = 0; i < faixas.Count; i++)
            {
                FaixaMusical f = faixas[i];
                Console.WriteLine($"  {$"M{i + 1}",-4} | {f.Titulo,-24} | {f.Artista,-18} | {f.Genero,-12} | {f.Ano,-5} | {f.Bpm,-4} | {f.Album,-20}");
            }
        }

        private static void MenuAdicionarFaixa()
        {
            EscreverTitulo("CADASTRAR NOVA MÚSICA (VÉRTICE)");
            Console.Write("Título da música : ");
            string titulo = (Console.ReadLine() ?? "").Trim();
            Console.Write("Artista / Banda  : ");
            string artista = (Console.ReadLine() ?? "").Trim();
            Console.Write("Gênero musical   : ");
            string genero = (Console.ReadLine() ?? "").Trim();
            Console.Write("Álbum (Enter para Single): ");
            string album = (Console.ReadLine() ?? "").Trim();
            Console.Write("Ano de lançamento (Enter para 2020): ");
            if (!int.TryParse(Console.ReadLine(), out int ano))
                ano = 2020;
            Console.Write("BPM estimado (40 a 260, Enter para 120): ");
            if (!int.TryParse(Console.ReadLine(), out int bpm))
                bpm = 120;

            if (_grafo.AdicionarFaixa(titulo, artista, genero, album, ano, bpm))
                EscreverSucesso($"Música '{titulo}' adicionada ao catálogo.");
            else
                EscreverAlerta("Título inválido ou música já cadastrada.");
        }

        private static TipoRelacionamento EscolherTipoRelacionamento()
        {
            Console.WriteLine("\nTipos de relacionamento disponíveis:");
            foreach (TipoRelacionamento tipo in Enum.GetValues<TipoRelacionamento>())
            {
                Console.WriteLine($"  [{(int)tipo,2}] {RelacionamentoHelper.ObterNome(tipo)}");
            }

            Console.Write("Escolha o número do relacionamento (Enter para 1 - Afinidade Sonora): ");
            string entrada = (Console.ReadLine() ?? "").Trim();

            if (int.TryParse(entrada, out int codigo) && Enum.IsDefined(typeof(TipoRelacionamento), codigo))
                return (TipoRelacionamento)codigo;

            return TipoRelacionamento.AfinidadeSonora;
        }

        private static void MenuAdicionarConexao()
        {
            EscreverTitulo("CONECTAR MÚSICAS (ARESTA / ARCO)");
            ListarTitulosDisponiveis();

            Console.Write("\nMúsica de origem : ");
            string origem = (Console.ReadLine() ?? "").Trim();
            Console.Write("Música de destino (use a mesma para auto-laço Repeat): ");
            string destino = (Console.ReadLine() ?? "").Trim();

            TipoRelacionamento tipo = EscolherTipoRelacionamento();
            string? contextoPersonalizado = null;

            if (tipo == TipoRelacionamento.Personalizado)
            {
                Console.Write("Digite o rótulo personalizado do relacionamento: ");
                contextoPersonalizado = (Console.ReadLine() ?? "").Trim();
            }

            int peso = 1;
            if (_grafo.EhPonderado)
            {
                Console.Write("Afinidade / Peso (1 a 100): ");
                if (!int.TryParse(Console.ReadLine(), out peso) || peso < 1 || peso > 100)
                {
                    EscreverAlerta("Valor inválido. Aplicando afinidade padrão de 75%.");
                    peso = 75;
                }
            }

            var (sucesso, mensagem) = _grafo.AdicionarConexao(origem, destino, peso, tipo, contextoPersonalizado);
            if (sucesso)
                EscreverSucesso(mensagem);
            else
                EscreverAlerta(mensagem);
        }

        private static void MenuAtivarModoRepeat()
        {
            EscreverTitulo("ATIVAR MODO REPEAT (CRIAR AUTO-LAÇO)");
            ListarTitulosDisponiveis();

            Console.Write("\nDigite o título da música para colocar em Repeat: ");
            string titulo = (Console.ReadLine() ?? "").Trim();

            var (sucesso, mensagem) = _grafo.AdicionarConexao(titulo, titulo, 100, TipoRelacionamento.ModoRepeat);
            if (sucesso)
                EscreverSucesso(mensagem + " — O grafo agora é classificado como Pseudografo!");
            else
                EscreverAlerta(mensagem);
        }

        private static void MenuAutoConectarPorRegras()
        {
            EscreverTitulo("AUTO-CONECTAR CATÁLOGO POR REGRAS MUSICAIS");
            Console.WriteLine(" [1] Conectar faixas do MESMO GÊNERO");
            Console.WriteLine(" [2] Conectar faixas do MESMO ARTISTA");
            Console.WriteLine(" [3] Conectar faixas com BPM COMPATÍVEL (diferença <= 8 BPM)");
            Console.Write("\nEscolha a regra: ");

            if (!int.TryParse(Console.ReadLine(), out int regra) || regra < 1 || regra > 3)
            {
                EscreverAlerta("Regra inválida.");
                return;
            }

            int criadas = _grafo.AutoConectarPorRegra(regra);
            if (criadas > 0)
                EscreverSucesso($"{criadas} nova(s) conexão(ões) gerada(s) automaticamente!");
            else
                EscreverAlerta("Nenhuma nova conexão foi necessária para esta regra.");
        }

        private static void MenuFiltrarPorRelacionamento()
        {
            EscreverTitulo("FILTRAR CONEXÕES POR TIPO DE RELACIONAMENTO");
            TipoRelacionamento tipo = EscolherTipoRelacionamento();

            var encontradas = _grafo.FiltrarConexoesPorTipo(tipo);
            string seta = _grafo.EhDirecionado ? "→" : "↔";

            Console.WriteLine($"\n  Relacionamento selecionado: {RelacionamentoHelper.ObterNome(tipo)} ({encontradas.Count} encontrada(s))\n");

            if (encontradas.Count == 0)
            {
                Console.WriteLine("  Nenhuma conexão desse tipo cadastrada no cenário atual.");
                return;
            }

            foreach (var (origem, conexao) in encontradas)
            {
                string peso = _grafo.EhPonderado ? $" [Peso: {conexao.Peso}%]" : "";
                Console.WriteLine($"  ♪ {origem} {seta} {conexao.Destino} ({conexao.Contexto}){peso}");
            }
        }

        private static void MenuRemoverItem()
        {
            EscreverTitulo("REMOVER MÚSICA OU CONEXÃO");
            Console.WriteLine(" [1] Remover uma Música (vértice e todas as suas conexões)");
            Console.WriteLine(" [2] Remover uma Conexão específica");
            Console.Write("Opção: ");
            string op = (Console.ReadLine() ?? "").Trim();

            ListarTitulosDisponiveis();

            if (op == "1")
            {
                Console.Write("\nTítulo da música a remover: ");
                string titulo = (Console.ReadLine() ?? "").Trim();
                if (_grafo.RemoverFaixa(titulo))
                    EscreverSucesso($"Música '{titulo}' removida com sucesso.");
                else
                    EscreverAlerta("Música não encontrada.");
            }
            else if (op == "2")
            {
                Console.Write("\nMúsica de origem : ");
                string orig = (Console.ReadLine() ?? "").Trim();
                Console.Write("Música de destino: ");
                string dest = (Console.ReadLine() ?? "").Trim();

                if (_grafo.RemoverConexao(orig, dest))
                    EscreverSucesso("Conexão removida com sucesso.");
                else
                    EscreverAlerta("Conexão não encontrada.");
            }
            else
            {
                EscreverAlerta("Opção inválida.");
            }
        }

        private static void MenuRecomendarMusicas()
        {
            EscreverTitulo("MOTOR DE RECOMENDAÇÃO MUSICAL");
            ListarTitulosDisponiveis();

            Console.Write("\nQual música você está ouvindo agora? ");
            string titulo = (Console.ReadLine() ?? "").Trim();

            FaixaMusical? faixa = _grafo.ObterFaixa(titulo);
            if (faixa is null)
            {
                EscreverAlerta("Música não encontrada no catálogo.");
                return;
            }

            var (diretas, descobertas) = _grafo.RecomendarAPartirDe(faixa.Titulo);

            Console.WriteLine($"\n  Tocando agora: {faixa}");
            Console.WriteLine("\n  1) Próximas recomendadas (Conexões diretas / 1º grau):");
            if (diretas.Count == 0)
            {
                Console.WriteLine("     Nenhuma conexão direta disponível.");
            }
            else
            {
                foreach (ConexaoMusical c in diretas)
                {
                    string extra = _grafo.EhPonderado ? $" | Afinidade: {c.Peso}%" : "";
                    Console.WriteLine($"     ♪ {c.Destino} ({c.Contexto}{extra})");
                }
            }

            Console.WriteLine("\n  2) Descobertas (Faixas alcançáveis em 2º grau):");
            if (descobertas.Count == 0)
            {
                Console.WriteLine("     Nenhuma descoberta de 2º grau disponível.");
            }
            else
            {
                foreach (var (sugestao, pontuacao) in descobertas)
                {
                    string metrica = _grafo.EhPonderado ? $"índice {pontuacao}" : $"{pontuacao} ponte(s) em comum";
                    Console.WriteLine($"     ★ {sugestao} ({metrica})");
                }
            }
        }

        private static void MenuGerarPlaylistAutomatica()
        {
            EscreverTitulo("GERAR PLAYLIST AUTOMÁTICA POR MAIOR AFINIDADE");
            ListarTitulosDisponiveis();

            Console.Write("\nMúsica inicial da playlist: ");
            string inicio = (Console.ReadLine() ?? "").Trim();
            FaixaMusical? faixaInicial = _grafo.ObterFaixa(inicio);

            if (faixaInicial is null)
            {
                EscreverAlerta("Música não encontrada no catálogo.");
                return;
            }

            Console.Write("Quantidade máxima de faixas na playlist (ex.: 6): ");
            if (!int.TryParse(Console.ReadLine(), out int limite) || limite < 2)
                limite = 6;

            var passos = _grafo.GerarPlaylistAutomatica(faixaInicial.Titulo, limite);
            Console.WriteLine($"\n  1. {faixaInicial.Titulo} ({faixaInicial.Artista}) [Início]");

            if (passos.Count == 0)
            {
                EscreverAlerta("Não há conexões de saída disponíveis a partir dessa música.");
                return;
            }

            int somaPesos = 0;
            for (int i = 0; i < passos.Count; i++)
            {
                var passo = passos[i];
                FaixaMusical? dest = _grafo.ObterFaixa(passo.Destino);
                somaPesos += passo.Conexao.Peso;
                string infoPeso = _grafo.EhPonderado ? $" | {passo.Conexao.Peso}%" : "";
                Console.WriteLine($"  {i + 2}. {passo.Destino} ({dest?.Artista})  <-- via {passo.Conexao.Contexto}{infoPeso}");
            }

            if (_grafo.EhPonderado)
                EscreverSucesso($"\nPlaylist montada com {passos.Count + 1} faixas | Afinidade média das transições: {(double)somaPesos / passos.Count:F1}%.");
            else
                EscreverSucesso($"\nPlaylist montada com {passos.Count + 1} faixas.");
        }

        private static void MenuGerarFilaTransicao()
        {
            EscreverTitulo("GERAR FILA DE TRANSIÇÃO ENTRE MÚSICAS (BFS)");
            ListarTitulosDisponiveis();

            Console.Write("\nMúsica inicial : ");
            string orig = (Console.ReadLine() ?? "").Trim();
            Console.Write("Música destino : ");
            string dest = (Console.ReadLine() ?? "").Trim();

            List<string>? fila = _grafo.GerarFilaTransicao(orig, dest);
            if (fila is null)
            {
                EscreverAlerta($"Não existe caminho de transição de '{orig}' até '{dest}' neste grafo.");
                return;
            }

            string seta = _grafo.EhDirecionado ? " → " : " ↔ ";
            EscreverSucesso($"Fila gerada ({fila.Count - 1} transição(ões)): {string.Join(seta, fila)}");
        }

        private static void MenuCarregarPresets()
        {
            EscreverTitulo("CENÁRIOS PRONTOS EM ALTA CAPACIDADE (TIPOS E DIREÇÕES)");
            Console.WriteLine(" [1] Mega Catálogo de Streaming   (18 músicas | Direcionado | Ponderado | Multi + Pseudografo)");
            Console.WriteLine(" [2] Ecossistema Pop & Synth      (12 músicas | Não-Direcionado | Não-Ponderado | Simples | Conexo)");
            Console.WriteLine(" [3] Histórico Global de Fila     (12 músicas | Direcionado | Ponderado | Cíclico | Desconexo)");
            Console.WriteLine(" [4] Álbum Conceitual Completo K6 ( 6 músicas | Não-Direcionado | Completo K6 | Simples)");
            Console.WriteLine(" [5] Setlist de Festival (DAG)    (10 músicas | Direcionado | Ponderado | Acíclico - DAG)");
            Console.WriteLine(" [6] Mega Mix Treino + Repeat     (10 músicas | Não-Direcionado | Ponderado | Multi + Pseudografo)");

            Console.Write("\nEscolha o cenário: ");
            string op = (Console.ReadLine() ?? "").Trim();

            switch (op)
            {
                case "1":
                    _grafo = CriarPresetMegaCatalogoStreaming();
                    break;
                case "2":
                    _grafo = CriarPresetEcossistemaPopSimples();
                    break;
                case "3":
                    _grafo = CriarPresetHistoricoGlobalDesconexo();
                    break;
                case "4":
                    _grafo = CriarPresetAlbumCompletoK6();
                    break;
                case "5":
                    _grafo = CriarPresetSetlistFestivalDag();
                    break;
                case "6":
                    _grafo = CriarPresetMegaMixMultigrafoPseudografo();
                    break;
                default:
                    EscreverAlerta("Cenário inválido.");
                    return;
            }

            EscreverSucesso($"Cenário '{_grafo.NomeCenario}' carregado!");
        }

        private static void MenuCriarGrafoPersonalizado()
        {
            EscreverTitulo("CRIAR NOVO GRAFO MUSICAL PERSONALIZADO");
            Console.Write("Nome do cenário: ");
            string nome = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nome))
                nome = "Playlist Personalizada";

            Console.Write("O grafo será DIRECIONADO (fila com sentido único)? (S/N): ");
            bool direcionado = (Console.ReadLine() ?? "").Trim().Equals("S", StringComparison.OrdinalIgnoreCase);

            Console.Write("O grafo será PONDERADO (com % de afinidade)? (S/N): ");
            bool ponderado = (Console.ReadLine() ?? "").Trim().Equals("S", StringComparison.OrdinalIgnoreCase);

            _grafo = new GrafoMusical(nome, direcionado, ponderado);
            EscreverSucesso($"Novo grafo '{nome}' criado! Use as opções [4], [5] e [7] para popular sua rede.");
        }

        private static GrafoMusical CriarPresetMegaCatalogoStreaming()
        {
            var g = new GrafoMusical(
                "Mega Catálogo de Streaming (18 Faixas | Dígrafo Ponderado | Multi + Pseudografo)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Blinding Lights", "The Weeknd", "Synthpop", "After Hours", 2020, 171);
            g.AdicionarFaixa("Save Your Tears", "The Weeknd", "Synthpop", "After Hours", 2020, 118);
            g.AdicionarFaixa("Starboy", "The Weeknd", "Pop", "Starboy", 2016, 186);
            g.AdicionarFaixa("Take On Me", "a-ha", "Synthpop", "Hunting High", 1985, 169);
            g.AdicionarFaixa("Levitating", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 103);
            g.AdicionarFaixa("Instant Crush", "Daft Punk", "Eletrônica", "RAM", 2013, 110);
            g.AdicionarFaixa("Get Lucky", "Daft Punk", "Eletrônica", "RAM", 2013, 116);
            g.AdicionarFaixa("Midnight City", "M83", "Eletrônica", "Hurry Up", 2011, 105);
            g.AdicionarFaixa("Numb", "Linkin Park", "Rock", "Meteora", 2003, 110);
            g.AdicionarFaixa("In The End", "Linkin Park", "Rock", "Hybrid Theory", 2000, 105);
            g.AdicionarFaixa("Enter Sandman", "Metallica", "Metal", "Black Album", 1991, 123);
            g.AdicionarFaixa("Master of Puppets", "Metallica", "Metal", "Master of Puppets", 1986, 212);
            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Till I Collapse", "Eminem", "Rap", "The Eminem Show", 2002, 171);
            g.AdicionarFaixa("Stronger", "Kanye West", "Rap", "Graduation", 2007, 104);
            g.AdicionarFaixa("Tempo Perdido", "Legião Urbana", "Rock BR", "Dois", 1986, 122);
            g.AdicionarFaixa("Pais e Filhos", "Legião Urbana", "Rock BR", "As Quatro Estações", 1989, 118);
            g.AdicionarFaixa("Lofi Rain", "Chillhop Beats", "Lo-Fi", "Study Sessions", 2023, 82);

            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 96, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 91, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Blinding Lights", "Take On Me", 94, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Blinding Lights", "Starboy", 89, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Blinding Lights", "Blinding Lights", 100, TipoRelacionamento.ModoRepeat);
            g.AdicionarConexao("Save Your Tears", "Levitating", 85, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Save Your Tears", "Instant Crush", 78, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Starboy", "Instant Crush", 92, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Starboy", "Get Lucky", 84, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Starboy", "Blinding Lights", 88, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("Take On Me", "Midnight City", 86, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Levitating", "Get Lucky", 90, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Get Lucky", "Instant Crush", 93, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Get Lucky", "Stronger", 87, TipoRelacionamento.SampleOuCover);
            g.AdicionarConexao("Instant Crush", "Midnight City", 83, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Midnight City", "Lofi Rain", 72, TipoRelacionamento.PlaylistFoco);
            g.AdicionarConexao("Lofi Rain", "Lofi Rain", 100, TipoRelacionamento.ModoRepeat);
            g.AdicionarConexao("Stronger", "Lose Yourself", 89, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Stronger", "Till I Collapse", 91, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Till I Collapse", 97, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Lose Yourself", "Till I Collapse", 95, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Blinding Lights", 80, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Till I Collapse", "Numb", 84, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Till I Collapse", "Enter Sandman", 88, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Numb", "In The End", 98, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Numb", "In The End", 92, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("In The End", "Numb", 90, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("In The End", "Enter Sandman", 79, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("In The End", "Tempo Perdido", 74, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Enter Sandman", "Master of Puppets", 96, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Master of Puppets", "Enter Sandman", 91, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Master of Puppets", "Till I Collapse", 82, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Tempo Perdido", "Pais e Filhos", 95, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Pais e Filhos", "Tempo Perdido", 89, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("Pais e Filhos", "Save Your Tears", 76, TipoRelacionamento.BpmCompativel);
            return g;
        }

        private static GrafoMusical CriarPresetEcossistemaPopSimples()
        {
            var g = new GrafoMusical(
                "Ecossistema Pop & Synth (12 Faixas | Não-Direcionado | Simples | Conexo)",
                ehDirecionado: false,
                ehPonderado: false);

            g.AdicionarFaixa("Blinding Lights", "The Weeknd", "Synthpop", "After Hours", 2020, 171);
            g.AdicionarFaixa("Save Your Tears", "The Weeknd", "Synthpop", "After Hours", 2020, 118);
            g.AdicionarFaixa("Starboy", "The Weeknd", "Pop", "Starboy", 2016, 186);
            g.AdicionarFaixa("Take On Me", "a-ha", "Synthpop", "Hunting High", 1985, 169);
            g.AdicionarFaixa("Levitating", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 103);
            g.AdicionarFaixa("Physical", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 147);
            g.AdicionarFaixa("As It Was", "Harry Styles", "Synthpop", "Harry's House", 2022, 174);
            g.AdicionarFaixa("Midnight City", "M83", "Eletrônica", "Hurry Up", 2011, 105);
            g.AdicionarFaixa("Instant Crush", "Daft Punk", "Eletrônica", "RAM", 2013, 110);
            g.AdicionarFaixa("Get Lucky", "Daft Punk", "Eletrônica", "RAM", 2013, 116);
            g.AdicionarFaixa("Billie Jean", "Michael Jackson", "Pop", "Thriller", 1982, 117);
            g.AdicionarFaixa("Beat It", "Michael Jackson", "Pop", "Thriller", 1982, 139);

            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Blinding Lights", "Take On Me", 1, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Blinding Lights", "As It Was", 1, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Save Your Tears", "Starboy", 1, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Save Your Tears", "Billie Jean", 1, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Starboy", "Instant Crush", 1, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Take On Me", "Midnight City", 1, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("As It Was", "Physical", 1, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Physical", "Levitating", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Levitating", "Get Lucky", 1, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Get Lucky", "Instant Crush", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Instant Crush", "Midnight City", 1, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Billie Jean", "Beat It", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Billie Jean", "Get Lucky", 1, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Beat It", "Physical", 1, TipoRelacionamento.PlaylistTreino);
            return g;
        }

        private static GrafoMusical CriarPresetHistoricoGlobalDesconexo()
        {
            var g = new GrafoMusical(
                "Histórico Global de Fila (12 Faixas | Dígrafo Ponderado | Cíclico | Desconexo)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Numb", "Linkin Park", "Rock", "Meteora", 2003, 110);
            g.AdicionarFaixa("In The End", "Linkin Park", "Rock", "Hybrid Theory", 2000, 105);
            g.AdicionarFaixa("Faint", "Linkin Park", "Rock", "Meteora", 2003, 135);
            g.AdicionarFaixa("Bring Me To Life", "Evanescence", "Rock", "Fallen", 2003, 95);
            g.AdicionarFaixa("Smells Like Teen", "Nirvana", "Grunge", "Nevermind", 1991, 117);
            g.AdicionarFaixa("Everlong", "Foo Fighters", "Rock", "The Colour", 1997, 158);
            g.AdicionarFaixa("Seven Nation Army", "White Stripes", "Rock", "Elephant", 2003, 124);
            g.AdicionarFaixa("Chop Suey", "System of a Down", "Metal", "Toxicity", 2001, 127);
            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Till I Collapse", "Eminem", "Rap", "Eminem Show", 2002, 171);
            g.AdicionarFaixa("Clair de Lune", "Debussy", "Clássica", "Suite Bergamasque", 1905, 66);
            g.AdicionarFaixa("Fly Me To The Moon", "Frank Sinatra", "Jazz", "It Might Be", 1964, 119);

            g.AdicionarConexao("Numb", "In The End", 96, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("In The End", "Faint", 92, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Faint", "Bring Me To Life", 85, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Bring Me To Life", "Numb", 88, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("Smells Like Teen", "Everlong", 91, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Everlong", "Seven Nation Army", 84, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Seven Nation Army", "Chop Suey", 82, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Chop Suey", "Faint", 87, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Smells Like Teen", "Numb", 80, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Lose Yourself", "Till I Collapse", 97, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 93, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Clair de Lune", "Fly Me To The Moon", 74, TipoRelacionamento.PlaylistFoco);
            return g;
        }

        private static GrafoMusical CriarPresetAlbumCompletoK6()
        {
            var g = new GrafoMusical(
                "Álbum Conceitual Neon Horizons (6 Faixas | Grafo Completo K6)",
                ehDirecionado: false,
                ehPonderado: false);

            var faixas = new (string Titulo, int Bpm)[]
            {
                ("Overture 1984", 112),
                ("Neon Highway", 118),
                ("Cybernetic Pulse", 122),
                ("Digital Horizon", 116),
                ("Starlight Echo", 114),
                ("Midnight Finale", 110)
            };

            foreach (var (titulo, bpm) in faixas)
                g.AdicionarFaixa(titulo, "Synth Collective", "Synthwave", "Neon Horizons", 2025, bpm);

            for (int i = 0; i < faixas.Length; i++)
            {
                for (int j = i + 1; j < faixas.Length; j++)
                    g.AdicionarConexao(faixas[i].Titulo, faixas[j].Titulo, 1, TipoRelacionamento.MesmoAlbum);
            }

            return g;
        }

        private static GrafoMusical CriarPresetSetlistFestivalDag()
        {
            var g = new GrafoMusical(
                "Setlist de Festival Progressivo (10 Faixas | Dígrafo Ponderado Acíclico - DAG)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Intro Acústica", "Banda Aurora", "Acústico", "Ao Vivo", 2026, 85);
            g.AdicionarFaixa("Warmup Indie", "Banda Aurora", "Indie", "Ao Vivo", 2026, 102);
            g.AdicionarFaixa("Batida Lo-Fi", "DJ Pulse", "Lo-Fi", "Ao Vivo", 2026, 90);
            g.AdicionarFaixa("Vibe Pop", "Luna Star", "Pop", "Ao Vivo", 2026, 118);
            g.AdicionarFaixa("Drop Eletrônico", "DJ Pulse", "Eletrônica", "Ao Vivo", 2026, 128);
            g.AdicionarFaixa("Solo de Guitarra", "Banda Aurora", "Rock", "Ao Vivo", 2026, 134);
            g.AdicionarFaixa("Clímax Rock", "Banda Aurora", "Rock", "Ao Vivo", 2026, 145);
            g.AdicionarFaixa("Hino da Arena", "All Stars", "Rock", "Ao Vivo", 2026, 140);
            g.AdicionarFaixa("Balada Final", "Luna Star", "Balada", "Ao Vivo", 2026, 78);
            g.AdicionarFaixa("Bis Épico", "All Stars", "Rock", "Ao Vivo", 2026, 132);

            g.AdicionarConexao("Intro Acústica", "Warmup Indie", 90, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Intro Acústica", "Batida Lo-Fi", 86, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Warmup Indie", "Vibe Pop", 88, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Warmup Indie", "Solo de Guitarra", 82, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Batida Lo-Fi", "Vibe Pop", 80, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Batida Lo-Fi", "Drop Eletrônico", 94, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Vibe Pop", "Drop Eletrônico", 92, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Vibe Pop", "Clímax Rock", 79, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Drop Eletrônico", "Clímax Rock", 91, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Solo de Guitarra", "Clímax Rock", 96, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Clímax Rock", "Hino da Arena", 97, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Hino da Arena", "Balada Final", 85, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Hino da Arena", "Bis Épico", 95, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Balada Final", "Bis Épico", 93, TipoRelacionamento.TransicaoFila);
            return g;
        }

        private static GrafoMusical CriarPresetMegaMixMultigrafoPseudografo()
        {
            var g = new GrafoMusical(
                "Mega Mix Treino + Repeat (10 Faixas | Ponderado | Multigrafo e Pseudografo)",
                ehDirecionado: false,
                ehPonderado: true);

            g.AdicionarFaixa("Eye of the Tiger", "Survivor", "Rock", "Eye of the Tiger", 1982, 109);
            g.AdicionarFaixa("Till I Collapse", "Eminem", "Rap", "Eminem Show", 2002, 171);
            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Remember The Name", "Fort Minor", "Rap", "Rising Tied", 2005, 85);
            g.AdicionarFaixa("Stronger", "Kanye West", "Rap", "Graduation", 2007, 104);
            g.AdicionarFaixa("Power", "Kanye West", "Rap", "MBDTF", 2010, 154);
            g.AdicionarFaixa("Can't Hold Us", "Macklemore", "Rap", "The Heist", 2011, 146);
            g.AdicionarFaixa("Believer", "Imagine Dragons", "Rock", "Evolve", 2017, 125);
            g.AdicionarFaixa("Thunder", "Imagine Dragons", "Pop Rock", "Evolve", 2017, 168);
            g.AdicionarFaixa("Enter Sandman", "Metallica", "Metal", "Black Album", 1991, 123);

            g.AdicionarConexao("Eye of the Tiger", "Till I Collapse", 93, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Eye of the Tiger", "Till I Collapse", 88, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 98, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 96, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 95, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Lose Yourself", 100, TipoRelacionamento.ModoRepeat);
            g.AdicionarConexao("Lose Yourself", "Remember The Name", 91, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Remember The Name", "Stronger", 87, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Stronger", "Power", 95, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Stronger", "Power", 92, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Power", "Can't Hold Us", 90, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Can't Hold Us", "Believer", 89, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Believer", "Thunder", 94, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Believer", "Enter Sandman", 86, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Enter Sandman", "Eye of the Tiger", 84, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Enter Sandman", "Enter Sandman", 100, TipoRelacionamento.ModoRepeat);
            return g;
        }

        private static void ListarTitulosDisponiveis()
        {
            var titulos = _grafo.ObterFaixasOrdenadas().Select(f => f.Titulo);
            Console.WriteLine("Músicas no catálogo: " + string.Join(" | ", titulos));
        }

        private static void EscreverTitulo(string titulo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== {titulo} ===");
            Console.ResetColor();
        }

        private static void EscreverSucesso(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {msg}");
            Console.ResetColor();
        }

        private static void EscreverAlerta(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] {msg}");
            Console.ResetColor();
        }
    }
}