# SoundGraph Pro — Motor de Recomendação de Músicas e Playlists com Grafos (AED3)

Trabalho da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (.NET 10)**.

O projeto aplica na prática os principais conceitos de **Teoria dos Grafos** — incluindo **tipos estruturais, direções, ponderação, representações computacionais e algoritmos de busca/percurso** — modelados como um motor de recomendação musical, geração automática de playlists e transição de fila no terminal.

---

## 🧠 Modelagem e Conceitos de Teoria dos Grafos

Um catálogo musical é modelado como um grafo $G = (V, E)$, onde:

- **Vértices (`FaixaMusical`):** Cada vértice representa uma música cadastrada no catálogo, armazenando `Titulo`, `Artista`, `Genero`, `Album`, `Ano` e `Bpm` em um dicionário `Dictionary<string, FaixaMusical>` (*case-insensitive*).
- **Arestas / Arcos (`ConexaoMusical`) e 14 Tipos de Relacionamento (`TipoRelacionamento`):** Cada conexão liga uma faixa de origem a uma faixa de destino, carregando um **peso de afinidade** (quando ponderado) e um rótulo semântico:
  - `AfinidadeSonora`, `MesmoArtista`, `MesmoAlbum`, `MesmoGenero`, `ColaboracaoFeat`, `TransicaoFila`, `PlaylistTreino`, `PlaylistFoco`, `PlaylistFesta`, `SampleOuCover`, `BpmCompativel`, `RadioLoop`, `ModoRepeat` (auto-laço) e `Personalizado`.

### Representações Computacionais Implementadas

1. **Lista de Adjacência (`_listaAdjacencia`):**
   - Implementada como `Dictionary<string, List<ConexaoMusical>>`.
   - Ideal para grafos esparsos e consultas rápidas da vizinhança de uma faixa na recomendação e na busca em largura (BFS).
2. **Matriz de Adjacência (`ExibirMatrizAdjacencia`):**
   - Construída dinamicamente como matriz $N \times N$ (`M1` até `Mn`) a partir da lista de adjacência.
   - Evidencia visualmente a **simetria** ($M[i,j] = M[j,i]$) em grafos não-direcionados, a **assimetria** em dígrafos, a **diagonal principal** destacada para auto-laços (*Modo Repeat*) e o indicador `N*` quando há **arestas paralelas** em grafos ponderados.

### Classificações de Grafos Demonstradas no Sistema

- **Quanto à Direção:**
  - **Não-Direcionado (`EhDirecionado = false`):** Afinidade mútua/bidirecional ($u \leftrightarrow v$). Cada conexão é inserida nos dois sentidos na lista de adjacência (exceto auto-laços), o número de arestas é $|E| = \frac{\text{não-laços}}{2} + \text{laços}$, e cada auto-laço soma $+2$ ao grau do vértice.
  - **Direcionado / Dígrafo (`EhDirecionado = true`):** Transições de fila ou setlist com sentido único ($u \rightarrow v$), calculando separadamente **Grau de Entrada** ($d^-(v)$) e **Grau de Saída** ($d^+(v)$).
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
  - **Grau Médio e Densidade Relativa (%):** Calcula $\frac{\sum d(v)}{|V|}$ e a porcentagem de ocupação em relação ao máximo de arestas de um grafo simples ($n(n-1)$ para dígrafos e $\frac{n(n-1)}{2}$ para não-direcionados).

---

## ⚙️ Algoritmos Implementados

1. **Análise de Conectividade (`VerificarConectividade`):**
   - Executa **BFS** sobre o grafo subjacente (ignorando a orientação das setas) para contar as **componentes conexas** e classificar em **Conexo** ou **Desconexo**.
   - Em **dígrafos**, quando há apenas 1 componente subjacente, executa `BfsDirecionado` a partir de um vértice inicial tanto no grafo original $G$ quanto no **grafo transposto** $G^T$ (arestas invertidas) para diferenciar **Fortemente Conexo** de **Fracamente Conexo**.

2. **Detecção de Ciclos (`PossuiCiclo`):**
   - Verifica imediatamente a existência de auto-laços (`PossuiLacos`).
   - Em grafos **não-direcionados**, detecta arestas paralelas ou aplica **DFS com rastreamento de pai** (`DfsCicloNaoDirecionado`) para identificar arestas de retorno.
   - Em **dígrafos**, aplica **DFS com coloração em 3 estados** (`DfsCicloDirecionado`: `0` = não visitado, `1` = ativo na pilha de recursão, `2` = finalizado), confirmando ciclo ao encontrar um vértice no estado `1` ou comprovando que o dígrafo é um **DAG**.

3. **Fila de Transição por Menor Caminho (`GerarFilaTransicao`):**
   - Implementa **Busca em Largura (BFS)** utilizando `Queue<string>` e mapa de predecessores (`anterior`) para reconstruir o caminho com o menor número de transições entre duas músicas escolhidas.

4. **Motor de Recomendação de 1º e 2º Grau (`RecomendarAPartirDe`):**
   - **1º Grau (Diretas):** Lista os vizinhos diretos ordenados pela maior afinidade (%).
   - **2º Grau (Descobertas):** Explora os vizinhos dos vizinhos (excluindo a faixa atual e conexões já diretas), somando os pesos (em grafos ponderados) ou a quantidade de pontes em comum (em grafos não-ponderados).

5. **Playlist Automática Contínua (`GerarPlaylistAutomatica`):**
   - Estratégia gulosa (*Greedy*): partindo da música escolhida, seleciona sempre a próxima faixa ainda não visitada (`HashSet<string>`) conectada pela aresta de maior peso, exibindo a afinidade média das transições.

6. **Auto-Conexão por Regras Musicais (`AutoConectarPorRegra`):**
   - Percorre os pares de faixas do catálogo e cria automaticamente conexões por **Mesmo Gênero** (`78%`), **Mesmo Artista** (`92%`) ou **BPM Compatível** (diferença $\le 8$ BPM, com afinidade proporcional entre `65%` e `98%`).

---

## 🎚️ Cenários Prontos (6 Presets em Alta Capacidade)

A opção **`[13]`** permite alternar instantaneamente entre 6 cenários pré-configurados que demonstram na prática todas as combinações da teoria:

| Preset | Cenário | Vértices | Direção | Ponderação | Estrutura / Propriedades |
| :--- | :--- | :---: | :--- | :--- | :--- |
| `[1]` | **Mega Catálogo de Streaming** *(padrão)* | 18 | Direcionado | Ponderado | Multigrafo + Pseudografo (cíclico) |
| `[2]` | **Ecossistema Pop & Synth** | 12 | Não-Direcionado | Não-Ponderado | Grafo Simples e Conexo |
| `[3]` | **Histórico Global de Fila** | 12 | Direcionado | Ponderado | Dígrafo Cíclico e Desconexo |
| `[4]` | **Álbum Conceitual Completo $K_6$** | 6 | Não-Direcionado | Não-Ponderado | Grafo Completo $K_6$ Simples |
| `[5]` | **Setlist de Festival (DAG)** | 10 | Direcionado | Ponderado | Dígrafo Acíclico (**DAG**) |
| `[6]` | **Mega Mix Treino + Repeat** | 10 | Não-Direcionado | Ponderado | Multigrafo + Pseudografo |

---

## 🎮 Funcionalidades do Menu Interativo

- **`[1]`** Exibir **Lista de Adjacência** e **Matriz de Adjacência** coloridas no terminal.
- **`[2]`** Exibir **Diagnóstico Completo** (direção, ponderação, tipo estrutural, conectividade, ciclicidade, $K_n$, grau médio, densidade, contagem por relacionamento e tabela de graus).
- **`[3]`** Listar o catálogo completo de músicas (título, artista, gênero, ano, BPM e álbum).
- **`[4]`** Cadastrar nova música (vértice).
- **`[5]`** Conectar duas músicas escolhendo entre os **14 tipos de relacionamento**.
- **`[6]`** Ativar **Modo Repeat** em uma música (cria um auto-laço e transforma o grafo em Pseudografo).
- **`[7]`** Auto-conectar o catálogo por regras (Mesmo Gênero, Mesmo Artista ou BPM Compatível).
- **`[8]`** Filtrar conexões por tipo de relacionamento.
- **`[9]`** Remover música (vértice) ou conexão (aresta).
- **`[10]`** Rodar o **Motor de Recomendação** (1º e 2º grau).
- **`[11]`** Gerar **Playlist Automática Contínua** por maior afinidade.
- **`[12]`** Gerar **Fila de Transição** entre duas músicas via **BFS** (menor caminho).
- **`[13]`** Carregar um dos **6 Cenários Prontos**.
- **`[14]`** Criar um **Novo Grafo Musical do Zero** escolhendo livremente direção e ponderação.

---

## 🖥️ Exemplo de Saída (Diagnóstico do Cenário Padrão)

```text
=== DIAGNÓSTICO DE TIPOS E DIREÇÕES DO GRAFO ===
  • Direção              : Direcionado (Dígrafo - transições de sentido único)
  • Ponderação           : Ponderado (arestas possuem peso de afinidade %)
  • Tipo Estrutural      : Pseudografo (possui auto-laço/Repeat e arestas paralelas)
  • Auto-laços (Repeat)  : Sim
  • Arestas Paralelas    : Sim (múltiplos relacionamentos entre o mesmo par)
  • Conectividade        : Fracamente Conexo (1 componente(s))
  • Ciclicidade          : Cíclico (rádio/loop contínuo detectado)
  • Grafo Completo (Kn)  : Não
  • Grau Médio           : 3.89
  • Densidade Relativa   : 11.4%
```

---

## 🚀 Como Compilar e Rodar o Código

### Pré-requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (ou superior) instalado na máquina.
  - Para conferir se já está instalado, execute no terminal:
    ```bash
    dotnet --version
    ```

### Opção 1 — Pelo Terminal (Windows, Linux ou macOS)

1. Entre na pasta do projeto:
   ```bash
   cd aed3-soundgraph
   ```
2. Compile e execute com um único comando:
   ```bash
   dotnet run
   ```
   *(Se quiser apenas compilar sem executar, use `dotnet build`.)*

### Opção 2 — Atalho Rápido no Windows (`ExecutarSoundGraph.bat`)

1. Abra a pasta `aed3-soundgraph` no Explorador de Arquivos.
2. Dê dois cliques no arquivo `ExecutarSoundGraph.bat` (ele já configura UTF-8 com `chcp 65001` para exibir corretamente as setas `→`, `↔`, símbolos musicais e acentuação no terminal e executa `dotnet run`).

### Opção 3 — Pelo Visual Studio ou VS Code

- **Visual Studio:** Abra o arquivo `SoundGraph.csproj` e pressione `Ctrl + F5` (ou clique em **Iniciar sem Depurar**).
- **VS Code:** Abra a pasta `aed3-soundgraph`, abra o terminal integrado (`Ctrl + '`) e execute `dotnet run`.

---

## 📂 Arquivos da Pasta

- `Program.cs` — Código-fonte completo em C# (`TipoRelacionamento`, `RelacionamentoHelper`, `FaixaMusical`, `ConexaoMusical`, `DiagnosticoGrafo`, `GrafoMusical` e `Program`).
- `SoundGraph.csproj` — Arquivo de projeto `.NET 10`.
- `ExecutarSoundGraph.bat` — Script de execução rápida com suporte a UTF-8 para Windows.
- `README.md` — Documentação completa do trabalho e explicação dos conceitos de grafos.
