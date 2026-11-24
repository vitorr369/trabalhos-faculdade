# SoundGraph Pro — Motor de Recomendação de Músicas e Playlists com Grafos (AED3)

Trabalho da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (.NET 10)**.

O projeto aplica na prática os principais conceitos de **Teoria dos Grafos** — incluindo **tipos estruturais, direções, ponderação, representações computacionais e algoritmos de busca e caminho mínimo** — dentro de uma interface interativa de console guiada pelas **setas do teclado (`↑`/`↓`/`←`/`→`)** e com um modo **Player Interativo** para navegar ao vivo pelas músicas do grafo.

---

## 🧠 Modelagem e Conceitos de Teoria dos Grafos

Um catálogo musical é modelado como um grafo $G = (V, E)$, onde:

- **Vértices (`FaixaMusical`):** Cada vértice representa uma música cadastrada no catálogo, armazenando `Titulo`, `Artista`, `Genero`, `Album`, `Ano` e `Bpm` em um dicionário `Dictionary<string, FaixaMusical>` (*case-insensitive*, busca $O(1)$).
- **Arestas / Arcos (`ConexaoMusical`) e 14 Tipos de Relacionamento (`TipoRelacionamento`):** Cada conexão liga uma faixa de origem a uma faixa de destino, carregando um **peso de afinidade** (quando ponderado) e um rótulo semântico:
  - `AfinidadeSonora`, `MesmoArtista`, `MesmoAlbum`, `MesmoGenero`, `ColaboracaoFeat`, `TransicaoFila`, `PlaylistTreino`, `PlaylistFoco`, `PlaylistFesta`, `SampleOuCover`, `BpmCompativel`, `RadioLoop`, `ModoRepeat` (auto-laço) e `Personalizado`.

### Representações Computacionais Implementadas

1. **Lista de Adjacência (`_listaAdjacencia`):**
   - Implementada como `Dictionary<string, List<ConexaoMusical>>`.
   - Exibida de forma paginada em formato de árvore (`├─ → Destino`), mantendo a leitura limpa no terminal.
2. **Matriz de Adjacência (`ObterDadosMatriz`):**
   - Construída dinamicamente como matriz $N \times N$ (`M1` até `Mn`) a partir da lista de adjacência.
   - Evidencia visualmente a **simetria** ($M[i,j] = M[j,i]$) em grafos não-direcionados, a **assimetria** em dígrafos, a **diagonal principal** destacada para auto-laços (*Modo Repeat*) e o indicador `N*` quando há **arestas paralelas** em grafos ponderados.

### Classificações de Grafos Demonstradas no Sistema

- **Quanto à Direção:**
  - **Não-Direcionado (`EhDirecionado = false`):** Afinidade mútua/bidirecional ($u \leftrightarrow v$). Cada conexão é inserida nos dois sentidos na lista de adjacência (exceto auto-laços), o número de arestas é $|E| = \frac{\text{não-laços}}{2} + \text{laços}$, e cada auto-laço soma $+2$ ao grau do vértice.
  - **Direcionado / Dígrafo (`EhDirecionado = true`):** Transições de fila ou setlist com sentido único ($u \rightarrow v$), calculando separadamente **Grau de Entrada** ($d^-(v)$) e **Grau de Saída** ($d^+(v)$), além da média por sentido ($\overline{d^-} = \overline{d^+} = \frac{|E|}{|V|}$).
- **Quanto à Ponderação:**
  - **Ponderado (`EhPonderado = true`):** Arestas possuem afinidade entre `1%` e `100%`.
  - **Não-Ponderado (`EhPonderado = false`):** Peso unitário (`1`).
- **Quanto à Estrutura (`TipoEstrutural`):**
  - **Grafo Simples:** Sem auto-laços e sem arestas paralelas.
  - **Multigrafo:** Possui múltiplas arestas entre o mesmo par de faixas (ex.: duas músicas ligadas por `Mesmo Álbum` e também por `Playlist Festa`).
  - **Pseudografo:** Possui pelo menos um **auto-laço** (a faixa conectada a ela mesma no *Modo Repeat*).
- **Propriedades e Métricas Topológicas:**
  - **Grafo Completo ($K_n$):** Verifica se não há laços nem arestas paralelas e se todo vértice está conectado a todos os outros $n - 1$ vértices.
  - **Ciclicidade e DAG:** Identifica grafos cíclicos vs. **DAG** (*Directed Acyclic Graph* — fluxo progressivo sem retorno).
  - **Grau Médio e Densidade Relativa (%):** Calcula $\frac{\sum d(v)}{|V|}$ e a porcentagem de pares distintos sem auto-laço presentes em relação ao máximo possível de um grafo simples ($n(n-1)$ para dígrafos e $\frac{n(n-1)}{2}$ para não-direcionados), mantendo a densidade sempre no intervalo $[0\%, 100\%]$ e atingindo `100,0%` em grafos completos $K_n$.

---

## ⚙️ Algoritmos Implementados

1. **Análise de Conectividade (`VerificarConectividade`):**
   - Executa **BFS** sobre o grafo subjacente (ignorando a orientação das setas) para contar as **componentes conexas** e classificar em **Conexo** ou **Desconexo**.
   - Em **dígrafos**, quando há apenas 1 componente subjacente, executa `BfsDirecionado` a partir de um vértice inicial tanto no grafo original $G$ quanto no **grafo transposto** $G^T$ para diferenciar **Fortemente Conexo** de **Fracamente Conexo**.

2. **Detecção de Ciclos (`PossuiCiclo`):**
   - Verifica imediatamente a existência de auto-laços (`PossuiLacos`).
   - Em grafos **não-direcionados**, detecta arestas paralelas ou aplica **DFS com rastreamento de pai** (`DfsCicloNaoDirecionado`).
   - Em **dígrafos**, aplica **DFS com coloração em 3 estados** (`DfsCicloDirecionado`: `0` = não visitado, `1` = ativo na pilha de recursão, `2` = finalizado), confirmando ciclo ao encontrar um vértice no estado `1` ou comprovando que o dígrafo é um **DAG**.

3. **Fila de Transição — BFS e Algoritmo de Dijkstra (`GerarFilaTransicao` e `GerarFilaTransicaoDijkstra`):**
   - **BFS (`Queue<string>`):** Encontra o caminho com o **menor número de saltos** (transições) entre duas músicas.
   - **Dijkstra (`PriorityQueue<string, int>`):** Em grafos ponderados, encontra a **rota de maior afinidade total** convertendo a afinidade percentual em custo de fricção (`101 - Peso >= 1`), comparando os dois caminhos lado a lado.

4. **Motor de Recomendação de 1º e 2º Grau (`RecomendarAPartirDe`):**
   - **1º Grau (Diretas):** Lista os vizinhos diretos ordenados pela maior afinidade (%) com barras visuais, sinalizando o *Modo Repeat* separadamente quando ativo.
   - **2º Grau (Descobertas):** Explora os vizinhos dos vizinhos (excluindo a faixa atual e conexões já diretas), somando os pesos ou pontes em comum.

5. **Playlist Automática Contínua (`GerarPlaylistAutomatica`):**
   - Estratégia gulosa (*Greedy*): partindo da música escolhida, seleciona sempre a próxima faixa ainda não visitada (`HashSet<string>`) conectada pela aresta de maior peso.

6. **Auto-Conexão por Regras Musicais (`AutoConectarPorRegra`):**
   - Conecta automaticamente pares por **Mesmo Gênero** (`78%`), **Mesmo Artista** (`92%`) ou **BPM Compatível** (diferença $\le 8$ BPM).

---

## 🎮 Navegação e Funcionalidades

O sistema funciona totalmente com **setas (`↑` / `↓` + `ENTER`)**, atalhos numéricos (`1-9`) e `ESC` para voltar, sem precisar digitar nomes de músicas:

- **`[1]` Player Interativo no Grafo (`PlayerInterativo`):**
  - Mostra a música tocando agora, trilha percorrida, próximas conexões com barra visual de afinidade (`██████████ 96%`) e descobertas de 2º grau.
  - **`[↑/↓ + ENTER]`** Tocar próxima música conectada (percorrer aresta) | **`[A]`** Auto-DJ | **`[R]`** Ligar/Desligar Repeat (auto-laço ao vivo) | **`[F]`** Traçar fila (BFS/Dijkstra) | **`[M]`** Pular para outra faixa | **`[ESC]`** Voltar.
- **`[2]` Visualizar Estrutura do Grafo:** Lista de Adjacência paginada em árvore (`←`/`→`), Matriz de Adjacência colorida (`[L]` alterna legenda) e Catálogo/Tabela de Graus paginada.
- **`[3]` Diagnóstico de Teoria dos Grafos:** Exibe todas as propriedades estruturais com explicação resumida e distribuição por relacionamento.
- **`[4]` Playlists e Filas:** Gera playlist gulosa ou compara **BFS vs. Dijkstra** entre quaisquer duas faixas selecionadas com as setas.
- **`[5]` Editar Grafo:** Cadastra faixas, conecta músicas escolhendo origem, destino, relacionamento e peso em listas guiadas, alterna Repeat, auto-conecta por regras, filtra relacionamentos e remove vértices/arestas.
- **`[6]` Trocar Cenário ou Criar Novo Grafo:** Alterna entre os **6 presets** abaixo ou cria um grafo personalizado do zero.
- **`[7]` Exportar Relatório Completo (`.TXT`):** Salva `relatorio_soundgraph.txt` com diagnóstico, catálogo, graus, lista e matriz.

---

## 🎚️ Cenários Prontos (6 Presets)

| Preset | Cenário | Vértices | Direção | Ponderação | Estrutura / Propriedades |
| :--- | :--- | :---: | :--- | :--- | :--- |
| `[1]` | **Mega Catálogo de Streaming** *(padrão)* | 18 | Direcionado | Ponderado | Multigrafo + Pseudografo (cíclico) |
| `[2]` | **Ecossistema Pop & Synth** | 12 | Não-Direcionado | Não-Ponderado | Grafo Simples e Conexo |
| `[3]` | **Histórico Global de Fila** | 12 | Direcionado | Ponderado | Dígrafo Cíclico e Desconexo |
| `[4]` | **Álbum Conceitual Completo $K_6$** | 6 | Não-Direcionado | Não-Ponderado | Grafo Completo $K_6$ Simples (`100,0%` densidade) |
| `[5]` | **Setlist de Festival (DAG)** | 10 | Direcionado | Ponderado | Dígrafo Acíclico (**DAG**) |
| `[6]` | **Mega Mix Treino + Repeat** | 10 | Não-Direcionado | Ponderado | Multigrafo + Pseudografo |

---

## 🚀 Como Compilar e Rodar o Código

### Pré-requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (ou superior) instalado na máquina:
  ```bash
  dotnet --version
  ```

### Opção 1 — Pelo Terminal (Windows, Linux ou macOS)

1. Entre na pasta do projeto:
   ```bash
   cd aed3-soundgraph
   ```
2. Compile e execute:
   ```bash
   dotnet run
   ```
   *(Para apenas compilar sem executar, use `dotnet build`.)*

### Opção 2 — Atalho Rápido no Windows (`ExecutarSoundGraph.bat`)

1. Abra a pasta `aed3-soundgraph` no Explorador de Arquivos.
2. Dê dois cliques em `ExecutarSoundGraph.bat` (configura UTF-8 com `chcp 65001` e inicia com `dotnet run`).

### Opção 3 — Pelo Visual Studio ou VS Code

- **Visual Studio:** Abra `SoundGraph.csproj` e pressione `Ctrl + F5`.
- **VS Code:** Abra a pasta `aed3-soundgraph`, abra o terminal integrado (`Ctrl + '`) e rode `dotnet run`.

---

## 📂 Arquivos da Pasta

- `Program.cs` — Código-fonte completo em C# (`GrafoMusical`, `InterfaceConsole`, `PlayerInterativo` e `Program`).
- `SoundGraph.csproj` — Arquivo de projeto `.NET 10`.
- `ExecutarSoundGraph.bat` — Script de execução rápida com UTF-8 para Windows.
- `README.md` — Documentação completa do trabalho e explicação dos conceitos de grafos.
