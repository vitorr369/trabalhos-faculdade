using System;
using System.Collections.Generic;
using System.IO;
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
        public string TipoEstruturalCurto { get; init; } = "";
        public string Conectividade { get; init; } = "";
        public int ComponentesConexas { get; init; }
        public bool PossuiCiclo { get; init; }
        public bool EhCompleto { get; init; }
        public double GrauMedio { get; init; }
        public double GrauMedioDirecionadoPorSentido { get; init; }
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
            if (string.IsNullOrWhiteSpace(limpo))
                return null;

            return _faixas.TryGetValue(limpo, out FaixaMusical? faixa) ? faixa.Titulo : null;
        }

        public string? ResolverTituloOuCodigo(string entrada)
        {
            string limpo = entrada.Trim();
            if (string.IsNullOrWhiteSpace(limpo))
                return null;

            string? direto = ObterTituloCanonico(limpo);
            if (direto is not null)
                return direto;

            string numeroTexto = limpo.StartsWith("M", StringComparison.OrdinalIgnoreCase)
                ? limpo[1..]
                : limpo;

            if (int.TryParse(numeroTexto, out int indice))
            {
                IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();
                if (indice >= 1 && indice <= faixas.Count)
                    return faixas[indice - 1].Titulo;
            }

            return null;
        }

        public FaixaMusical? ObterFaixa(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
            return canonico is null ? null : _faixas[canonico];
        }

        public IReadOnlyList<ConexaoMusical> ObterConexoesDe(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
            return canonico is null
                ? Array.Empty<ConexaoMusical>()
                : _listaAdjacencia[canonico];
        }

        public bool PossuiModoRepeat(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
            if (canonico is null)
                return false;

            return _listaAdjacencia[canonico]
                .Any(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));
        }

        public (bool Ativado, string Mensagem) AlternarModoRepeat(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
            if (canonico is null)
                return (false, "Música não encontrada.");

            if (PossuiModoRepeat(canonico))
            {
                _listaAdjacencia[canonico]
                    .RemoveAll(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));
                return (false, $"Modo Repeat desativado em '{canonico}'.");
            }

            AdicionarConexao(canonico, canonico, 100, TipoRelacionamento.ModoRepeat);
            return (true, $"Modo Repeat ativado em '{canonico}' (auto-laço criado).");
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

        public bool RemoverFaixa(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
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

        public List<ConexaoMusical> ObterConexoesEntre(string origemOuCodigo, string destinoOuCodigo)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

            if (orig is null || dest is null)
                return new List<ConexaoMusical>();

            return _listaAdjacencia[orig]
                .Where(c => c.Destino.Equals(dest, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public (bool Sucesso, string Mensagem) AdicionarConexao(
            string origemOuCodigo,
            string destinoOuCodigo,
            int peso = 1,
            TipoRelacionamento tipo = TipoRelacionamento.AfinidadeSonora,
            string? contextoPersonalizado = null)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

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
            string textoPeso = EhPonderado ? $" [{pesoFinal}%]" : "";
            return (true, $"Conexão criada: {orig} {seta} {dest} ({novaConexao.Contexto}){textoPeso}");
        }

        public bool RemoverConexaoPorIndice(string origemOuCodigo, string destinoOuCodigo, int indiceEntrePar)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

            if (orig is null || dest is null || indiceEntrePar < 0)
                return false;

            List<ConexaoMusical> listaOrig = _listaAdjacencia[orig];
            int encontrados = 0;

            for (int i = 0; i < listaOrig.Count; i++)
            {
                if (!listaOrig[i].Destino.Equals(dest, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (encontrados == indiceEntrePar)
                {
                    ConexaoMusical removida = listaOrig[i];
                    listaOrig.RemoveAt(i);
                    RemoverEspelhoSeNaoDirecionado(orig, dest, removida);
                    return true;
                }

                encontrados++;
            }

            return false;
        }

        public int RemoverTodasConexoesEntre(string origemOuCodigo, string destinoOuCodigo)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

            if (orig is null || dest is null)
                return 0;

            int removidas = _listaAdjacencia[orig]
                .RemoveAll(c => c.Destino.Equals(dest, StringComparison.OrdinalIgnoreCase));

            if (!EhDirecionado && !orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
            {
                _listaAdjacencia[dest]
                    .RemoveAll(c => c.Destino.Equals(orig, StringComparison.OrdinalIgnoreCase));
            }

            return removidas;
        }

        private void RemoverEspelhoSeNaoDirecionado(string orig, string dest, ConexaoMusical removida)
        {
            if (EhDirecionado || orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
                return;

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
                        criadas += ConectarRegraSimetrica(a.Titulo, b.Titulo, 78, TipoRelacionamento.MesmoGenero);
                    }
                    else if (tipoRegra == 2 && a.Artista.Equals(b.Artista, StringComparison.OrdinalIgnoreCase))
                    {
                        criadas += ConectarRegraSimetrica(a.Titulo, b.Titulo, 92, TipoRelacionamento.MesmoArtista);
                    }
                    else if (tipoRegra == 3 && Math.Abs(a.Bpm - b.Bpm) <= 8)
                    {
                        int afinidadeBpm = Math.Clamp(100 - Math.Abs(a.Bpm - b.Bpm) * 3, 65, 98);

                        if (EhDirecionado && a.Bpm == b.Bpm)
                        {
                            criadas += ConectarRegraSimetrica(a.Titulo, b.Titulo, afinidadeBpm, TipoRelacionamento.BpmCompativel);
                        }
                        else
                        {
                            FaixaMusical origem = EhDirecionado && a.Bpm > b.Bpm ? b : a;
                            FaixaMusical destino = EhDirecionado && a.Bpm > b.Bpm ? a : b;

                            if (!ExisteConexaoMesmoTipo(origem.Titulo, destino.Titulo, TipoRelacionamento.BpmCompativel))
                            {
                                AdicionarConexao(origem.Titulo, destino.Titulo, afinidadeBpm, TipoRelacionamento.BpmCompativel);
                                criadas++;
                            }
                        }
                    }
                }
            }

            return criadas;
        }

        private int ConectarRegraSimetrica(string tituloA, string tituloB, int peso, TipoRelacionamento tipo)
        {
            int criadas = 0;

            if (!ExisteConexaoMesmoTipo(tituloA, tituloB, tipo))
            {
                AdicionarConexao(tituloA, tituloB, peso, tipo);
                criadas++;
            }

            if (EhDirecionado && !ExisteConexaoMesmoTipo(tituloB, tituloA, tipo))
            {
                AdicionarConexao(tituloB, tituloA, peso, tipo);
                criadas++;
            }

            return criadas;
        }

        public List<(string Origem, ConexaoMusical Conexao)> FiltrarConexoesPorTipo(TipoRelacionamento tipo)
        {
            var resultado = new List<(string, ConexaoMusical)>();

            foreach (FaixaMusical faixa in ObterFaixasOrdenadas())
            {
                foreach (ConexaoMusical conexao in _listaAdjacencia[faixa.Titulo])
                {
                    if (conexao.Tipo != tipo)
                        continue;

                    if (!EhDirecionado &&
                        string.Compare(faixa.Titulo, conexao.Destino, StringComparison.OrdinalIgnoreCase) > 0)
                        continue;

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
            string tipoCurto;
            if (lacos && paralelas)
            {
                tipoEstrutural = "Pseudografo (possui auto-laço/Repeat e arestas paralelas)";
                tipoCurto = "Multi + Pseudografo";
            }
            else if (lacos)
            {
                tipoEstrutural = "Pseudografo (possui auto-laço / modo Repeat)";
                tipoCurto = "Pseudografo";
            }
            else if (paralelas)
            {
                tipoEstrutural = "Multigrafo (possui múltiplos relacionamentos entre o mesmo par)";
                tipoCurto = "Multigrafo";
            }
            else
            {
                tipoEstrutural = "Grafo Simples (sem laços e sem arestas paralelas)";
                tipoCurto = "Grafo Simples";
            }

            var (conectividade, componentes) = VerificarConectividade();
            int n = _faixas.Count;
            int e = ContarArestas();

            double somaGraus = EhDirecionado
                ? _faixas.Keys.Sum(v => ObterGrauEntrada(v) + ObterGrauSaida(v))
                : _faixas.Keys.Sum(ObterGrauNaoDirecionado);

            double grauMedio = n == 0 ? 0 : somaGraus / n;
            double grauMedioPorSentido = n == 0 ? 0 : (double)e / n;

            int paresDistintos = 0;
            foreach (var par in _listaAdjacencia)
            {
                paresDistintos += par.Value
                    .Where(c => !c.Destino.Equals(par.Key, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Destino)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            }
            if (!EhDirecionado)
                paresDistintos /= 2;

            double maxArestasSimples = n < 2 ? 0 : (EhDirecionado ? (double)n * (n - 1) : (double)n * (n - 1) / 2.0);
            double densidade = maxArestasSimples == 0 ? 0 : (paresDistintos / maxArestasSimples) * 100.0;

            return new DiagnosticoGrafo
            {
                TotalVertices = n,
                TotalArestas = e,
                EhDirecionado = EhDirecionado,
                EhPonderado = EhPonderado,
                PossuiLacos = lacos,
                PossuiArestasParalelas = paralelas,
                TipoEstrutural = tipoEstrutural,
                TipoEstruturalCurto = tipoCurto,
                Conectividade = conectividade,
                ComponentesConexas = componentes,
                PossuiCiclo = PossuiCiclo(),
                EhCompleto = EhCompleto(),
                GrauMedio = grauMedio,
                GrauMedioDirecionadoPorSentido = grauMedioPorSentido,
                DensidadePercentual = densidade,
                ContagemPorTipo = ContarPorTipoRelacionamento()
            };
        }

        public (List<ConexaoMusical> Diretas, List<(string Titulo, int Pontuacao)> Descobertas, bool PossuiModoRepeat)
            RecomendarAPartirDe(string tituloOuCodigo)
        {
            string? canonico = ResolverTituloOuCodigo(tituloOuCodigo);
            if (canonico is null)
                return (new List<ConexaoMusical>(), new List<(string, int)>(), false);

            bool possuiModoRepeat = _listaAdjacencia[canonico]
                .Any(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));

            List<ConexaoMusical> diretas = _listaAdjacencia[canonico]
                .Where(c => !c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.Peso)
                .ThenBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var vizinhosDiretos = diretas
                .Select(c => c.Destino)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var pontuacaoSegundoGrau = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string vizinho in vizinhosDiretos)
            {
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

            return (diretas, descobertas, possuiModoRepeat);
        }

        public List<(string Origem, string Destino, ConexaoMusical Conexao)> GerarPlaylistAutomatica(string tituloOuCodigoInicial, int tamanhoMaximo)
        {
            string? atual = ResolverTituloOuCodigo(tituloOuCodigoInicial);
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

        public List<string>? GerarFilaTransicao(string origemOuCodigo, string destinoOuCodigo)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

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

        public (List<string> Caminho, List<(string Origem, string Destino, string Contexto, int Peso)> Passos, double AfinidadeMedia)?
            GerarFilaTransicaoDijkstra(string origemOuCodigo, string destinoOuCodigo)
        {
            string? orig = ResolverTituloOuCodigo(origemOuCodigo);
            string? dest = ResolverTituloOuCodigo(destinoOuCodigo);

            if (orig is null || dest is null)
                return null;

            if (orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
                return (new List<string> { orig }, new List<(string, string, string, int)>(), 100.0);

            var distancia = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { [orig] = 0 };
            var anterior = new Dictionary<string, (string? Anterior, string Contexto, int Peso)>(StringComparer.OrdinalIgnoreCase);
            var filaPrioridade = new PriorityQueue<string, int>();
            filaPrioridade.Enqueue(orig, 0);

            while (filaPrioridade.TryDequeue(out string? atual, out int custoAtual))
            {
                if (custoAtual > distancia.GetValueOrDefault(atual, int.MaxValue))
                    continue;

                if (atual.Equals(dest, StringComparison.OrdinalIgnoreCase))
                    break;

                foreach (ConexaoMusical c in _listaAdjacencia[atual])
                {
                    if (c.Destino.Equals(atual, StringComparison.OrdinalIgnoreCase))
                        continue;

                    int pesoValido = EhPonderado ? Math.Clamp(c.Peso, 1, 100) : 100;
                    int custoPasso = 101 - pesoValido;
                    int novoCusto = custoAtual + custoPasso;

                    if (novoCusto < distancia.GetValueOrDefault(c.Destino, int.MaxValue))
                    {
                        distancia[c.Destino] = novoCusto;
                        anterior[c.Destino] = (atual, c.Contexto, pesoValido);
                        filaPrioridade.Enqueue(c.Destino, novoCusto);
                    }
                }
            }

            if (!anterior.ContainsKey(dest))
                return null;

            var caminho = new List<string> { dest };
            var passos = new List<(string Origem, string Destino, string Contexto, int Peso)>();
            string cursor = dest;

            while (anterior.TryGetValue(cursor, out var passo) && passo.Anterior is not null)
            {
                passos.Add((passo.Anterior, cursor, passo.Contexto, passo.Peso));
                caminho.Add(passo.Anterior);
                cursor = passo.Anterior;
            }

            caminho.Reverse();
            passos.Reverse();
            double afinidadeMedia = passos.Count == 0 ? 100.0 : passos.Average(p => p.Peso);
            return (caminho, passos, afinidadeMedia);
        }

        public (string[] Codigos, string[,] Celulas, int LarguraColuna) ObterDadosMatriz(IReadOnlyList<FaixaMusical> faixas)
        {
            int n = faixas.Count;
            string[] codigos = Enumerable.Range(1, n).Select(i => $"M{i}").ToArray();
            string[,] celulas = new string[n, n];
            int larguraColuna = 4;

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

                    larguraColuna = Math.Max(larguraColuna, celulas[i, j].Length + 1);
                }
            }

            larguraColuna = Math.Max(larguraColuna, codigos.Length > 0 ? codigos.Max(c => c.Length) + 1 : 4);
            return (codigos, celulas, larguraColuna);
        }

        public string GerarRelatorioTexto()
        {
            var sb = new StringBuilder();
            DiagnosticoGrafo d = GerarDiagnostico();
            IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();

            sb.AppendLine("===============================================================================");
            sb.AppendLine("                  SOUNDGRAPH PRO - RELATÓRIO DO GRAFO MUSICAL                  ");
            sb.AppendLine("===============================================================================");
            sb.AppendLine($"Cenário       : {NomeCenario}");
            sb.AppendLine($"Gerado em     : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Vértices (|V|): {d.TotalVertices}");
            sb.AppendLine($"Arestas  (|E|): {d.TotalArestas}");
            sb.AppendLine();

            sb.AppendLine("=== DIAGNÓSTICO DE TIPOS E DIREÇÕES ===");
            sb.AppendLine($"  • Direção              : {(d.EhDirecionado ? "Direcionado (Dígrafo - transições de sentido único)" : "Não-Direcionado (afinidade mútua / bidirecional)")}");
            sb.AppendLine($"  • Ponderação           : {(d.EhPonderado ? "Ponderado (arestas possuem peso de afinidade %)" : "Não-Ponderado (peso unitário)")}");
            sb.AppendLine($"  • Tipo Estrutural      : {d.TipoEstrutural}");
            sb.AppendLine($"  • Auto-laços (Repeat)  : {(d.PossuiLacos ? "Sim" : "Não")}");
            sb.AppendLine($"  • Arestas Paralelas    : {(d.PossuiArestasParalelas ? "Sim (múltiplos relacionamentos entre o mesmo par)" : "Não")}");
            sb.AppendLine($"  • Conectividade        : {d.Conectividade} ({d.ComponentesConexas} componente(s))");
            sb.AppendLine($"  • Ciclicidade          : {(d.PossuiCiclo ? "Cíclico (rádio/loop contínuo detectado)" : d.EhDirecionado ? "Acíclico (DAG - Fluxo sem retorno)" : "Acíclico")}");
            sb.AppendLine($"  • Grafo Completo (Kn)  : {(d.EhCompleto ? $"Sim (K{d.TotalVertices} - todas as faixas conectadas entre si)" : "Não")}");
            sb.AppendLine(d.EhDirecionado
                ? $"  • Grau Médio (Ent+Saí) : {d.GrauMedio:F2} (Entrada = Saída = {d.GrauMedioDirecionadoPorSentido:F2})"
                : $"  • Grau Médio           : {d.GrauMedio:F2}");
            sb.AppendLine($"  • Densidade Relativa   : {d.DensidadePercentual:F1}% (pares distintos sem laço)");
            sb.AppendLine();

            sb.AppendLine("=== DISTRIBUIÇÃO POR TIPO DE RELACIONAMENTO ===");
            if (d.ContagemPorTipo.Count == 0)
            {
                sb.AppendLine("  Nenhuma conexão registrada.");
            }
            else
            {
                foreach (var par in d.ContagemPorTipo.OrderByDescending(p => p.Value))
                    sb.AppendLine($"  • {RelacionamentoHelper.ObterNome(par.Key),-24}: {par.Value} conexão(ões)");
            }
            sb.AppendLine();

            sb.AppendLine("=== CATÁLOGO E GRAUS DAS FAIXAS ===");
            if (faixas.Count == 0)
            {
                sb.AppendLine("  Nenhuma música cadastrada.");
            }
            else if (EhDirecionado)
            {
                sb.AppendLine($"  {"#",-4} | {"Faixa Musical",-24} | {"Artista",-18} | {"Gênero",-12} | {"BPM",-4} | {"Entrada",-7} | {"Saída",-6} | {"Total",-5}");
                sb.AppendLine("  " + new string('-', 98));
                for (int i = 0; i < faixas.Count; i++)
                {
                    FaixaMusical f = faixas[i];
                    int ent = ObterGrauEntrada(f.Titulo);
                    int sai = ObterGrauSaida(f.Titulo);
                    sb.AppendLine($"  {$"M{i + 1}",-4} | {f.Titulo,-24} | {f.Artista,-18} | {f.Genero,-12} | {f.Bpm,-4} | {ent,-7} | {sai,-6} | {ent + sai,-5}");
                }
            }
            else
            {
                sb.AppendLine($"  {"#",-4} | {"Faixa Musical",-24} | {"Artista",-18} | {"Gênero",-12} | {"BPM",-4} | {"Grau",-5}");
                sb.AppendLine("  " + new string('-', 82));
                for (int i = 0; i < faixas.Count; i++)
                {
                    FaixaMusical f = faixas[i];
                    int grau = ObterGrauNaoDirecionado(f.Titulo);
                    sb.AppendLine($"  {$"M{i + 1}",-4} | {f.Titulo,-24} | {f.Artista,-18} | {f.Genero,-12} | {f.Bpm,-4} | {grau,-5}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("=== LISTA DE ADJACÊNCIA ===");
            if (faixas.Count == 0)
            {
                sb.AppendLine("  Nenhuma música cadastrada.");
            }
            else
            {
                string seta = EhDirecionado ? "→" : "↔";
                int larguraTitulo = Math.Max(18, faixas.Max(f => f.Titulo.Length));
                foreach (FaixaMusical f in faixas)
                {
                    List<ConexaoMusical> conexoes = _listaAdjacencia[f.Titulo];
                    string destinoTexto = conexoes.Count == 0
                        ? "(faixa isolada - sem conexões)"
                        : string.Join(", ", conexoes.Select(c =>
                            EhPonderado ? $"{c.Destino} [{c.Contexto}: {c.Peso}%]" : $"{c.Destino} ({c.Contexto})"));
                    sb.AppendLine($"  {f.Titulo.PadRight(larguraTitulo)} {seta} {destinoTexto}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("=== MATRIZ DE ADJACÊNCIA ===");
            if (faixas.Count == 0)
            {
                sb.AppendLine("  Nenhuma música cadastrada.");
            }
            else
            {
                var (codigos, celulas, larguraColuna) = ObterDadosMatriz(faixas);
                sb.Append("      ");
                foreach (string cod in codigos)
                    sb.Append(cod.PadLeft(larguraColuna));
                sb.AppendLine();

                for (int i = 0; i < faixas.Count; i++)
                {
                    sb.Append($"  {codigos[i],-4}");
                    for (int j = 0; j < faixas.Count; j++)
                        sb.Append(celulas[i, j].PadLeft(larguraColuna));
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }

    internal static class InterfaceConsole
    {
        public static void LimparTela()
        {
            if (Console.IsOutputRedirected)
                return;

            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
            }
        }

        public static void EscreverCabecalho(GrafoMusical grafo)
        {
            DiagnosticoGrafo d = grafo.GerarDiagnostico();
            string direcao = grafo.EhDirecionado ? "Direcionado (Dígrafo)" : "Não-Direcionado";
            string peso = grafo.EhPonderado ? "Ponderado (%)" : "Não-Ponderado";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================================================================");
            Console.WriteLine("                  SOUNDGRAPH PRO - RECOMENDAÇÃO MUSICAL EM GRAFOS              ");
            Console.WriteLine("===============================================================================");
            Console.ResetColor();
            Console.WriteLine($" Cenário : {Truncar(grafo.NomeCenario, 66)}");
            Console.WriteLine($" Modelo  : {direcao} | {peso} | {d.TipoEstruturalCurto}");
            Console.WriteLine($" Rede    : {d.TotalVertices} músicas (|V|) | {d.TotalArestas} conexões (|E|) | {d.Conectividade}");
            Console.WriteLine("-------------------------------------------------------------------------------");
        }

        public static void EscreverTitulo(string titulo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== {titulo} ===");
            Console.ResetColor();
        }

        public static void EscreverSucesso(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {msg}");
            Console.ResetColor();
        }

        public static void EscreverAlerta(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] {msg}");
            Console.ResetColor();
        }

        public static void Pausar(string mensagem = "Pressione qualquer tecla para voltar...")
        {
            if (Console.IsInputRedirected)
            {
                Console.WriteLine($"\n{mensagem}");
                Console.ReadLine();
                return;
            }

            Console.Write($"\n{mensagem}");
            Console.ReadKey(true);
        }

        public static int SelecionarOpcao(
            string titulo,
            IReadOnlyList<string> opcoes,
            string? subtitulo = null,
            bool permitirVoltar = true,
            string rotuloVoltar = "Voltar",
            GrafoMusical? cabecalhoGrafo = null,
            bool atalhoNumericoDireto = true)
        {
            if (opcoes.Count == 0)
                return -1;

            if (Console.IsInputRedirected)
                return SelecionarOpcaoRedirecionado(titulo, opcoes, subtitulo, permitirVoltar, rotuloVoltar, cabecalhoGrafo);

            int selecionado = 0;
            int offset = 0;
            const int maxVisiveis = 10;
            bool cursorAnterior = true;

            try
            {
                if (OperatingSystem.IsWindows())
                    cursorAnterior = Console.CursorVisible;
                Console.CursorVisible = false;
            }
            catch { }

            try
            {
                LimparTela();
                if (cabecalhoGrafo is not null)
                    EscreverCabecalho(cabecalhoGrafo);

                EscreverTitulo(titulo);
                if (!string.IsNullOrWhiteSpace(subtitulo))
                    Console.WriteLine($" {subtitulo}");
                Console.WriteLine();

                int linhaInicio = Console.CursorTop;
                int linhasVisiveis = Math.Min(maxVisiveis, opcoes.Count);

                while (true)
                {
                    if (selecionado < offset)
                        offset = selecionado;
                    else if (selecionado >= offset + linhasVisiveis)
                        offset = selecionado - linhasVisiveis + 1;

                    try { Console.SetCursorPosition(0, linhaInicio); } catch { }

                    for (int i = 0; i < linhasVisiveis; i++)
                    {
                        int idx = offset + i;
                        string prefixo = $"[{idx + 1,2}] ";
                        string texto = Truncar(prefixo + opcoes[idx], 75);

                        if (idx == selecionado)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write($" ► {texto.PadRight(75)}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write($"   {texto.PadRight(75)}");
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    string navIndicador = opcoes.Count > maxVisiveis
                        ? $" (Item {selecionado + 1}/{opcoes.Count} | PgUp/PgDn)"
                        : "";
                    string dicaVoltar = permitirVoltar ? $" | [0/ESC] {rotuloVoltar}" : "";
                    string dicaAtalho = atalhoNumericoDireto && opcoes.Count <= 9 ? " | [1-9] Atalho" : "";
                    Console.WriteLine($" [↑/↓] Mover | [ENTER] Confirmar{dicaAtalho}{dicaVoltar}{navIndicador}".PadRight(79));
                    Console.ResetColor();

                    ConsoleKeyInfo tecla = Console.ReadKey(true);
                    switch (tecla.Key)
                    {
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.W:
                            selecionado = (selecionado - 1 + opcoes.Count) % opcoes.Count;
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                            selecionado = (selecionado + 1) % opcoes.Count;
                            break;
                        case ConsoleKey.PageUp:
                            selecionado = Math.Max(0, selecionado - maxVisiveis);
                            break;
                        case ConsoleKey.PageDown:
                            selecionado = Math.Min(opcoes.Count - 1, selecionado + maxVisiveis);
                            break;
                        case ConsoleKey.Home:
                            selecionado = 0;
                            break;
                        case ConsoleKey.End:
                            selecionado = opcoes.Count - 1;
                            break;
                        case ConsoleKey.Enter:
                        case ConsoleKey.Spacebar:
                            return selecionado;
                        case ConsoleKey.Escape:
                            if (permitirVoltar)
                                return -1;
                            break;
                        case ConsoleKey.D0:
                        case ConsoleKey.NumPad0:
                            if (permitirVoltar)
                                return -1;
                            break;
                        default:
                            if (atalhoNumericoDireto && opcoes.Count <= 9 &&
                                tecla.KeyChar is >= '1' and <= '9')
                            {
                                int num = tecla.KeyChar - '1';
                                if (num < opcoes.Count)
                                    return num;
                            }
                            break;
                    }
                }
            }
            finally
            {
                Console.ResetColor();
                try { Console.CursorVisible = cursorAnterior; } catch { }
            }
        }

        private static int SelecionarOpcaoRedirecionado(
            string titulo,
            IReadOnlyList<string> opcoes,
            string? subtitulo,
            bool permitirVoltar,
            string rotuloVoltar,
            GrafoMusical? cabecalhoGrafo)
        {
            if (cabecalhoGrafo is not null)
                EscreverCabecalho(cabecalhoGrafo);

            EscreverTitulo(titulo);
            if (!string.IsNullOrWhiteSpace(subtitulo))
                Console.WriteLine($" {subtitulo}");

            for (int i = 0; i < opcoes.Count; i++)
                Console.WriteLine($" [{i + 1}] {opcoes[i]}");

            if (permitirVoltar)
                Console.WriteLine($" [0] {rotuloVoltar}");

            while (true)
            {
                Console.Write("\nEscolha: ");
                string? entrada = Console.ReadLine();
                if (entrada is null)
                    return -1;

                string limpo = entrada.Trim();
                if (permitirVoltar && limpo == "0")
                    return -1;

                if (int.TryParse(limpo, out int escolha) && escolha >= 1 && escolha <= opcoes.Count)
                    return escolha - 1;

                EscreverAlerta("Opção inválida.");
            }
        }

        public static string? SelecionarMusica(GrafoMusical grafo, string titulo, string? subtitulo = null)
        {
            IReadOnlyList<FaixaMusical> faixas = grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                EscreverAlerta("Nenhuma música cadastrada no grafo.");
                Pausar();
                return null;
            }

            var itens = faixas
                .Select((f, i) => $"M{i + 1,-2} — {f.Titulo} ({f.Artista} | {f.Genero} | {f.Bpm} BPM)")
                .ToList();

            int idx = SelecionarOpcao(
                titulo,
                itens,
                subtitulo ?? "Escolha a faixa usando as setas:",
                permitirVoltar: true,
                rotuloVoltar: "Cancelar",
                atalhoNumericoDireto: false);

            return idx < 0 ? null : faixas[idx].Titulo;
        }

        public static string GerarBarraAfinidade(int peso)
        {
            int cheios = Math.Clamp((int)Math.Round(peso / 10.0), 1, 10);
            return new string('█', cheios) + new string('░', 10 - cheios);
        }

        public static string Truncar(string texto, int larguraMax)
        {
            if (texto.Length <= larguraMax)
                return texto;

            return larguraMax <= 3 ? texto[..larguraMax] : texto[..(larguraMax - 3)] + "...";
        }
    }

    internal static class PlayerInterativo
    {
        public static void Executar(GrafoMusical grafo)
        {
            IReadOnlyList<FaixaMusical> faixas = grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                InterfaceConsole.EscreverAlerta("O grafo está vazio. Cadastre músicas antes de abrir o Player.");
                InterfaceConsole.Pausar();
                return;
            }

            string atual = EscolherFaixaInicial(grafo, faixas);
            var historico = new List<string> { atual };
            string aviso = "Navegue pelas conexões com [↑/↓ + ENTER] ou use [A] Auto-DJ.";
            int selecionado = 0;

            while (true)
            {
                FaixaMusical? faixa = grafo.ObterFaixa(atual);
                if (faixa is null)
                    return;

                var (diretas, descobertas, possuiRepeat) = grafo.RecomendarAPartirDe(atual);
                List<ConexaoMusical> conexoesVisiveis = diretas.Take(9).ToList();
                if (selecionado >= conexoesVisiveis.Count)
                    selecionado = Math.Max(0, conexoesVisiveis.Count - 1);

                RenderizarTelaPlayer(grafo, faixa, historico, conexoesVisiveis, diretas.Count, descobertas, possuiRepeat, selecionado, aviso);

                if (Console.IsInputRedirected)
                {
                    string? cmd = Console.ReadLine()?.Trim();
                    if (cmd is null || cmd == "0" || cmd.Equals("Q", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (cmd.Equals("A", StringComparison.OrdinalIgnoreCase))
                    {
                        ExecutarAutoDj(grafo, ref atual, historico, diretas, ref aviso, ref selecionado);
                    }
                    else if (cmd.Equals("R", StringComparison.OrdinalIgnoreCase))
                    {
                        var (_, msg) = grafo.AlternarModoRepeat(atual);
                        aviso = msg;
                    }
                    else if (int.TryParse(cmd, out int idx) && idx >= 1 && idx <= conexoesVisiveis.Count)
                    {
                        NavegarPara(grafo, ref atual, conexoesVisiveis[idx - 1], historico, ref aviso, ref selecionado);
                    }
                    continue;
                }

                ConsoleKeyInfo tecla = Console.ReadKey(true);
                switch (tecla.Key)
                {
                    case ConsoleKey.Escape:
                    case ConsoleKey.Q:
                        return;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        if (conexoesVisiveis.Count > 0)
                            selecionado = (selecionado - 1 + conexoesVisiveis.Count) % conexoesVisiveis.Count;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (conexoesVisiveis.Count > 0)
                            selecionado = (selecionado + 1) % conexoesVisiveis.Count;
                        break;
                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        if (conexoesVisiveis.Count > 0)
                            NavegarPara(grafo, ref atual, conexoesVisiveis[selecionado], historico, ref aviso, ref selecionado);
                        else
                            aviso = "Esta faixa não tem conexões de saída. Use [M] para pular ou [R] para Repeat.";
                        break;
                    case ConsoleKey.A:
                        ExecutarAutoDj(grafo, ref atual, historico, diretas, ref aviso, ref selecionado);
                        break;
                    case ConsoleKey.R:
                        var (_, msgRepeat) = grafo.AlternarModoRepeat(atual);
                        DiagnosticoGrafo diag = grafo.GerarDiagnostico();
                        aviso = $"{msgRepeat} | Estrutura atual: {diag.TipoEstruturalCurto}";
                        break;
                    case ConsoleKey.M:
                        string? escolhida = InterfaceConsole.SelecionarMusica(grafo, "PULAR PARA OUTRA MÚSICA", "Escolha a próxima faixa para tocar:");
                        if (escolhida is not null)
                        {
                            atual = escolhida;
                            historico.Add(atual);
                            selecionado = 0;
                            aviso = $"Pulou diretamente para '{atual}'.";
                        }
                        break;
                    case ConsoleKey.F:
                        string? destino = InterfaceConsole.SelecionarMusica(grafo, "TRAÇAR FILA A PARTIR DA FAIXA ATUAL", $"Origem fixa: {atual} — Escolha o destino:");
                        if (destino is not null)
                            Program.ExibirComparativoRotas(grafo, atual, destino);
                        break;
                    default:
                        if (tecla.KeyChar is >= '1' and <= '9')
                        {
                            int idx = tecla.KeyChar - '1';
                            if (idx < conexoesVisiveis.Count)
                                NavegarPara(grafo, ref atual, conexoesVisiveis[idx], historico, ref aviso, ref selecionado);
                        }
                        break;
                }
            }
        }

        private static string EscolherFaixaInicial(GrafoMusical grafo, IReadOnlyList<FaixaMusical> faixas)
        {
            return faixas
                .OrderByDescending(f => grafo.EhDirecionado ? grafo.ObterGrauSaida(f.Titulo) : grafo.ObterGrauNaoDirecionado(f.Titulo))
                .ThenBy(f => f.Titulo, StringComparer.OrdinalIgnoreCase)
                .First()
                .Titulo;
        }

        private static void NavegarPara(
            GrafoMusical grafo,
            ref string atual,
            ConexaoMusical conexao,
            List<string> historico,
            ref string aviso,
            ref int selecionado)
        {
            string origem = atual;
            atual = conexao.Destino;
            historico.Add(atual);
            selecionado = 0;

            string pesoTxt = grafo.EhPonderado ? $" ({conexao.Peso}%)" : "";
            aviso = $"Transição: {origem} → {atual} via {conexao.Contexto}{pesoTxt}";
        }

        private static void ExecutarAutoDj(
            GrafoMusical grafo,
            ref string atual,
            List<string> historico,
            List<ConexaoMusical> diretas,
            ref string aviso,
            ref int selecionado)
        {
            if (diretas.Count == 0)
            {
                aviso = "Auto-DJ sem conexões de saída nesta faixa. Use [M] para trocar.";
                return;
            }

            var recentes = historico.TakeLast(4).ToHashSet(StringComparer.OrdinalIgnoreCase);
            ConexaoMusical proxima = diretas.FirstOrDefault(c => !recentes.Contains(c.Destino)) ?? diretas[0];
            NavegarPara(grafo, ref atual, proxima, historico, ref aviso, ref selecionado);
            aviso = "[Auto-DJ] " + aviso;
        }

        private static void RenderizarTelaPlayer(
            GrafoMusical grafo,
            FaixaMusical faixa,
            List<string> historico,
            List<ConexaoMusical> conexoesVisiveis,
            int totalDiretas,
            List<(string Titulo, int Pontuacao)> descobertas,
            bool possuiRepeat,
            int selecionado,
            string aviso)
        {
            InterfaceConsole.LimparTela();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================================================================");
            Console.WriteLine("                    PLAYER INTERATIVO NO GRAFO (SOUNDGRAPH)                    ");
            Console.WriteLine("===============================================================================");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"  ♪ TOCANDO AGORA : {faixa.Titulo}");
            Console.ResetColor();
            if (possuiRepeat)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("  [REPEAT / AUTO-LAÇO ATIVO]");
                Console.ResetColor();
            }
            Console.WriteLine();

            string infoGrau = grafo.EhDirecionado
                ? $"Entrada: {grafo.ObterGrauEntrada(faixa.Titulo)} | Saída: {grafo.ObterGrauSaida(faixa.Titulo)}"
                : $"Grau: {grafo.ObterGrauNaoDirecionado(faixa.Titulo)}";

            Console.WriteLine($"    Artista: {faixa.Artista} | Gênero: {faixa.Genero} | Álbum: {faixa.Album} ({faixa.Ano}) | {faixa.Bpm} BPM");
            Console.WriteLine($"    Vértice: {infoGrau} | Cenário: {InterfaceConsole.Truncar(grafo.NomeCenario, 42)}");

            string seta = grafo.EhDirecionado ? " → " : " ↔ ";
            string trilha = string.Join(seta, historico.TakeLast(4));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    Trilha : {InterfaceConsole.Truncar(trilha, 69)}");
            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  PRÓXIMAS FAIXAS CONECTADAS (Arestas de saída: {totalDiretas}):");
            Console.ResetColor();

            if (conexoesVisiveis.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("    (Nenhuma aresta de saída — vértice sumidouro / isolado)");
                Console.ResetColor();
            }
            else
            {
                for (int i = 0; i < conexoesVisiveis.Count; i++)
                {
                    ConexaoMusical c = conexoesVisiveis[i];
                    string barra = grafo.EhPonderado
                        ? $"{InterfaceConsole.GerarBarraAfinidade(c.Peso)} {c.Peso,3}% "
                        : "";
                    string linha = $"[{i + 1}] {c.Destino,-22} {barra}({c.Contexto})";
                    linha = InterfaceConsole.Truncar(linha, 75);

                    if (i == selecionado)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($" ► {linha.PadRight(75)}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {linha.PadRight(75)}");
                    }
                }
            }

            if (descobertas.Count > 0)
            {
                string lista2Grau = string.Join(", ", descobertas.Take(4).Select(d => d.Titulo));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n  ★ Descobertas em 2º grau: {InterfaceConsole.Truncar(lista2Grau, 51)}");
                Console.ResetColor();
            }

            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  > {InterfaceConsole.Truncar(aviso, 75)}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [↑/↓+ENTER] Tocar  [A] Auto-DJ  [R] Repeat  [F] Fila  [M] Trocar  [ESC] Menu");
            Console.ResetColor();
        }
    }

    internal static class Program
    {
        private static GrafoMusical _grafo = CriarPresetMegaCatalogoStreaming();

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var opcoesMenu = new[]
            {
                "Player Interativo (Explorar o grafo tocando músicas ao vivo)",
                "Visualizar Grafo (Lista de Adjacência, Matriz ou Tabela de Graus)",
                "Diagnóstico de Teoria dos Grafos (Propriedades explicadas)",
                "Playlists e Filas (Playlist Automática ou Rota BFS vs. Dijkstra)",
                "Editar Grafo (Músicas, Conexões, Repeat e Auto-Conexão)",
                "Trocar Cenário Pronto (6 Presets) ou Criar Novo Grafo",
                "Exportar Relatório Completo (.TXT)"
            };

            while (true)
            {
                int escolha = InterfaceConsole.SelecionarOpcao(
                    "MENU PRINCIPAL",
                    opcoesMenu,
                    subtitulo: "Escolha uma funcionalidade:",
                    permitirVoltar: true,
                    rotuloVoltar: "Sair",
                    cabecalhoGrafo: _grafo);

                switch (escolha)
                {
                    case 0:
                        PlayerInterativo.Executar(_grafo);
                        break;
                    case 1:
                        MenuVisualizarGrafo();
                        break;
                    case 2:
                        ExibirDiagnosticoCompleto();
                        break;
                    case 3:
                        MenuPlaylistsEFilas();
                        break;
                    case 4:
                        MenuEditarGrafo();
                        break;
                    case 5:
                        MenuCenariosEPresets();
                        break;
                    case 6:
                        MenuExportarRelatorio();
                        break;
                    case -1:
                        InterfaceConsole.LimparTela();
                        Console.WriteLine("Encerrando o SoundGraph Pro. Até a próxima!");
                        return;
                }
            }
        }

        private static void MenuVisualizarGrafo()
        {
            var opcoes = new[]
            {
                "Lista de Adjacência (paginada, 6 faixas por página)",
                "Matriz de Adjacência (N × N colorida + opção de legenda)",
                "Catálogo Completo e Tabela de Graus"
            };

            while (true)
            {
                int escolha = InterfaceConsole.SelecionarOpcao(
                    "VISUALIZAR ESTRUTURA DO GRAFO",
                    opcoes,
                    cabecalhoGrafo: _grafo);

                switch (escolha)
                {
                    case 0:
                        ExibirListaAdjacenciaPaginada();
                        break;
                    case 1:
                        ExibirMatrizAdjacenciaInterativa();
                        break;
                    case 2:
                        ExibirCatalogoEGrausPaginado();
                        break;
                    case -1:
                        return;
                }
            }
        }

        private static void ExibirListaAdjacenciaPaginada()
        {
            IReadOnlyList<FaixaMusical> faixas = _grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverAlerta("Nenhuma música cadastrada no grafo.");
                InterfaceConsole.Pausar();
                return;
            }

            const int porPagina = 6;
            int totalPaginas = (int)Math.Ceiling(faixas.Count / (double)porPagina);
            int pagina = 0;
            string seta = _grafo.EhDirecionado ? "→" : "↔";

            while (true)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverTitulo($"LISTA DE ADJACÊNCIA (Página {pagina + 1}/{totalPaginas})");
                Console.WriteLine($"  Cada música (vértice) lista suas conexões ({seta} saída):\n");

                foreach (FaixaMusical f in faixas.Skip(pagina * porPagina).Take(porPagina))
                {
                    IReadOnlyList<ConexaoMusical> conexoes = _grafo.ObterConexoesDe(f.Titulo);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"  {f.Titulo}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  ({f.Artista} | {f.Bpm} BPM)");
                    Console.ResetColor();

                    if (conexoes.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("     └─ (sem conexões de saída)");
                        Console.ResetColor();
                    }
                    else
                    {
                        for (int i = 0; i < conexoes.Count; i++)
                        {
                            ConexaoMusical c = conexoes[i];
                            string galho = i == conexoes.Count - 1 ? "└─" : "├─";
                            bool ehLaco = c.Destino.Equals(f.Titulo, StringComparison.OrdinalIgnoreCase);
                            string simbolo = ehLaco ? "↻" : seta;
                            string peso = _grafo.EhPonderado ? $" | {c.Peso}%" : "";

                            Console.ForegroundColor = ehLaco ? ConsoleColor.Magenta : ConsoleColor.Green;
                            Console.Write($"     {galho} {simbolo} {c.Destino}");
                            Console.ResetColor();
                            Console.WriteLine($" ({c.Contexto}{peso})");
                        }
                    }
                }

                if (Console.IsInputRedirected)
                    return;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\n  [←/→] Mudar página  |  [ENTER / ESC] Voltar ");
                Console.ResetColor();

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.RightArrow || key == ConsoleKey.D || key == ConsoleKey.PageDown)
                    pagina = (pagina + 1) % totalPaginas;
                else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A || key == ConsoleKey.PageUp)
                    pagina = (pagina - 1 + totalPaginas) % totalPaginas;
                else if (key == ConsoleKey.Escape || key == ConsoleKey.Enter || key == ConsoleKey.D0)
                    return;
            }
        }

        private static void ExibirMatrizAdjacenciaInterativa()
        {
            IReadOnlyList<FaixaMusical> faixas = _grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverAlerta("Nenhuma música cadastrada no grafo.");
                InterfaceConsole.Pausar();
                return;
            }

            var (codigos, celulas, larguraColuna) = _grafo.ObterDadosMatriz(faixas);
            bool mostrarLegenda = faixas.Count <= 8;

            while (true)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverTitulo("MATRIZ DE ADJACÊNCIA");
                Console.WriteLine(_grafo.EhPonderado
                    ? "  Valores: '-' = sem conexão | número = peso (%) | 'N*' = arestas paralelas"
                    : "  Valores: '0' = sem conexão | '1' = conexão simples | '>1' = arestas paralelas");
                Console.WriteLine(_grafo.EhDirecionado
                    ? "  Direção: Dígrafo (ASSIMÉTRICA: linha → coluna) | Roxo = Auto-laço Repeat\n"
                    : "  Direção: Não-Direcionado (SIMÉTRICA: linha ↔ coluna) | Roxo = Auto-laço Repeat\n");

                Console.Write("      ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (string cod in codigos)
                    Console.Write(cod.PadLeft(larguraColuna));
                Console.ResetColor();
                Console.WriteLine();

                for (int i = 0; i < faixas.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"  {codigos[i],-4}");
                    Console.ResetColor();

                    for (int j = 0; j < faixas.Count; j++)
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

                if (mostrarLegenda)
                {
                    Console.WriteLine("\n  Legenda:");
                    for (int i = 0; i < faixas.Count; i += 2)
                    {
                        string esquerda = $"{codigos[i],-3} = {InterfaceConsole.Truncar(faixas[i].Titulo, 30)}";
                        string direita = i + 1 < faixas.Count
                            ? $"{codigos[i + 1],-3} = {InterfaceConsole.Truncar(faixas[i + 1].Titulo, 30)}"
                            : "";
                        Console.WriteLine($"    {esquerda,-37} {direita}");
                    }
                }

                if (Console.IsInputRedirected)
                    return;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\n  [L] {(mostrarLegenda ? "Ocultar" : "Mostrar")} legenda M1..M{faixas.Count}  |  [ENTER / ESC] Voltar ");
                Console.ResetColor();

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.L)
                    mostrarLegenda = !mostrarLegenda;
                else if (key == ConsoleKey.Escape || key == ConsoleKey.Enter || key == ConsoleKey.D0)
                    return;
            }
        }

        private static void ExibirCatalogoEGrausPaginado()
        {
            IReadOnlyList<FaixaMusical> faixas = _grafo.ObterFaixasOrdenadas();
            if (faixas.Count == 0)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverAlerta("Catálogo vazio.");
                InterfaceConsole.Pausar();
                return;
            }

            const int porPagina = 12;
            int totalPaginas = (int)Math.Ceiling(faixas.Count / (double)porPagina);
            int pagina = 0;

            while (true)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverTitulo($"CATÁLOGO E TABELA DE GRAUS (Página {pagina + 1}/{totalPaginas})");

                if (_grafo.EhDirecionado)
                {
                    Console.WriteLine($"  {"#",-4} | {"Título",-22} | {"Artista",-16} | {"BPM",-4} | {"Ent",-4} | {"Saí",-4} | {"Total",-5}");
                    Console.WriteLine("  " + new string('-', 76));

                    for (int idx = pagina * porPagina; idx < Math.Min(faixas.Count, (pagina + 1) * porPagina); idx++)
                    {
                        FaixaMusical f = faixas[idx];
                        int ent = _grafo.ObterGrauEntrada(f.Titulo);
                        int sai = _grafo.ObterGrauSaida(f.Titulo);
                        Console.WriteLine(
                            $"  {$"M{idx + 1}",-4} | {InterfaceConsole.Truncar(f.Titulo, 22),-22} | {InterfaceConsole.Truncar(f.Artista, 16),-16} | {f.Bpm,-4} | {ent,-4} | {sai,-4} | {ent + sai,-5}");
                    }

                    FaixaMusical maisAlcancada = faixas.OrderByDescending(f => _grafo.ObterGrauEntrada(f.Titulo)).First();
                    FaixaMusical maisRecomendadora = faixas.OrderByDescending(f => _grafo.ObterGrauSaida(f.Titulo)).First();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  Maior grau de entrada : {maisAlcancada.Titulo} ({_grafo.ObterGrauEntrada(maisAlcancada.Titulo)})");
                    Console.WriteLine($"  Maior grau de saída   : {maisRecomendadora.Titulo} ({_grafo.ObterGrauSaida(maisRecomendadora.Titulo)})");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {"#",-4} | {"Título",-22} | {"Artista",-16} | {"Gênero",-11} | {"BPM",-4} | {"Grau",-4}");
                    Console.WriteLine("  " + new string('-', 76));

                    for (int idx = pagina * porPagina; idx < Math.Min(faixas.Count, (pagina + 1) * porPagina); idx++)
                    {
                        FaixaMusical f = faixas[idx];
                        int grau = _grafo.ObterGrauNaoDirecionado(f.Titulo);
                        Console.WriteLine(
                            $"  {$"M{idx + 1}",-4} | {InterfaceConsole.Truncar(f.Titulo, 22),-22} | {InterfaceConsole.Truncar(f.Artista, 16),-16} | {InterfaceConsole.Truncar(f.Genero, 11),-11} | {f.Bpm,-4} | {grau,-4}");
                    }

                    FaixaMusical hub = faixas.OrderByDescending(f => _grafo.ObterGrauNaoDirecionado(f.Titulo)).First();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n  Faixa hub central (maior grau): {hub.Titulo} (grau {_grafo.ObterGrauNaoDirecionado(hub.Titulo)})");
                    Console.ResetColor();
                }

                if (Console.IsInputRedirected)
                    return;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("\n  [←/→] Mudar página  |  [ENTER / ESC] Voltar ");
                Console.ResetColor();

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.RightArrow || key == ConsoleKey.D || key == ConsoleKey.PageDown)
                    pagina = (pagina + 1) % totalPaginas;
                else if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A || key == ConsoleKey.PageUp)
                    pagina = (pagina - 1 + totalPaginas) % totalPaginas;
                else if (key == ConsoleKey.Escape || key == ConsoleKey.Enter || key == ConsoleKey.D0)
                    return;
            }
        }

        private static void ExibirDiagnosticoCompleto()
        {
            InterfaceConsole.LimparTela();
            DiagnosticoGrafo d = _grafo.GerarDiagnostico();

            InterfaceConsole.EscreverTitulo("DIAGNÓSTICO DE TEORIA DOS GRAFOS");
            Console.WriteLine($"  • Direção       : {(d.EhDirecionado ? "Direcionado (Dígrafo — arestas têm origem → destino)" : "Não-Direcionado (afinidade recíproca ↔)")}");
            Console.WriteLine($"  • Ponderação    : {(d.EhPonderado ? "Ponderado (arestas possuem peso de afinidade %)" : "Não-Ponderado (peso unitário)")}");
            Console.WriteLine($"  • Estrutura     : {d.TipoEstrutural}");
            Console.WriteLine($"  • Conectividade : {d.Conectividade} ({d.ComponentesConexas} componente(s))");
            Console.WriteLine($"      ↳ {(d.EhDirecionado ? "BFS no grafo original + grafo transposto verifica alcançabilidade mútua." : "BFS no grafo verifica se todos os vértices se alcançam.")}");
            Console.WriteLine($"  • Ciclicidade   : {(d.PossuiCiclo ? "Cíclico (DFS detectou retorno/loop)" : d.EhDirecionado ? "Acíclico — DAG (DFS sem arestas de retorno)" : "Acíclico (sem ciclos)")}");
            Console.WriteLine($"  • Completo (Kn) : {(d.EhCompleto ? $"Sim (K{d.TotalVertices} — todos os pares conectados)" : "Não")}");
            Console.WriteLine(d.EhDirecionado
                ? $"  • Grau Médio    : {d.GrauMedio:F2} total (Entrada = Saída = {d.GrauMedioDirecionadoPorSentido:F2})"
                : $"  • Grau Médio    : {d.GrauMedio:F2}");
            Console.WriteLine($"  • Densidade     : {d.DensidadePercentual:F1}% (pares distintos sem laço / máximo simples)");

            Console.WriteLine();
            InterfaceConsole.EscreverTitulo("RELACIONAMENTOS PRESENTES");
            if (d.ContagemPorTipo.Count == 0)
            {
                Console.WriteLine("  Nenhuma conexão registrada.");
            }
            else
            {
                var resumo = d.ContagemPorTipo
                    .OrderByDescending(p => p.Value)
                    .Select(p => $"{RelacionamentoHelper.ObterNome(p.Key)} ({p.Value})");
                foreach (var linha in QuebrarEmLinhas(string.Join(" | ", resumo), 74))
                    Console.WriteLine($"  {linha}");
            }

            InterfaceConsole.Pausar();
        }

        private static void MenuPlaylistsEFilas()
        {
            var opcoes = new[]
            {
                "Gerar Playlist Automática Contínua (Algoritmo Guloso)",
                "Traçar Fila entre 2 Músicas (Comparativo BFS vs. Dijkstra)"
            };

            while (true)
            {
                int escolha = InterfaceConsole.SelecionarOpcao(
                    "PLAYLISTS E FILAS DE TRANSIÇÃO",
                    opcoes,
                    cabecalhoGrafo: _grafo);

                switch (escolha)
                {
                    case 0:
                        MenuGerarPlaylistAutomatica();
                        break;
                    case 1:
                        MenuGerarFilaTransicao();
                        break;
                    case -1:
                        return;
                }
            }
        }

        private static void MenuGerarPlaylistAutomatica()
        {
            string? inicial = InterfaceConsole.SelecionarMusica(_grafo, "PLAYLIST AUTOMÁTICA (GULOSA)", "Escolha a música inicial:");
            if (inicial is null)
                return;

            var tamanhos = new[] { "4 faixas", "6 faixas", "8 faixas", "10 faixas" };
            int idxTam = InterfaceConsole.SelecionarOpcao(
                "TAMANHO DA PLAYLIST",
                tamanhos,
                subtitulo: $"Música inicial: {inicial}");
            if (idxTam < 0)
                return;

            int limite = (idxTam + 2) * 2;
            FaixaMusical? faixaInicial = _grafo.ObterFaixa(inicial);
            var passos = _grafo.GerarPlaylistAutomatica(inicial, limite);

            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo("PLAYLIST GERADA POR MAIOR AFINIDADE");
            Console.WriteLine($"\n  1. {faixaInicial?.Titulo} ({faixaInicial?.Artista}) [Início]");

            if (passos.Count == 0)
            {
                InterfaceConsole.EscreverAlerta("Não há conexões de saída disponíveis a partir dessa música.");
                InterfaceConsole.Pausar();
                return;
            }

            int somaPesos = 0;
            for (int i = 0; i < passos.Count; i++)
            {
                var passo = passos[i];
                FaixaMusical? dest = _grafo.ObterFaixa(passo.Destino);
                somaPesos += passo.Conexao.Peso;
                string infoPeso = _grafo.EhPonderado ? $" | {passo.Conexao.Peso}%" : "";
                Console.WriteLine($"  {i + 2}. {passo.Destino} ({dest?.Artista})  <-- {passo.Conexao.Contexto}{infoPeso}");
            }

            Console.WriteLine();
            if (_grafo.EhPonderado)
                InterfaceConsole.EscreverSucesso($"Playlist com {passos.Count + 1} faixas | Afinidade média: {(double)somaPesos / passos.Count:F1}%.");
            else
                InterfaceConsole.EscreverSucesso($"Playlist com {passos.Count + 1} faixas.");

            InterfaceConsole.Pausar();
        }

        private static void MenuGerarFilaTransicao()
        {
            string? orig = InterfaceConsole.SelecionarMusica(_grafo, "FILA DE TRANSIÇÃO (PASSO 1/2)", "Escolha a música de ORIGEM:");
            if (orig is null)
                return;

            string? dest = InterfaceConsole.SelecionarMusica(_grafo, "FILA DE TRANSIÇÃO (PASSO 2/2)", $"Origem: {orig} — Agora escolha o DESTINO:");
            if (dest is null)
                return;

            ExibirComparativoRotas(_grafo, orig, dest);
        }

        public static void ExibirComparativoRotas(GrafoMusical grafo, string orig, string dest)
        {
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo("COMPARATIVO DE ROTAS: BFS vs. DIJKSTRA");
            Console.WriteLine($"  Origem  : {orig}");
            Console.WriteLine($"  Destino : {dest}\n");

            List<string>? filaBfs = grafo.GerarFilaTransicao(orig, dest);
            if (filaBfs is null)
            {
                InterfaceConsole.EscreverAlerta($"Não existe caminho de transição de '{orig}' até '{dest}' neste grafo.");
                InterfaceConsole.Pausar();
                return;
            }

            string seta = grafo.EhDirecionado ? " → " : " ↔ ";
            InterfaceConsole.EscreverSucesso($"1) Menor caminho em saltos (BFS — {filaBfs.Count - 1} transição(ões)):");
            Console.WriteLine($"   {string.Join(seta, filaBfs)}");

            if (grafo.EhPonderado)
            {
                var melhorRota = grafo.GerarFilaTransicaoDijkstra(orig, dest);
                if (melhorRota is not null)
                {
                    var (caminhoDijkstra, passos, afinidadeMedia) = melhorRota.Value;
                    Console.WriteLine();
                    InterfaceConsole.EscreverSucesso($"2) Rota de maior afinidade (Dijkstra — {passos.Count} transição(ões) | Média: {afinidadeMedia:F1}%):");
                    Console.WriteLine($"   {string.Join(seta, caminhoDijkstra)}");

                    if (passos.Count > 0)
                    {
                        Console.WriteLine();
                        foreach (var p in passos)
                            Console.WriteLine($"     • {p.Origem} {seta.Trim()} {p.Destino} ({p.Contexto} — {p.Peso}%)");
                    }
                }
            }

            InterfaceConsole.Pausar();
        }

        private static void MenuEditarGrafo()
        {
            var opcoes = new[]
            {
                "Cadastrar nova música (Vértice)",
                "Conectar duas músicas (Escolher relacionamento e peso)",
                "Ligar / Desligar Modo Repeat em uma música (Auto-laço)",
                "Auto-conectar catálogo por regra (Gênero, Artista ou BPM)",
                "Filtrar conexões por tipo de relacionamento",
                "Remover música ou conexão"
            };

            while (true)
            {
                int escolha = InterfaceConsole.SelecionarOpcao(
                    "EDITAR GRAFO MUSICAL",
                    opcoes,
                    cabecalhoGrafo: _grafo);

                switch (escolha)
                {
                    case 0:
                        MenuAdicionarFaixa();
                        break;
                    case 1:
                        MenuAdicionarConexao();
                        break;
                    case 2:
                        MenuAlternarModoRepeat();
                        break;
                    case 3:
                        MenuAutoConectarPorRegras();
                        break;
                    case 4:
                        MenuFiltrarPorRelacionamento();
                        break;
                    case 5:
                        MenuRemoverItem();
                        break;
                    case -1:
                        return;
                }
            }
        }

        private static void MenuAdicionarFaixa()
        {
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo("CADASTRAR NOVA MÚSICA (VÉRTICE)");
            Console.Write("Título da música (obrigatório)   : ");
            string titulo = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(titulo))
            {
                InterfaceConsole.EscreverAlerta("Cadastro cancelado.");
                InterfaceConsole.Pausar();
                return;
            }

            Console.Write("Artista / Banda                  : ");
            string artista = (Console.ReadLine() ?? "").Trim();
            Console.Write("Gênero musical                   : ");
            string genero = (Console.ReadLine() ?? "").Trim();
            Console.Write("Álbum (Enter para Single)        : ");
            string album = (Console.ReadLine() ?? "").Trim();
            Console.Write("Ano (Enter para 2020)            : ");
            if (!int.TryParse(Console.ReadLine(), out int ano))
                ano = 2020;
            Console.Write("BPM 40..260 (Enter para 120)     : ");
            if (!int.TryParse(Console.ReadLine(), out int bpm))
                bpm = 120;

            if (_grafo.AdicionarFaixa(titulo, artista, genero, album, ano, bpm))
                InterfaceConsole.EscreverSucesso($"Música '{titulo}' adicionada ao catálogo.");
            else
                InterfaceConsole.EscreverAlerta("Título inválido ou música já cadastrada.");

            InterfaceConsole.Pausar();
        }

        private static TipoRelacionamento? EscolherTipoRelacionamento()
        {
            TipoRelacionamento[] tipos = Enum.GetValues<TipoRelacionamento>();
            var nomes = tipos.Select(RelacionamentoHelper.ObterNome).ToList();

            int idx = InterfaceConsole.SelecionarOpcao(
                "ESCOLHER TIPO DE RELACIONAMENTO",
                nomes,
                subtitulo: "Qual vínculo liga essas músicas?",
                atalhoNumericoDireto: false);

            return idx < 0 ? null : tipos[idx];
        }

        private static void MenuAdicionarConexao()
        {
            string? orig = InterfaceConsole.SelecionarMusica(_grafo, "CONECTAR MÚSICAS (PASSO 1/3)", "Escolha a música de ORIGEM:");
            if (orig is null)
                return;

            string? dest = InterfaceConsole.SelecionarMusica(_grafo, "CONECTAR MÚSICAS (PASSO 2/3)", $"Origem: {orig} — Escolha o DESTINO (mesma para Repeat):");
            if (dest is null)
                return;

            TipoRelacionamento? tipo = EscolherTipoRelacionamento();
            if (tipo is null)
                return;

            string? contextoPersonalizado = null;
            if (tipo == TipoRelacionamento.Personalizado)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverTitulo("RÓTULO PERSONALIZADO");
                Console.Write("Digite o nome do relacionamento: ");
                contextoPersonalizado = (Console.ReadLine() ?? "").Trim();
            }

            int peso = 1;
            if (_grafo.EhPonderado)
            {
                var pesos = new[] { "95% — Muito alta", "85% — Alta", "70% — Média", "50% — Moderada", "Digitar valor manual (1..100)" };
                int idxPeso = InterfaceConsole.SelecionarOpcao(
                    "CONECTAR MÚSICAS (AFINIDADE)",
                    pesos,
                    subtitulo: $"{orig} → {dest}");
                if (idxPeso < 0)
                    return;

                peso = idxPeso switch
                {
                    0 => 95,
                    1 => 85,
                    2 => 70,
                    3 => 50,
                    _ => LerPesoManual()
                };
            }

            var (sucesso, mensagem) = _grafo.AdicionarConexao(orig, dest, peso, tipo.Value, contextoPersonalizado);
            InterfaceConsole.LimparTela();
            if (sucesso)
                InterfaceConsole.EscreverSucesso(mensagem);
            else
                InterfaceConsole.EscreverAlerta(mensagem);

            InterfaceConsole.Pausar();
        }

        private static int LerPesoManual()
        {
            Console.Write("\nDigite a afinidade (1 a 100): ");
            if (int.TryParse(Console.ReadLine(), out int peso) && peso >= 1 && peso <= 100)
                return peso;

            return 75;
        }

        private static void MenuAlternarModoRepeat()
        {
            string? titulo = InterfaceConsole.SelecionarMusica(_grafo, "MODO REPEAT (AUTO-LAÇO)", "Escolha a música para ligar ou desligar o Repeat:");
            if (titulo is null)
                return;

            var (_, mensagem) = _grafo.AlternarModoRepeat(titulo);
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverSucesso(mensagem);
            Console.WriteLine($"  Tipo estrutural atual: {_grafo.GerarDiagnostico().TipoEstrutural}");
            InterfaceConsole.Pausar();
        }

        private static void MenuAutoConectarPorRegras()
        {
            var regras = new[]
            {
                "Mesmo Gênero (conecta faixas de estilos iguais)",
                "Mesmo Artista (conecta faixas do mesmo artista)",
                "BPM Compatível (diferença de até 8 BPM)"
            };

            int idx = InterfaceConsole.SelecionarOpcao("AUTO-CONECTAR POR REGRAS", regras);
            if (idx < 0)
                return;

            int criadas = _grafo.AutoConectarPorRegra(idx + 1);
            InterfaceConsole.LimparTela();
            if (criadas > 0)
                InterfaceConsole.EscreverSucesso($"{criadas} nova(s) conexão(ões) gerada(s) automaticamente!");
            else
                InterfaceConsole.EscreverAlerta("Nenhuma nova conexão foi necessária para esta regra.");

            InterfaceConsole.Pausar();
        }

        private static void MenuFiltrarPorRelacionamento()
        {
            TipoRelacionamento? tipo = EscolherTipoRelacionamento();
            if (tipo is null)
                return;

            var encontradas = _grafo.FiltrarConexoesPorTipo(tipo.Value);
            string seta = _grafo.EhDirecionado ? "→" : "↔";

            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo($"FILTRO: {RelacionamentoHelper.ObterNome(tipo.Value).ToUpperInvariant()}");
            Console.WriteLine($"  Total encontrado: {encontradas.Count} conexão(ões)\n");

            if (encontradas.Count == 0)
            {
                Console.WriteLine("  Nenhuma conexão desse tipo cadastrada no cenário atual.");
            }
            else
            {
                foreach (var (origem, conexao) in encontradas)
                {
                    string peso = _grafo.EhPonderado ? $" [{conexao.Peso}%]" : "";
                    Console.WriteLine($"  ♪ {origem} {seta} {conexao.Destino}{peso}");
                }
            }

            InterfaceConsole.Pausar();
        }

        private static void MenuRemoverItem()
        {
            var opcoes = new[]
            {
                "Remover uma Música (vértice e todas as suas conexões)",
                "Remover uma Conexão específica entre duas músicas"
            };

            int op = InterfaceConsole.SelecionarOpcao("REMOVER DO GRAFO", opcoes);
            if (op < 0)
                return;

            if (op == 0)
            {
                string? titulo = InterfaceConsole.SelecionarMusica(_grafo, "REMOVER MÚSICA", "Escolha a música para remover:");
                if (titulo is null)
                    return;

                InterfaceConsole.LimparTela();
                if (_grafo.RemoverFaixa(titulo))
                    InterfaceConsole.EscreverSucesso($"Música '{titulo}' removida com sucesso.");
                else
                    InterfaceConsole.EscreverAlerta("Música não encontrada.");

                InterfaceConsole.Pausar();
                return;
            }

            string? orig = InterfaceConsole.SelecionarMusica(_grafo, "REMOVER CONEXÃO (ORIGEM)", "Escolha a música de origem:");
            if (orig is null)
                return;

            IReadOnlyList<ConexaoMusical> saidas = _grafo.ObterConexoesDe(orig);
            if (saidas.Count == 0)
            {
                InterfaceConsole.LimparTela();
                InterfaceConsole.EscreverAlerta($"'{orig}' não possui conexões de saída.");
                InterfaceConsole.Pausar();
                return;
            }

            var itens = saidas.Select(c =>
            {
                string peso = _grafo.EhPonderado ? $" [{c.Peso}%]" : "";
                return $"{orig} → {c.Destino} ({c.Contexto}{peso})";
            }).ToList();

            int idx = InterfaceConsole.SelecionarOpcao(
                "REMOVER CONEXÃO (ESCOLHA)",
                itens,
                subtitulo: $"Conexões a partir de {orig}:",
                atalhoNumericoDireto: false);
            if (idx < 0)
                return;

            ConexaoMusical escolhida = saidas[idx];
            int indiceEntrePar = saidas
                .Take(idx)
                .Count(c => c.Destino.Equals(escolhida.Destino, StringComparison.OrdinalIgnoreCase));

            InterfaceConsole.LimparTela();
            if (_grafo.RemoverConexaoPorIndice(orig, escolhida.Destino, indiceEntrePar))
                InterfaceConsole.EscreverSucesso("Conexão removida com sucesso.");
            else
                InterfaceConsole.EscreverAlerta("Conexão não encontrada.");

            InterfaceConsole.Pausar();
        }

        private static void MenuCenariosEPresets()
        {
            var opcoes = new[]
            {
                "Mega Catálogo de Streaming   (18 faixas | Dígrafo Ponderado | Multi+Pseudo)",
                "Ecossistema Pop & Synth      (12 faixas | Não-Direcionado | Simples Conexo)",
                "Histórico Global de Fila     (12 faixas | Dígrafo Cíclico e Desconexo)",
                "Álbum Conceitual Completo K6 ( 6 faixas | Grafo Completo K6 — 100% denso)",
                "Setlist de Festival (DAG)    (10 faixas | Dígrafo Ponderado Acíclico DAG)",
                "Mega Mix Treino + Repeat     (10 faixas | Não-Direcionado Multi+Pseudo)",
                "Criar Novo Grafo Musical do Zero (escolher direção e ponderação)"
            };

            int escolha = InterfaceConsole.SelecionarOpcao(
                "CENÁRIOS PRONTOS E CRIAÇÃO DE GRAFO",
                opcoes,
                cabecalhoGrafo: _grafo);

            if (escolha < 0)
                return;

            if (escolha == 6)
            {
                MenuCriarGrafoPersonalizado();
                return;
            }

            _grafo = escolha switch
            {
                0 => CriarPresetMegaCatalogoStreaming(),
                1 => CriarPresetEcossistemaPopSimples(),
                2 => CriarPresetHistoricoGlobalDesconexo(),
                3 => CriarPresetAlbumCompletoK6(),
                4 => CriarPresetSetlistFestivalDag(),
                _ => CriarPresetMegaMixMultigrafoPseudografo()
            };

            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverSucesso($"Cenário carregado: {_grafo.NomeCenario}");
            InterfaceConsole.Pausar();
        }

        private static void MenuCriarGrafoPersonalizado()
        {
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo("CRIAR NOVO GRAFO MUSICAL DO ZERO");
            Console.Write("Nome do cenário (Enter para Playlist Personalizada): ");
            string nome = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nome))
                nome = "Playlist Personalizada";

            int idxDir = InterfaceConsole.SelecionarOpcao(
                "DIREÇÃO DAS ARESTAS",
                new[] { "Direcionado (Dígrafo: origem → destino)", "Não-Direcionado (afinidade mútua: A ↔ B)" },
                subtitulo: $"Cenário: {nome}");
            if (idxDir < 0)
                return;

            int idxPeso = InterfaceConsole.SelecionarOpcao(
                "PONDERAÇÃO DAS ARESTAS",
                new[] { "Ponderado (com afinidade de 1% a 100%)", "Não-Ponderado (peso unitário)" },
                subtitulo: $"Cenário: {nome}");
            if (idxPeso < 0)
                return;

            _grafo = new GrafoMusical(nome, idxDir == 0, idxPeso == 0);
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverSucesso($"Novo grafo '{nome}' criado! Use 'Editar Grafo' para adicionar músicas e conexões.");
            InterfaceConsole.Pausar();
        }

        private static void MenuExportarRelatorio()
        {
            InterfaceConsole.LimparTela();
            InterfaceConsole.EscreverTitulo("EXPORTAR RELATÓRIO COMPLETO (.TXT)");
            string caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "relatorio_soundgraph.txt");
            string conteudo = _grafo.GerarRelatorioTexto();
            File.WriteAllText(caminhoArquivo, conteudo, Encoding.UTF8);
            InterfaceConsole.EscreverSucesso($"Relatório salvo em:\n  {caminhoArquivo}");
            InterfaceConsole.Pausar();
        }

        private static IEnumerable<string> QuebrarEmLinhas(string texto, int larguraMax)
        {
            var palavras = texto.Split(' ');
            var linhaAtual = new StringBuilder();

            foreach (string p in palavras)
            {
                if (linhaAtual.Length + p.Length + 1 > larguraMax && linhaAtual.Length > 0)
                {
                    yield return linhaAtual.ToString();
                    linhaAtual.Clear();
                }

                if (linhaAtual.Length > 0)
                    linhaAtual.Append(' ');
                linhaAtual.Append(p);
            }

            if (linhaAtual.Length > 0)
                yield return linhaAtual.ToString();
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
    }
}
