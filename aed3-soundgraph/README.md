# SoundGraph Pro — Grafo Interativo de Músicas e Recomendações em C# (AED3)

Trabalho da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (.NET 10 — ASP.NET Core Minimal API)** com **interface web visual interativa**.

O sistema modela um motor de recomendação musical (estilo Spotify) como um **Grafo** processado integralmente no backend C# e desenhado em tempo real no navegador, permitindo **enxergar e interagir com o mouse** sobre todos os tipos, direções, pesos e algoritmos de grafos estudados na disciplina.

---

## 🧠 O Conceito de forma simples (Como funciona?)

Na plataforma, o catálogo de músicas vira uma rede visual $G = (V, E)$:

- **Cada bolinha no mapa é um Vértice (`FaixaMusical`):** Representa uma música com `Título`, `Artista`, `Gênero`, `Álbum`, `Ano` e `BPM`.
- **Cada linha/seta é uma Aresta (`ConexaoMusical`):** Representa **por que** duas músicas combinam (entre **14 tipos de relacionamento**, como *Mesmo Álbum*, *Mesmo Artista*, *BPM Compatível*, *Playlist Festa*, *Playlist Treino*, *Colaboração/Feat* ou *Modo Repeat*) e **o quanto** elas combinam (**peso de `1%` a `100%`**).

### O que você enxerga visualmente no Mapa

- **Grafo Direcionado (`→`) vs. Não-Direcionado (`↔`):** Quando é um **Dígrafo**, o mapa desenha setas indicando o sentido da fila; quando é **Não-Direcionado**, desenha linhas simétricas de mão dupla.
- **Grafo Simples, Multigrafo e Pseudografo:**
  - **Simples:** No máximo 1 linha entre duas músicas, sem laços.
  - **Multigrafo:** Duas músicas ligadas por mais de um motivo ao mesmo tempo aparecem com **curvas paralelas separadas**.
  - **Pseudografo:** Ao clicar em **`🔁 Ligar Repeat`** numa música, surge um **arco roxo apontando para a própria bolinha** (**auto-laço**) e a classificação no topo muda na hora para `Pseudografo`.
- **Conectividade e Topologias Especiais:**
  - **Conexo / Fortemente / Fracamente Conexo vs. Desconexo:** No cenário *3. Desconexo*, o algoritmo separa visualmente as **3 ilhas isoladas** (Rock, Rap e Clássica).
  - **Grafo Completo ($K_6$):** No cenário *4. Completo K6*, forma o hexágono clássico onde todas as 6 músicas se conectam com todas (`15` arestas e `100%` de densidade).
  - **Cíclico vs. Acíclico (DAG):** No cenário *5. Festival (DAG)*, as camadas fluem da esquerda para a direita sem nenhum ciclo de retorno.

---

## ⚙️ Estruturas e Algoritmos Implementados em C# (`Program.cs`)

Todo o processamento de dados e algoritmos roda no backend em C# (`GrafoMusical`):

1. **Lista de Adjacência e Matriz de Adjacência ($N \times N$):**
   - Armazenamento principal em `Dictionary<string, List<ConexaoMusical>>` e geração dinâmica da Matriz de Adjacência destacando conexões em verde, auto-laços na diagonal em roxo e arestas paralelas com `*`.
2. **Conectividade via BFS (`VerificarConectividade`):**
   - Busca em Largura (`Queue<string>`) no grafo subjacente para contar componentes conexas e dupla BFS no grafo original $G$ e no **grafo transposto** $G^T$ para diferenciar dígrafos **Fortemente Conexos** de **Fracamente Conexos**.
3. **Detecção de Ciclos via DFS (`PossuiCiclo`):**
   - Busca em Profundidade com rastreamento de pai em grafos não-direcionados e **DFS com 3 cores/estados** (`0` não visitado, `1` em recursão, `2` finalizado) em dígrafos para identificar loops ou confirmar um **DAG**.
4. **Comparador Visual de Menor Caminho — BFS vs. Dijkstra (`GerarFilaTransicao` e `GerarFilaTransicaoDijkstra`):**
   - **BFS (`Queue<string>`):** Encontra o caminho com **menos pulos (transições)** entre duas músicas e acende a rota em **azul** no mapa.
   - **Dijkstra (`PriorityQueue<string, int>`):** Em grafos ponderados, minimiza a dissonância (`101 - Peso`) para achar a rota de **maior afinidade percentual média**, acendendo a rota em **dourado** no mapa (ex.: no cenário *5. Festival*, entre *Vibe Pop* e *Clímax Rock*, o BFS faz 1 salto de `72%`, enquanto o Dijkstra escolhe 2 saltos com média de `92,5%`!).
5. **Motor de Recomendação de 1º e 2º Grau (`RecomendarAPartirDe`):**
   - Ao clicar em qualquer música no mapa, destaca em **verde** as conexões diretas (1º grau) e em **azul** as descobertas alcançáveis em 2 passos (2º grau).
6. **Playlist Automática Contínua (`GerarPlaylistAutomatica`):**
   - Algoritmo guloso (*Greedy*) que percorre sempre a aresta de maior afinidade ainda não visitada e numera `1 → 2 → 3...` nos nós do grafo.

---

## 🚀 Como Compilar e Rodar o Código

### Pré-requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) instalado (`dotnet --version`). Não é necessário instalar Node.js nem bibliotecas externas (funciona inclusive 100% offline).

### Opção 1 — Atalho Rápido no Windows (`ExecutarSoundGraph.bat`)
1. Abra a pasta `aed3-soundgraph`.
2. Dê dois cliques em **`ExecutarSoundGraph.bat`** — o servidor C# compila, inicia em `http://localhost:5180` e abre automaticamente no seu navegador padrão.

### Opção 2 — Pelo Terminal (PowerShell, CMD, Linux ou macOS)
```bash
cd aed3-soundgraph
dotnet run
```
Depois, se o navegador não abrir sozinho, acesse **`http://localhost:5180`**. *(Para encerrar o servidor, pressione `Ctrl + C` no terminal.)*

---

## 📂 Arquivos do Projeto

- `Program.cs` — Backend completo em C#: classes de domínio, estrutura `GrafoMusical`, algoritmos (BFS, DFS, Dijkstra, Recomendações, Playlist Gulosa, Matriz/Lista e Diagnóstico), 6 presets prontos e rotas da Minimal API.
- `wwwroot/index.html` — Interface visual interativa com renderização em SVG (clique, arraste, curvas de multigrafo, arcos de auto-laço e destaque de rotas), player lateral, editor de grafo e matriz colorida.
- `SoundGraph.csproj` — Arquivo de projeto `.NET 10` (`Microsoft.NET.Sdk.Web`).
- `ExecutarSoundGraph.bat` — Inicializador rápido para Windows.
- `README.md` — Documentação do projeto e explicação dos conceitos de grafos.
