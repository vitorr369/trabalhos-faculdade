using System.Diagnostics;
using System.Text;
using SoundGraph;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5180");

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

object syncLock = new();
GrafoMusical grafoAtual = PresetsCatalogo.CriarPreset(1);
int presetAtualId = 1;

app.MapGet("/api/estado", () =>
{
    lock (syncLock)
    {
        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapGet("/api/recomendar", (string musica) =>
{
    lock (syncLock)
    {
        FaixaMusical? faixa = grafoAtual.ObterFaixa(musica);
        if (faixa is null)
            return Results.BadRequest(new { erro = "Música não encontrada." });

        var (diretas, descobertas, possuiRepeat) = grafoAtual.RecomendarAPartirDe(faixa.Titulo);
        return Results.Ok(new
        {
            atual = faixa,
            possuiRepeat,
            diretas = diretas.Select(c => new
            {
                destino = c.Destino,
                artista = grafoAtual.ObterFaixa(c.Destino)?.Artista ?? "",
                peso = c.Peso,
                tipo = (int)c.Tipo,
                contexto = c.Contexto
            }),
            descobertas = descobertas.Select(d => new
            {
                titulo = d.Titulo,
                artista = grafoAtual.ObterFaixa(d.Titulo)?.Artista ?? "",
                pontuacao = d.Pontuacao
            })
        });
    }
});

app.MapGet("/api/rota", (string origem, string destino) =>
{
    lock (syncLock)
    {
        List<string>? caminhoBfs = grafoAtual.GerarFilaTransicao(origem, destino);
        if (caminhoBfs is null)
        {
            return Results.Ok(new
            {
                encontrou = false,
                mensagem = $"Não existe caminho conectando '{origem}' até '{destino}' neste grafo."
            });
        }

        var djk = grafoAtual.EhPonderado ? grafoAtual.GerarFilaTransicaoDijkstra(origem, destino) : null;

        return Results.Ok(new
        {
            encontrou = true,
            ehPonderado = grafoAtual.EhPonderado,
            bfs = new
            {
                caminho = caminhoBfs,
                transicoes = Math.Max(0, caminhoBfs.Count - 1)
            },
            dijkstra = djk is null ? null : new
            {
                caminho = djk.Value.Caminho,
                transicoes = djk.Value.Passos.Count,
                afinidadeMedia = Math.Round(djk.Value.AfinidadeMedia, 1),
                passos = djk.Value.Passos.Select(p => new
                {
                    origem = p.Origem,
                    destino = p.Destino,
                    contexto = p.Contexto,
                    peso = p.Peso
                })
            }
        });
    }
});

app.MapGet("/api/playlist", (string inicio, int tamanho = 6) =>
{
    lock (syncLock)
    {
        FaixaMusical? faixaInicial = grafoAtual.ObterFaixa(inicio);
        if (faixaInicial is null)
            return Results.BadRequest(new { erro = "Música inicial não encontrada." });

        int lim = Math.Clamp(tamanho, 2, 15);
        var passos = grafoAtual.GerarPlaylistAutomatica(faixaInicial.Titulo, lim);
        var ordem = new List<string> { faixaInicial.Titulo };
        ordem.AddRange(passos.Select(p => p.Destino));

        double media = passos.Count == 0 ? 0 : Math.Round(passos.Average(p => p.Conexao.Peso), 1);

        return Results.Ok(new
        {
            inicio = faixaInicial.Titulo,
            ordem,
            afinidadeMedia = media,
            passos = passos.Select(p => new
            {
                origem = p.Origem,
                destino = p.Destino,
                contexto = p.Conexao.Contexto,
                peso = p.Conexao.Peso
            })
        });
    }
});

app.MapPost("/api/preset/{id:int}", (int id) =>
{
    lock (syncLock)
    {
        grafoAtual = PresetsCatalogo.CriarPreset(id);
        presetAtualId = id;
        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapPost("/api/novo", (NovoGrafoReq req) =>
{
    lock (syncLock)
    {
        string nome = string.IsNullOrWhiteSpace(req.Nome) ? "Grafo Personalizado" : req.Nome.Trim();
        grafoAtual = new GrafoMusical(nome, req.EhDirecionado, req.EhPonderado);
        presetAtualId = 0;
        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapPost("/api/musica", (NovaMusicaReq req) =>
{
    lock (syncLock)
    {
        if (!grafoAtual.AdicionarFaixa(req.Titulo, req.Artista, req.Genero, req.Album, req.Ano, req.Bpm))
            return Results.BadRequest(new { erro = "Título inválido ou música já cadastrada." });

        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapDelete("/api/musica", (string titulo) =>
{
    lock (syncLock)
    {
        if (!grafoAtual.RemoverFaixa(titulo))
            return Results.BadRequest(new { erro = "Música não encontrada." });

        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapPost("/api/conexao", (NovaConexaoReq req) =>
{
    lock (syncLock)
    {
        TipoRelacionamento tipo = Enum.IsDefined(typeof(TipoRelacionamento), req.Tipo)
            ? (TipoRelacionamento)req.Tipo
            : TipoRelacionamento.AfinidadeSonora;

        var (ok, msg) = grafoAtual.AdicionarConexao(req.Origem, req.Destino, req.Peso, tipo, req.Contexto);
        if (!ok)
            return Results.BadRequest(new { erro = msg });

        return Results.Ok(new { mensagem = msg, estado = grafoAtual.ExportarEstadoDto(presetAtualId) });
    }
});

app.MapPost("/api/repeat", (RepeatReq req) =>
{
    lock (syncLock)
    {
        var (ativado, msg) = grafoAtual.AlternarModoRepeat(req.Titulo);
        return Results.Ok(new { ativado, mensagem = msg, estado = grafoAtual.ExportarEstadoDto(presetAtualId) });
    }
});

app.MapPost("/api/autoconectar/{regra:int}", (int regra) =>
{
    lock (syncLock)
    {
        if (regra is < 1 or > 3)
            return Results.BadRequest(new { erro = "Regra inválida." });

        int criadas = grafoAtual.AutoConectarPorRegra(regra);
        return Results.Ok(new
        {
            criadas,
            mensagem = criadas > 0
                ? $"{criadas} conexão(ões) criada(s) automaticamente!"
                : "Nenhuma nova conexão necessária para esta regra.",
            estado = grafoAtual.ExportarEstadoDto(presetAtualId)
        });
    }
});

app.MapDelete("/api/conexao", (string origem, string destino, int indice = 0) =>
{
    lock (syncLock)
    {
        if (!grafoAtual.RemoverConexaoPorIndice(origem, destino, indice))
            return Results.BadRequest(new { erro = "Conexão não encontrada." });

        return Results.Ok(grafoAtual.ExportarEstadoDto(presetAtualId));
    }
});

app.MapGet("/api/relatorio", () =>
{
    lock (syncLock)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(grafoAtual.GerarRelatorioTexto());
        return Results.File(bytes, "text/plain; charset=utf-8", "relatorio_soundgraph.txt");
    }
});

bool abrirNavegador = !args.Contains("--no-open", StringComparer.OrdinalIgnoreCase);
app.Lifetime.ApplicationStarted.Register(() =>
{
    const string url = "http://localhost:5180";
    Console.WriteLine("==================================================================");
    Console.WriteLine($" SoundGraph Pro iniciado! Acesse no navegador: {url}");
    Console.WriteLine(" Pressione Ctrl+C no terminal para encerrar o servidor.");
    Console.WriteLine("==================================================================");

    if (abrirNavegador)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
        }
    }
});

app.Run();

namespace SoundGraph
{
    public record NovoGrafoReq(string Nome, bool EhDirecionado, bool EhPonderado);
    public record NovaMusicaReq(string Titulo, string Artista, string Genero, string Album, int Ano, int Bpm);
    public record NovaConexaoReq(string Origem, string Destino, int Peso, int Tipo, string? Contexto);
    public record RepeatReq(string Titulo);

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

        public FaixaMusical(string titulo, string artista, string genero, string album = "Single", int ano = 2020, int bpm = 120)
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
        public string ExplicacaoEstrutura { get; init; } = "";
        public string Conectividade { get; init; } = "";
        public int ComponentesConexas { get; init; }
        public string ExplicacaoConectividade { get; init; } = "";
        public bool PossuiCiclo { get; init; }
        public string CiclicidadeTexto { get; init; } = "";
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

        private readonly Dictionary<string, FaixaMusical> _faixas = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<ConexaoMusical>> _listaAdjacencia = new(StringComparer.OrdinalIgnoreCase);

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
            string limpo = (titulo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(limpo))
                return null;

            return _faixas.TryGetValue(limpo, out FaixaMusical? faixa) ? faixa.Titulo : null;
        }

        public FaixaMusical? ObterFaixa(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            return canonico is null ? null : _faixas[canonico];
        }

        public bool PossuiModoRepeat(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            return canonico is not null &&
                   _listaAdjacencia[canonico].Any(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));
        }

        public (bool Ativado, string Mensagem) AlternarModoRepeat(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return (false, "Música não encontrada.");

            if (PossuiModoRepeat(canonico))
            {
                _listaAdjacencia[canonico].RemoveAll(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));
                return (false, $"Repeat desativado em '{canonico}'.");
            }

            AdicionarConexao(canonico, canonico, 100, TipoRelacionamento.ModoRepeat);
            return (true, $"Repeat ativado em '{canonico}' (auto-laço adicionado -> Pseudografo).");
        }

        public bool AdicionarFaixa(string titulo, string artista, string genero, string album = "Single", int ano = 2020, int bpm = 120)
        {
            string tituloLimpo = (titulo ?? "").Trim();
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
                conexoes.RemoveAll(c => c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase));

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
                return (false, "Escolha músicas válidas de origem e destino.");

            bool ehLaco = orig.Equals(dest, StringComparison.OrdinalIgnoreCase);
            int pesoFinal = EhPonderado ? Math.Clamp(peso, 1, 100) : 1;
            TipoRelacionamento tipoFinal = ehLaco && tipo == TipoRelacionamento.AfinidadeSonora
                ? TipoRelacionamento.ModoRepeat
                : tipo;

            var novaConexao = new ConexaoMusical(dest, pesoFinal, tipoFinal, contextoPersonalizado);
            _listaAdjacencia[orig].Add(novaConexao);

            if (!EhDirecionado && !ehLaco)
                _listaAdjacencia[dest].Add(new ConexaoMusical(orig, pesoFinal, tipoFinal, contextoPersonalizado));

            string seta = EhDirecionado ? "→" : "↔";
            string textoPeso = EhPonderado ? $" ({pesoFinal}%)" : "";
            return (true, $"Conexão criada: {orig} {seta} {dest} [{novaConexao.Contexto}]{textoPeso}");
        }

        public bool RemoverConexaoPorIndice(string origem, string destino, int indiceEntrePar)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);
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

                    if (!EhDirecionado && !orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
                    {
                        List<ConexaoMusical> listaDest = _listaAdjacencia[dest];
                        int idxRev = listaDest.FindIndex(c =>
                            c.Destino.Equals(orig, StringComparison.OrdinalIgnoreCase) &&
                            c.Peso == removida.Peso &&
                            c.Tipo == removida.Tipo &&
                            c.Contexto.Equals(removida.Contexto, StringComparison.OrdinalIgnoreCase));

                        if (idxRev == -1)
                            idxRev = listaDest.FindIndex(c => c.Destino.Equals(orig, StringComparison.OrdinalIgnoreCase));

                        if (idxRev != -1)
                            listaDest.RemoveAt(idxRev);
                    }
                    return true;
                }
                encontrados++;
            }

            return false;
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
                        criadas += ConectarRegraSimetrica(a.Titulo, b.Titulo, 78, TipoRelacionamento.MesmoGenero);
                    else if (tipoRegra == 2 && a.Artista.Equals(b.Artista, StringComparison.OrdinalIgnoreCase))
                        criadas += ConectarRegraSimetrica(a.Titulo, b.Titulo, 92, TipoRelacionamento.MesmoArtista);
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
                foreach (ConexaoMusical c in par.Value)
                {
                    if (c.Destino.Equals(par.Key, StringComparison.OrdinalIgnoreCase))
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

            return _listaAdjacencia[canonico].Sum(c =>
                c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase) ? 2 : 1);
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

        public bool PossuiLacos() =>
            _listaAdjacencia.Any(par =>
                par.Value.Any(c => c.Destino.Equals(par.Key, StringComparison.OrdinalIgnoreCase)));

        public bool PossuiArestasParalelas() =>
            _listaAdjacencia.Values.Any(lista =>
                lista.GroupBy(c => c.Destino, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1));

        public bool EhCompleto()
        {
            int n = _faixas.Count;
            if (n < 2 || PossuiLacos() || PossuiArestasParalelas())
                return false;

            return _listaAdjacencia.Values.All(lista =>
                lista.Select(c => c.Destino).Distinct(StringComparer.OrdinalIgnoreCase).Count() == n - 1);
        }

        public Dictionary<string, int> MapearComponentesSubjacentes()
        {
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

            var mapa = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int idComponente = 0;

            foreach (FaixaMusical faixa in ObterFaixasOrdenadas())
            {
                if (mapa.ContainsKey(faixa.Titulo))
                    continue;

                idComponente++;
                var fila = new Queue<string>();
                fila.Enqueue(faixa.Titulo);
                mapa[faixa.Titulo] = idComponente;

                while (fila.Count > 0)
                {
                    string atual = fila.Dequeue();
                    foreach (string viz in subjacente[atual])
                    {
                        if (!mapa.ContainsKey(viz))
                        {
                            mapa[viz] = idComponente;
                            fila.Enqueue(viz);
                        }
                    }
                }
            }

            return mapa;
        }

        public (string Classificacao, int Componentes, string Explicacao) VerificarConectividade()
        {
            int n = _faixas.Count;
            if (n == 0)
                return ("Grafo Vazio", 0, "Nenhuma música cadastrada.");
            if (n == 1)
                return ("Conexo", 1, "Apenas 1 vértice no grafo.");

            var mapa = MapearComponentesSubjacentes();
            int componentes = mapa.Values.DefaultIfEmpty(0).Max();

            if (!EhDirecionado)
            {
                return componentes == 1
                    ? ("Conexo", 1, "Existe caminho entre quaisquer duas músicas da rede.")
                    : ("Desconexo", componentes, $"O grafo está dividido em {componentes} grupos (ilhas) sem ligação entre si.");
            }

            if (componentes > 1)
                return ("Desconexo", componentes, $"Mesmo ignorando o sentido das setas, existem {componentes} ilhas isoladas.");

            string inicio = _faixas.Keys.First();
            bool alcancaOriginal = BfsDirecionado(inicio, _listaAdjacencia).Count == n;

            var transposto = new Dictionary<string, List<ConexaoMusical>>(StringComparer.OrdinalIgnoreCase);
            foreach (string v in _faixas.Keys)
                transposto[v] = new List<ConexaoMusical>();

            foreach (var par in _listaAdjacencia)
                foreach (ConexaoMusical c in par.Value)
                    transposto[c.Destino].Add(new ConexaoMusical(par.Key, c.Peso, c.Tipo, c.Contexto));

            bool alcancaTransposto = BfsDirecionado(inicio, transposto).Count == n;

            return (alcancaOriginal && alcancaTransposto)
                ? ("Fortemente Conexo", 1, "Qualquer música consegue chegar em qualquer outra respeitando o sentido das setas.")
                : ("Fracamente Conexo", 1, "Todas as músicas formam 1 rede única, mas algumas setas são só de ida (sem caminho de volta).");
        }

        private static HashSet<string> BfsDirecionado(string inicio, Dictionary<string, List<ConexaoMusical>> adj)
        {
            var visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { inicio };
            var fila = new Queue<string>();
            fila.Enqueue(inicio);

            while (fila.Count > 0)
            {
                string atual = fila.Dequeue();
                foreach (ConexaoMusical c in adj[atual])
                    if (visitados.Add(c.Destino))
                        fila.Enqueue(c.Destino);
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
                    if (!visitados.Contains(v) && DfsCicloNaoDirecionado(v, null, visitados))
                        return true;
                return false;
            }

            var estado = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string v in _faixas.Keys)
                if (estado.GetValueOrDefault(v, 0) == 0 && DfsCicloDirecionado(v, estado))
                    return true;
            return false;
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
                int st = estado.GetValueOrDefault(c.Destino, 0);
                if (st == 1)
                    return true;
                if (st == 0 && DfsCicloDirecionado(c.Destino, estado))
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
            string explicacaoEstrutura;

            if (lacos && paralelas)
            {
                tipoEstrutural = "Pseudografo + Multigrafo";
                tipoCurto = "Multi + Pseudografo";
                explicacaoEstrutura = "Possui auto-laço (música em Modo Repeat apontando para si mesma) e também arestas paralelas (mais de uma ligação entre o mesmo par).";
            }
            else if (lacos)
            {
                tipoEstrutural = "Pseudografo";
                tipoCurto = "Pseudografo";
                explicacaoEstrutura = "Possui pelo menos um auto-laço (Modo Repeat ativo em alguma música).";
            }
            else if (paralelas)
            {
                tipoEstrutural = "Multigrafo";
                tipoCurto = "Multigrafo";
                explicacaoEstrutura = "Possui arestas paralelas (duas músicas conectadas por mais de um motivo ao mesmo tempo).";
            }
            else
            {
                tipoEstrutural = "Grafo Simples";
                tipoCurto = "Grafo Simples";
                explicacaoEstrutura = "Não possui auto-laços (Repeat) nem arestas paralelas — no máximo 1 ligação entre cada par.";
            }

            var (conectividade, componentes, explicacaoConect) = VerificarConectividade();
            int n = _faixas.Count;
            int e = ContarArestas();

            double somaGraus = EhDirecionado
                ? _faixas.Keys.Sum(v => ObterGrauEntrada(v) + ObterGrauSaida(v))
                : _faixas.Keys.Sum(ObterGrauNaoDirecionado);

            double grauMedio = n == 0 ? 0 : somaGraus / n;
            double grauPorSentido = n == 0 ? 0 : (double)e / n;

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
            bool ciclo = PossuiCiclo();

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
                ExplicacaoEstrutura = explicacaoEstrutura,
                Conectividade = conectividade,
                ComponentesConexas = componentes,
                ExplicacaoConectividade = explicacaoConect,
                PossuiCiclo = ciclo,
                CiclicidadeTexto = ciclo
                    ? "Cíclico (existe caminho que fecha um loop)"
                    : (EhDirecionado ? "Acíclico — DAG (fluxo sem volta)" : "Acíclico (sem loops)"),
                EhCompleto = EhCompleto(),
                GrauMedio = grauMedio,
                GrauMedioDirecionadoPorSentido = grauPorSentido,
                DensidadePercentual = densidade,
                ContagemPorTipo = ContarPorTipoRelacionamento()
            };
        }

        public (List<ConexaoMusical> Diretas, List<(string Titulo, int Pontuacao)> Descobertas, bool PossuiModoRepeat)
            RecomendarAPartirDe(string titulo)
        {
            string? canonico = ObterTituloCanonico(titulo);
            if (canonico is null)
                return (new List<ConexaoMusical>(), new List<(string, int)>(), false);

            bool possuiRepeat = PossuiModoRepeat(canonico);

            List<ConexaoMusical> diretas = _listaAdjacencia[canonico]
                .Where(c => !c.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.Peso)
                .ThenBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var vizinhosDiretos = diretas.Select(c => c.Destino).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var pontuacaoSegundoGrau = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string vizinho in vizinhosDiretos)
            {
                foreach (ConexaoMusical seg in _listaAdjacencia[vizinho])
                {
                    if (seg.Destino.Equals(canonico, StringComparison.OrdinalIgnoreCase) || vizinhosDiretos.Contains(seg.Destino))
                        continue;

                    int inc = EhPonderado ? seg.Peso : 1;
                    pontuacaoSegundoGrau[seg.Destino] = pontuacaoSegundoGrau.GetValueOrDefault(seg.Destino, 0) + inc;
                }
            }

            var descobertas = pontuacaoSegundoGrau
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(p => (p.Key, p.Value))
                .ToList();

            return (diretas, descobertas, possuiRepeat);
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
                ConexaoMusical? melhor = _listaAdjacencia[atual]
                    .Where(c => !visitadas.Contains(c.Destino))
                    .OrderByDescending(c => c.Peso)
                    .ThenBy(c => c.Destino, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (melhor is null)
                    break;

                playlist.Add((atual, melhor.Destino, melhor));
                visitadas.Add(melhor.Destino);
                atual = melhor.Destino;
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

        public (List<string> Caminho, List<(string Origem, string Destino, string Contexto, int Peso)> Passos, double AfinidadeMedia)?
            GerarFilaTransicaoDijkstra(string origem, string destino)
        {
            string? orig = ObterTituloCanonico(origem);
            string? dest = ObterTituloCanonico(destino);
            if (orig is null || dest is null)
                return null;
            if (orig.Equals(dest, StringComparison.OrdinalIgnoreCase))
                return (new List<string> { orig }, new List<(string, string, string, int)>(), 100.0);

            var distancia = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { [orig] = 0 };
            var anterior = new Dictionary<string, (string? Anterior, string Contexto, int Peso)>(StringComparer.OrdinalIgnoreCase);
            var pq = new PriorityQueue<string, int>();
            pq.Enqueue(orig, 0);

            while (pq.TryDequeue(out string? atual, out int custoAtual))
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
                    int novoCusto = custoAtual + (101 - pesoValido);

                    if (novoCusto < distancia.GetValueOrDefault(c.Destino, int.MaxValue))
                    {
                        distancia[c.Destino] = novoCusto;
                        anterior[c.Destino] = (atual, c.Contexto, pesoValido);
                        pq.Enqueue(c.Destino, novoCusto);
                    }
                }
            }

            if (!anterior.ContainsKey(dest))
                return null;

            var caminho = new List<string> { dest };
            var passos = new List<(string Origem, string Destino, string Contexto, int Peso)>();
            string cursor = dest;

            while (anterior.TryGetValue(cursor, out var p) && p.Anterior is not null)
            {
                passos.Add((p.Anterior, cursor, p.Contexto, p.Peso));
                caminho.Add(p.Anterior);
                cursor = p.Anterior;
            }

            caminho.Reverse();
            passos.Reverse();
            double media = passos.Count == 0 ? 100.0 : passos.Average(x => x.Peso);
            return (caminho, passos, media);
        }

        public object ExportarEstadoDto(int presetId)
        {
            IReadOnlyList<FaixaMusical> faixas = ObterFaixasOrdenadas();
            DiagnosticoGrafo d = GerarDiagnostico();
            Dictionary<string, int> componentes = MapearComponentesSubjacentes();

            var verticesDto = faixas.Select((f, idx) =>
            {
                int ent = EhDirecionado ? ObterGrauEntrada(f.Titulo) : 0;
                int sai = EhDirecionado ? ObterGrauSaida(f.Titulo) : 0;
                int grau = EhDirecionado ? ent + sai : ObterGrauNaoDirecionado(f.Titulo);
                return new
                {
                    codigo = $"M{idx + 1}",
                    titulo = f.Titulo,
                    artista = f.Artista,
                    genero = f.Genero,
                    album = f.Album,
                    ano = f.Ano,
                    bpm = f.Bpm,
                    grauEntrada = ent,
                    grauSaida = sai,
                    grauTotal = grau,
                    possuiRepeat = PossuiModoRepeat(f.Titulo),
                    componente = componentes.GetValueOrDefault(f.Titulo, 1)
                };
            }).ToList();

            var arestasDto = new List<object>();
            foreach (FaixaMusical f in faixas)
            {
                var gruposPorDestino = _listaAdjacencia[f.Titulo]
                    .GroupBy(c => c.Destino, StringComparer.OrdinalIgnoreCase);

                foreach (var grupo in gruposPorDestino)
                {
                    string dest = grupo.Key;
                    if (!EhDirecionado && string.Compare(f.Titulo, dest, StringComparison.OrdinalIgnoreCase) > 0)
                        continue;

                    List<ConexaoMusical> listaPar = grupo.ToList();
                    bool existeReversoDirecionado = EhDirecionado &&
                        !f.Titulo.Equals(dest, StringComparison.OrdinalIgnoreCase) &&
                        _listaAdjacencia[dest].Any(r => r.Destino.Equals(f.Titulo, StringComparison.OrdinalIgnoreCase));

                    for (int idx = 0; idx < listaPar.Count; idx++)
                    {
                        ConexaoMusical c = listaPar[idx];
                        arestasDto.Add(new
                        {
                            origem = f.Titulo,
                            destino = c.Destino,
                            peso = c.Peso,
                            tipo = (int)c.Tipo,
                            contexto = c.Contexto,
                            ehLaco = f.Titulo.Equals(c.Destino, StringComparison.OrdinalIgnoreCase),
                            indiceEntrePar = idx,
                            totalEntrePar = listaPar.Count,
                            possuiArcoReverso = existeReversoDirecionado
                        });
                    }
                }
            }

            int n = faixas.Count;
            string[] codigos = Enumerable.Range(1, n).Select(i => $"M{i}").ToArray();
            var matrizLinhas = new List<object>();
            for (int i = 0; i < n; i++)
            {
                var celulas = new List<string>();
                for (int j = 0; j < n; j++)
                {
                    var enc = _listaAdjacencia[faixas[i].Titulo]
                        .Where(c => c.Destino.Equals(faixas[j].Titulo, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (enc.Count == 0)
                        celulas.Add(EhPonderado ? "-" : "0");
                    else if (!EhPonderado)
                        celulas.Add(enc.Count.ToString());
                    else
                        celulas.Add(enc.Count == 1 ? enc[0].Peso.ToString() : $"{enc.Max(c => c.Peso)}*");
                }
                matrizLinhas.Add(new { codigo = codigos[i], titulo = faixas[i].Titulo, celulas });
            }

            var listaAdjDto = faixas.Select((f, idx) => new
            {
                codigo = $"M{idx + 1}",
                titulo = f.Titulo,
                conexoes = _listaAdjacencia[f.Titulo].Select(c => new
                {
                    destino = c.Destino,
                    peso = c.Peso,
                    contexto = c.Contexto,
                    ehLaco = f.Titulo.Equals(c.Destino, StringComparison.OrdinalIgnoreCase)
                }).ToList()
            }).ToList();

            var relacionamentosCatalogo = Enum.GetValues<TipoRelacionamento>()
                .Select(t => new { id = (int)t, nome = RelacionamentoHelper.ObterNome(t) })
                .ToList();

            return new
            {
                presetId,
                nomeCenario = NomeCenario,
                ehDirecionado = EhDirecionado,
                ehPonderado = EhPonderado,
                vertices = verticesDto,
                arestas = arestasDto,
                listaAdjacencia = listaAdjDto,
                matrizAdjacencia = new { codigos, linhas = matrizLinhas },
                relacionamentosDisponiveis = relacionamentosCatalogo,
                diagnostico = new
                {
                    d.TotalVertices,
                    d.TotalArestas,
                    d.EhDirecionado,
                    d.EhPonderado,
                    d.PossuiLacos,
                    d.PossuiArestasParalelas,
                    d.TipoEstrutural,
                    d.TipoEstruturalCurto,
                    d.ExplicacaoEstrutura,
                    d.Conectividade,
                    d.ComponentesConexas,
                    d.ExplicacaoConectividade,
                    d.PossuiCiclo,
                    d.CiclicidadeTexto,
                    d.EhCompleto,
                    grauMedio = Math.Round(d.GrauMedio, 2),
                    grauPorSentido = Math.Round(d.GrauMedioDirecionadoPorSentido, 2),
                    densidadePercentual = Math.Round(d.DensidadePercentual, 1),
                    contagemPorTipo = d.ContagemPorTipo
                        .OrderByDescending(p => p.Value)
                        .Select(p => new { nome = RelacionamentoHelper.ObterNome(p.Key), qtd = p.Value })
                        .ToList()
                }
            };
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
            sb.AppendLine("=== DIAGNÓSTICO DE TEORIA DOS GRAFOS ===");
            sb.AppendLine($"  • Direção       : {(d.EhDirecionado ? "Direcionado (Dígrafo)" : "Não-Direcionado")}");
            sb.AppendLine($"  • Ponderação    : {(d.EhPonderado ? "Ponderado (%)" : "Não-Ponderado")}");
            sb.AppendLine($"  • Estrutura     : {d.TipoEstrutural} — {d.ExplicacaoEstrutura}");
            sb.AppendLine($"  • Conectividade : {d.Conectividade} ({d.ComponentesConexas} comp.) — {d.ExplicacaoConectividade}");
            sb.AppendLine($"  • Ciclicidade   : {d.CiclicidadeTexto}");
            sb.AppendLine($"  • Completo (Kn) : {(d.EhCompleto ? $"Sim (K{d.TotalVertices})" : "Não")}");
            sb.AppendLine($"  • Grau Médio    : {d.GrauMedio:F2}");
            sb.AppendLine($"  • Densidade     : {d.DensidadePercentual:F1}%");
            sb.AppendLine();

            sb.AppendLine("=== LISTA DE ADJACÊNCIA ===");
            string seta = EhDirecionado ? "→" : "↔";
            foreach (FaixaMusical f in faixas)
            {
                var conns = _listaAdjacencia[f.Titulo];
                string destTxt = conns.Count == 0
                    ? "(isolada)"
                    : string.Join(", ", conns.Select(c => EhPonderado ? $"{c.Destino} [{c.Contexto}: {c.Peso}%]" : $"{c.Destino} ({c.Contexto})"));
                sb.AppendLine($"  {f.Titulo} {seta} {destTxt}");
            }

            return sb.ToString();
        }
    }

    public static class PresetsCatalogo
    {
        public static GrafoMusical CriarPreset(int id) => id switch
        {
            2 => CriarPopSimples(),
            3 => CriarIlhasDesconexas(),
            4 => CriarAlbumCompletoK6(),
            5 => CriarFestivalDag(),
            6 => CriarTreinoRepeat(),
            _ => CriarStreamingCompleto()
        };

        private static GrafoMusical CriarStreamingCompleto()
        {
            var g = new GrafoMusical(
                "1. Streaming Mix (Dígrafo Ponderado | Multigrafo + Pseudografo)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Blinding Lights", "The Weeknd", "Synthpop", "After Hours", 2020, 171);
            g.AdicionarFaixa("Save Your Tears", "The Weeknd", "Synthpop", "After Hours", 2020, 118);
            g.AdicionarFaixa("Starboy", "The Weeknd", "Pop", "Starboy", 2016, 186);
            g.AdicionarFaixa("Take On Me", "a-ha", "Synthpop", "Hunting High", 1985, 169);
            g.AdicionarFaixa("Levitating", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 103);
            g.AdicionarFaixa("Get Lucky", "Daft Punk", "Eletrônica", "RAM", 2013, 116);
            g.AdicionarFaixa("Instant Crush", "Daft Punk", "Eletrônica", "RAM", 2013, 110);
            g.AdicionarFaixa("Midnight City", "M83", "Eletrônica", "Hurry Up", 2011, 105);
            g.AdicionarFaixa("Stronger", "Kanye West", "Rap", "Graduation", 2007, 104);
            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Numb", "Linkin Park", "Rock", "Meteora", 2003, 110);
            g.AdicionarFaixa("In The End", "Linkin Park", "Rock", "Hybrid Theory", 2000, 105);

            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 96, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 90, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Blinding Lights", "Take On Me", 94, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Blinding Lights", "Starboy", 89, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Blinding Lights", "Blinding Lights", 100, TipoRelacionamento.ModoRepeat);
            g.AdicionarConexao("Save Your Tears", "Levitating", 86, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Levitating", "Get Lucky", 91, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Starboy", "Instant Crush", 92, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Starboy", "Blinding Lights", 88, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("Get Lucky", "Instant Crush", 93, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Get Lucky", "Stronger", 87, TipoRelacionamento.SampleOuCover);
            g.AdicionarConexao("Instant Crush", "Midnight City", 84, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Take On Me", "Midnight City", 86, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Midnight City", "Blinding Lights", 82, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Stronger", "Lose Yourself", 91, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Numb", 85, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Blinding Lights", 80, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Numb", "In The End", 98, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Numb", "In The End", 92, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("In The End", "Numb", 90, TipoRelacionamento.RadioLoop);
            g.AdicionarConexao("In The End", "Midnight City", 75, TipoRelacionamento.AfinidadeSonora);
            return g;
        }

        private static GrafoMusical CriarPopSimples()
        {
            var g = new GrafoMusical(
                "2. Pop & Synth (Não-Direcionado | Sem Peso | Grafo Simples e Conexo)",
                ehDirecionado: false,
                ehPonderado: false);

            g.AdicionarFaixa("Blinding Lights", "The Weeknd", "Synthpop", "After Hours", 2020, 171);
            g.AdicionarFaixa("Save Your Tears", "The Weeknd", "Synthpop", "After Hours", 2020, 118);
            g.AdicionarFaixa("Take On Me", "a-ha", "Synthpop", "Hunting High", 1985, 169);
            g.AdicionarFaixa("As It Was", "Harry Styles", "Synthpop", "Harry's House", 2022, 174);
            g.AdicionarFaixa("Levitating", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 103);
            g.AdicionarFaixa("Physical", "Dua Lipa", "Pop", "Future Nostalgia", 2020, 147);
            g.AdicionarFaixa("Get Lucky", "Daft Punk", "Eletrônica", "RAM", 2013, 116);
            g.AdicionarFaixa("Billie Jean", "Michael Jackson", "Pop", "Thriller", 1982, 117);

            g.AdicionarConexao("Blinding Lights", "Save Your Tears", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Blinding Lights", "Take On Me", 1, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Blinding Lights", "As It Was", 1, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("As It Was", "Physical", 1, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Physical", "Levitating", 1, TipoRelacionamento.MesmoAlbum);
            g.AdicionarConexao("Levitating", "Get Lucky", 1, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Get Lucky", "Billie Jean", 1, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Billie Jean", "Save Your Tears", 1, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Take On Me", "Billie Jean", 1, TipoRelacionamento.AfinidadeSonora);
            return g;
        }

        private static GrafoMusical CriarIlhasDesconexas()
        {
            var g = new GrafoMusical(
                "3. Ilhas Musicais (Dígrafo Desconexo — 3 Grupos Isolados: Rock, Rap e Clássica)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Numb", "Linkin Park", "Rock", "Meteora", 2003, 110);
            g.AdicionarFaixa("In The End", "Linkin Park", "Rock", "Hybrid Theory", 2000, 105);
            g.AdicionarFaixa("Bring Me To Life", "Evanescence", "Rock", "Fallen", 2003, 95);
            g.AdicionarFaixa("Everlong", "Foo Fighters", "Rock", "The Colour", 1997, 158);

            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Till I Collapse", "Eminem", "Rap", "Eminem Show", 2002, 171);
            g.AdicionarFaixa("Stronger", "Kanye West", "Rap", "Graduation", 2007, 104);

            g.AdicionarFaixa("Clair de Lune", "Debussy", "Clássica", "Suite", 1905, 66);
            g.AdicionarFaixa("Nocturne Op.9", "Chopin", "Clássica", "Nocturnes", 1832, 70);

            g.AdicionarConexao("Numb", "In The End", 96, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("In The End", "Bring Me To Life", 88, TipoRelacionamento.AfinidadeSonora);
            g.AdicionarConexao("Bring Me To Life", "Everlong", 84, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Everlong", "Numb", 86, TipoRelacionamento.RadioLoop);

            g.AdicionarConexao("Lose Yourself", "Till I Collapse", 97, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Till I Collapse", "Stronger", 91, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Stronger", "Lose Yourself", 89, TipoRelacionamento.MesmoGenero);

            g.AdicionarConexao("Clair de Lune", "Nocturne Op.9", 95, TipoRelacionamento.PlaylistFoco);
            return g;
        }

        private static GrafoMusical CriarAlbumCompletoK6()
        {
            var g = new GrafoMusical(
                "4. Álbum Conceitual K6 (Grafo Completo — Todas conectadas com todas, 100% densidade)",
                ehDirecionado: false,
                ehPonderado: false);

            var faixas = new (string Titulo, int Bpm)[]
            {
                ("Overture 1984", 112),
                ("Neon Highway", 118),
                ("Cyber Pulse", 122),
                ("Digital Sun", 116),
                ("Starlight", 114),
                ("Night Finale", 110)
            };

            foreach (var (titulo, bpm) in faixas)
                g.AdicionarFaixa(titulo, "Synth Collective", "Synthwave", "Neon Horizons", 2025, bpm);

            for (int i = 0; i < faixas.Length; i++)
                for (int j = i + 1; j < faixas.Length; j++)
                    g.AdicionarConexao(faixas[i].Titulo, faixas[j].Titulo, 1, TipoRelacionamento.MesmoAlbum);

            return g;
        }

        private static GrafoMusical CriarFestivalDag()
        {
            var g = new GrafoMusical(
                "5. Setlist de Festival — DAG (Dígrafo Acíclico: fluxo só avança sem loops)",
                ehDirecionado: true,
                ehPonderado: true);

            g.AdicionarFaixa("Intro Acústica", "Banda Aurora", "Acústico", "Ao Vivo", 2026, 85);
            g.AdicionarFaixa("Warmup Indie", "Banda Aurora", "Indie", "Ao Vivo", 2026, 102);
            g.AdicionarFaixa("Batida Lo-Fi", "DJ Pulse", "Lo-Fi", "Ao Vivo", 2026, 90);
            g.AdicionarFaixa("Vibe Pop", "Luna Star", "Pop", "Ao Vivo", 2026, 118);
            g.AdicionarFaixa("Drop Eletrônico", "DJ Pulse", "Eletrônica", "Ao Vivo", 2026, 128);
            g.AdicionarFaixa("Clímax Rock", "Banda Aurora", "Rock", "Ao Vivo", 2026, 145);
            g.AdicionarFaixa("Hino da Arena", "All Stars", "Rock", "Ao Vivo", 2026, 140);
            g.AdicionarFaixa("Bis Épico", "All Stars", "Rock", "Ao Vivo", 2026, 132);

            g.AdicionarConexao("Intro Acústica", "Warmup Indie", 90, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Intro Acústica", "Batida Lo-Fi", 86, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Warmup Indie", "Vibe Pop", 88, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Batida Lo-Fi", "Vibe Pop", 82, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Batida Lo-Fi", "Drop Eletrônico", 94, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Vibe Pop", "Drop Eletrônico", 93, TipoRelacionamento.PlaylistFesta);
            g.AdicionarConexao("Vibe Pop", "Clímax Rock", 72, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Drop Eletrônico", "Clímax Rock", 92, TipoRelacionamento.TransicaoFila);
            g.AdicionarConexao("Clímax Rock", "Hino da Arena", 97, TipoRelacionamento.ColaboracaoFeat);
            g.AdicionarConexao("Hino da Arena", "Bis Épico", 95, TipoRelacionamento.TransicaoFila);
            return g;
        }

        private static GrafoMusical CriarTreinoRepeat()
        {
            var g = new GrafoMusical(
                "6. Mix Treino + Repeat (Não-Direcionado Ponderado | Multigrafo + Pseudografo)",
                ehDirecionado: false,
                ehPonderado: true);

            g.AdicionarFaixa("Eye of the Tiger", "Survivor", "Rock", "Eye of the Tiger", 1982, 109);
            g.AdicionarFaixa("Till I Collapse", "Eminem", "Rap", "Eminem Show", 2002, 171);
            g.AdicionarFaixa("Lose Yourself", "Eminem", "Rap", "8 Mile", 2002, 171);
            g.AdicionarFaixa("Stronger", "Kanye West", "Rap", "Graduation", 2007, 104);
            g.AdicionarFaixa("Power", "Kanye West", "Rap", "MBDTF", 2010, 154);
            g.AdicionarFaixa("Believer", "Imagine Dragons", "Rock", "Evolve", 2017, 125);
            g.AdicionarFaixa("Enter Sandman", "Metallica", "Metal", "Black Album", 1991, 123);

            g.AdicionarConexao("Eye of the Tiger", "Till I Collapse", 93, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 98, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Till I Collapse", "Lose Yourself", 95, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Lose Yourself", "Lose Yourself", 100, TipoRelacionamento.ModoRepeat);
            g.AdicionarConexao("Lose Yourself", "Stronger", 89, TipoRelacionamento.MesmoGenero);
            g.AdicionarConexao("Stronger", "Power", 95, TipoRelacionamento.MesmoArtista);
            g.AdicionarConexao("Power", "Believer", 88, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Believer", "Enter Sandman", 86, TipoRelacionamento.BpmCompativel);
            g.AdicionarConexao("Enter Sandman", "Eye of the Tiger", 84, TipoRelacionamento.PlaylistTreino);
            g.AdicionarConexao("Enter Sandman", "Enter Sandman", 100, TipoRelacionamento.ModoRepeat);
            return g;
        }
    }
}
