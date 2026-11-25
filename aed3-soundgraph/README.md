# SoundGraph Pro - Grafos Aplicados a Recomendação Musical (AED3)

Trabalho da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (ASP.NET Core)** com interface web para visualização do grafo.

O projeto simula um sistema de recomendação de músicas e playlists utilizando **Teoria dos Grafos**. Toda a estrutura de dados e os algoritmos rodam no backend em C#, enquanto a página no navegador desenha os vértices e arestas para facilitar a visualização dos tipos e direções de grafos.

## Modelagem em Grafos

O catálogo de músicas é representado por um grafo $G = (V, E)$:

- **Vértices (`FaixaMusical`):** Cada vértice representa uma música cadastrada (título, artista, gênero, álbum, ano e BPM).
- **Arestas (`ConexaoMusical`):** Representam a relação entre duas músicas (mesmo artista, mesmo álbum, mesmo gênero, BPM compatível, playlist, colaboração ou modo repeat) e o peso de afinidade (`1%` a `100%` quando o grafo é ponderado).

### Tipos e Direções Demonstrados

Nos cenários prontos na barra superior da aplicação (ou criando um grafo novo), o sistema trabalha com:

- **Não-Direcionado vs. Direcionado (Dígrafo):** Conexões mútuas (`A <-> B`, sem seta) ou transições de fila com sentido único (`A -> B`, calculando grau de entrada e grau de saída).
- **Ponderado vs. Não-Ponderado:** Com porcentagem de afinidade nas arestas ou peso unitário.
- **Grafo Simples:** Sem auto-laços e com no máximo uma aresta entre cada par de vértices.
- **Multigrafo:** Múltiplas arestas entre o mesmo par de músicas (desenhadas em curvas separadas).
- **Pseudografo:** Presença de auto-laço ao ativar o **Modo Repeat** em uma música (ligação do vértice para ele mesmo).
- **Conexo, Fortemente/Fracamente Conexo e Desconexo:** Incluindo cenário dividido em componentes isoladas.
- **Grafo Completo ($K_6$) e Grafo Direcionado Acíclico (DAG):** Exemplos práticos de grafo com 100% de densidade e de fluxo sem ciclos.

## Estruturas e Algoritmos em C# (`Program.cs`)

1. **Lista de Adjacência e Matriz de Adjacência:** O grafo utiliza `Dictionary<string, List<ConexaoMusical>>` como estrutura principal e gera dinamicamente a Matriz de Adjacência $N \times N$.
2. **Conectividade com BFS (`VerificarConectividade`):** Usa Busca em Largura (`Queue<string>`) para contar componentes conexas e aplica BFS no grafo original e no grafo transposto para classificar dígrafos em fortemente ou fracamente conexos.
3. **Detecção de Ciclos com DFS (`PossuiCiclo`):** Aplica Busca em Profundidade controlando o vértice pai (em grafos não-direcionados) e visitação em 3 estados (em dígrafos) para detectar loops ou confirmar um DAG.
4. **Menor Caminho - BFS vs. Dijkstra (`GerarFilaTransicao` e `GerarFilaTransicaoDijkstra`):**
   - **BFS:** Retorna a sequência com o menor número de transições entre duas músicas.
   - **Dijkstra (`PriorityQueue<string, int>`):** Em grafos ponderados, calcula a rota de maior afinidade média entre origem e destino.
5. **Recomendação (1º e 2º Grau) e Playlist Automática:** Sugere conexões diretas e alcançáveis em dois passos, além de montar playlists por estratégia gulosa e tocar prévias de áudio das faixas (com suporte a `.mp3` local e loop no modo repeat).

## Como Compilar e Executar

### Pré-requisito
- [.NET SDK](https://dotnet.microsoft.com/download) instalado (versão 8.0, 9.0 ou 10.0). Para conferir no terminal:
  ```bash
  dotnet --version
  ```

### Baixando o projeto
- **Via arquivo ZIP:** Na página principal do repositório no GitHub, clique em **`Code` -> `Download ZIP`**, extraia os arquivos no computador e abra a pasta `aed3-soundgraph`.
- **Via Git:**
  ```bash
  git clone https://github.com/vitorr369/trabalhos-faculdade.git
  cd trabalhos-faculdade/aed3-soundgraph
  ```

### Rodando a aplicação

- **No Windows (atalho `.bat`):**
  Abra a pasta `aed3-soundgraph` e dê dois cliques em `ExecutarSoundGraph.bat`. O projeto será compilado e abrirá automaticamente no navegador em `http://localhost:5180`.

- **Pelo Terminal (Windows, Linux ou macOS):**
  Dentro da pasta `aed3-soundgraph`, execute:
  ```bash
  dotnet run
  ```
  Em seguida, acesse `http://localhost:5180` no navegador (caso não abra sozinho). Para encerrar, pressione `Ctrl + C` no terminal.

- **Pelo Visual Studio:**
  Abra o arquivo `SoundGraph.csproj` e pressione `Ctrl + F5`.

## Estrutura de Arquivos

- `Program.cs` - Classes do grafo, algoritmos (BFS, DFS, Dijkstra), cenários de teste e rotas da API em C#.
- `wwwroot/index.html` - Página web com o desenho interativo do grafo, player de áudio, controles de edição e matriz de adjacência.
- `SoundGraph.csproj` - Configuração do projeto .NET.
- `ExecutarSoundGraph.bat` - Script para iniciar rapidamente no Windows.
