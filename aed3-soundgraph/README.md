# SoundGraph Pro — Grafo Interativo de Músicas e Recomendações em C# (AED3)

Trabalho da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (ASP.NET Core Minimal API)** com **interface web visual interativa**.

O sistema modela um motor de recomendação musical (estilo Spotify) como um **Grafo** processado integralmente no backend C# e desenhado em tempo real no navegador, permitindo **enxergar e interagir com o mouse** sobre todos os tipos, direções, pesos e algoritmos de grafos estudados na disciplina.

---

## 👩‍🏫 Passo a Passo para Rodar e Testar em Outra Máquina (Guia de Avaliação)

Não é necessário instalar banco de dados, Node.js nem pacotes externos — o projeto é 100% autocontido em C# e funciona inclusive sem internet após baixado.

### 1. Pré-requisito
- Ter o **[.NET SDK](https://dotnet.microsoft.com/download)** instalado na máquina (compatível com **.NET 8, .NET 9 ou .NET 10**).
  - Para verificar se já está instalado, abra o terminal (CMD ou PowerShell) e digite:
    ```bash
    dotnet --version
    ```

### 2. Baixar os arquivos do GitHub
Escolha uma das duas formas:
- **Sem Git (Download ZIP):** Na página principal do repositório no GitHub, clique no botão verde **`<> Code`** → **`Download ZIP`**, extraia o arquivo `.zip` em qualquer pasta do computador e abra a subpasta **`aed3-soundgraph`**.
- **Com Git (Terminal):**
  ```bash
  git clone https://github.com/vitorr369/trabalhos-faculdade.git
  cd trabalhos-faculdade/aed3-soundgraph
  ```

### 3. Executar o Projeto (3 opções)

- **Opção A — Pelo atalho no Windows (Mais rápido):**
  1. Entre na pasta `aed3-soundgraph`.
  2. Dê dois cliques no arquivo **`ExecutarSoundGraph.bat`**.
  3. Aguarde alguns segundos na primeira compilação: o servidor iniciará em `http://localhost:5180` e abrirá automaticamente no navegador padrão.

- **Opção B — Pelo Terminal (Windows, Linux ou macOS):**
  1. Abra o terminal dentro da pasta `aed3-soundgraph` e execute:
     ```bash
     dotnet run
     ```
  2. Caso o navegador não abra sozinho, acesse **`http://localhost:5180`**.
  3. Para encerrar o servidor após testar, basta pressionar `Ctrl + C` no terminal (ou fechar a janela).

- **Opção C — Pelo Visual Studio ou VS Code:**
  - **Visual Studio:** Abra o arquivo `SoundGraph.csproj` e pressione `Ctrl + F5` (Iniciar sem Depurar).
  - **VS Code:** Abra a pasta `aed3-soundgraph`, abra o terminal integrado (`Ctrl + '`) e rode `dotnet run`.

### 4. Roteiro Rápido de Teste na Interface (1 minuto)
Com a página aberta em `http://localhost:5180`:
1. **Alternar Tipos de Grafos (barra superior):** Clique nos botões **`1` a `6`** no topo para ver a estrutura mudar ao vivo entre *Multigrafo + Pseudografo*, *Grafo Simples Não-Direcionado*, *Grafo Desconexo (3 ilhas)*, *Grafo Completo $K_6$* e *DAG Acíclico*.
2. **Explorar Vértices e Arestas (no mapa central):** Clique em qualquer bolinha de música para ver suas conexões diretas (**1º grau em verde**) e descobertas (**2º grau em azul**), ou arraste os nós com o mouse.
3. **Testar Auto-Laço / Pseudografo (aba `🎧 Player`):** Clique em **`🔁 Ligar/Desligar Repeat`** para criar ou remover um auto-laço na música atual e observar o arco roxo no mapa e a mudança automática da classificação estrutural no topo.
4. **Comparar BFS vs. Dijkstra (aba `🧭 Rotas`):** Selecione o cenário **`5. Festival (DAG)`**, abra a aba **`🧭 Rotas`** e clique em **`🔍 Traçar e Comparar no Mapa`** (entre *Vibe Pop* e *Clímax Rock*) para comparar o menor caminho em saltos (**BFS**, 1 salto) com a rota de maior afinidade (**Dijkstra**, 2 saltos com média de `92,5%`).
5. **Conferir Estruturas Formais (aba `📊 Matriz`):** Veja a **Matriz de Adjacência ($N \times N$)** colorida, a **Lista de Adjacência**, o diagnóstico explicado e o botão **`⬇ Baixar .TXT`**.

---

## 🧠 O Conceito de forma simples (Como funciona?)

Na plataforma, o catálogo de músicas vira uma rede visual $G = (V, E)$:

- **Cada bolinha no mapa é um Vértice (`FaixaMusical`):** Representa uma música com `Título`, `Artista`, `Gênero`, `Álbum`, `Ano` e `BPM`.
- **Cada linha/seta é uma Aresta (`ConexaoMusical`):** Representa **por que** duas músicas combinam (entre **14 tipos de relacionamento**, como *Mesmo Álbum*, *Mesmo Artista*, *BPM Compatível*, *Playlist Festa*, *Playlist Treino*, *Colaboração/Feat* ou *Modo Repeat*) e **o quanto** elas combinam (**peso de `1%` a `100%`**).

### O que é demonstrado visualmente

- **Grafo Direcionado (`→`) vs. Não-Direcionado (`↔`):** Quando é um **Dígrafo**, o mapa desenha setas indicando o sentido da fila; quando é **Não-Direcionado**, desenha linhas simétricas de mão dupla.
- **Grafo Simples, Multigrafo e Pseudografo:**
  - **Simples:** No máximo 1 linha entre duas músicas, sem laços.
  - **Multigrafo:** Duas músicas ligadas por mais de um motivo ao mesmo tempo aparecem com **curvas paralelas separadas**.
  - **Pseudografo:** Ao ativar o **Modo Repeat** numa música, surge um **arco roxo apontando para a própria bolinha** (**auto-laço**).
- **Conectividade e Topologias Especiais:**
  - **Conexo / Fortemente / Fracamente Conexo vs. Desconexo:** No cenário *3. Desconexo*, o layout separa visualmente as **3 ilhas isoladas** (Rock, Rap e Clássica).
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
   - Busca em Profundidade com rastreamento de pai em grafos não-direcionados e **DFS com 3 estados** (`0` não visitado, `1` em recursão, `2` finalizado) em dígrafos para identificar loops ou confirmar um **DAG**.
4. **Comparador Visual de Menor Caminho — BFS vs. Dijkstra (`GerarFilaTransicao` e `GerarFilaTransicaoDijkstra`):**
   - **BFS (`Queue<string>`):** Encontra o caminho com **menos pulos (transições)** entre duas músicas e destaca a rota em **azul** no mapa.
   - **Dijkstra (`PriorityQueue<string, int>`):** Em grafos ponderados, minimiza a dissonância (`101 - Peso`) para achar a rota de **maior afinidade percentual média**, destacando a rota em **dourado** no mapa.
5. **Motor de Recomendação de 1º e 2º Grau (`RecomendarAPartirDe`):**
   - Destaca conexões diretas (1º grau) e descobertas alcançáveis em 2 passos (2º grau).
6. **Playlist Automática Contínua (`GerarPlaylistAutomatica`):**
   - Algoritmo guloso (*Greedy*) que percorre sempre a aresta de maior afinidade ainda não visitada e numera `1 → 2 → 3...` nos nós do grafo.

---

## 📂 Arquivos do Projeto

- `Program.cs` — Backend completo em C#: classes de domínio, estrutura `GrafoMusical`, algoritmos (BFS, DFS, Dijkstra, Recomendações, Playlist Gulosa, Matriz/Lista e Diagnóstico), 6 presets prontos e rotas da Minimal API.
- `wwwroot/index.html` — Interface visual interativa com renderização em SVG, player lateral, editor de grafo e matriz colorida.
- `SoundGraph.csproj` — Arquivo de projeto C# Web compatível com `.NET 8`, `.NET 9` e `.NET 10`.
- `ExecutarSoundGraph.bat` — Inicializador rápido para Windows.
- `README.md` — Documentação completa e guia de execução para avaliação.
