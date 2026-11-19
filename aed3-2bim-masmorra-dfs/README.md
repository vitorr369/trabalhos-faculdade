# Gerador Procedural e Analisador de Masmorras com DFS em Grafos (AED3 - 2º Bimestre)

Trabalho do **2º Bimestre** da disciplina de **Algoritmos e Estrutura de Dados 3 (AED3)** desenvolvido em **C# (.NET 10)**.

O projeto aplica conceitos clássicos de **Teoria dos Grafos** e **Busca em Profundidade (Depth-First Search — DFS)** com **Backtracking** para gerar masmorras procedurais garantidamente solucionáveis, analisá-las estruturalmente, resolver caminhos em tempo real, jogar no terminal e exportar relatórios completos em `.txt`.

---

## 🧠 Modelagem em Teoria dos Grafos

A masmorra é modelada como um **grafo não-direcionado, não-ponderado e conexo** $G = (V, E)$ construído sobre uma grade bidimensional configurável de $L \times C$ salas (de $4 \times 4$ até $18 \times 28$, padrão $10 \times 18$):

- **Vértices (`Vertice`):** Cada sala da grade possui identificador único (`Id`), coordenadas `(Linha, Coluna)`, tipo de conteúdo (`ConteudoSala`), profundidade a partir da raiz (`Profundidade`), tempo de descoberta (`d[v]`) e tempo de finalização (`f[v]`).
- **Arestas e Lista de Adjacência (`ListaAdjacencia`):** Representada via `Dictionary<Vertice, List<Vertice>>`. Uma aresta só é adicionada entre duas salas vizinhas quando o algoritmo derruba a parede entre elas.
- **Propriedade de Árvore Geradora (Spanning Tree):** Como o DFS nunca abre arestas para vértices já visitados, o grafo gerado é **conexo e acíclico**, garantindo:
  - $|E| = |V| - 1$ arestas;
  - Caminho único entre quaisquer dois vértices da masmorra;
  - Tempo de finalização da raiz (`Entrada`) igual a $f[\text{Entrada}] = 2 \cdot |V|$.

---

## ⚙️ Algoritmos Implementados

1. **Geração Procedural Iterativa (`GerarComDFSIterativo`):**
   - Implementa o algoritmo *Randomized DFS* (Recursive Backtracker) utilizando uma pilha explícita (`Stack<Vertice>`) e um conjunto de visitados (`HashSet<Vertice>`).
   - A partir da entrada `V0(0,0)`, escolhe aleatoriamente um vizinho ortogonal não visitado, cria uma aresta não-direcionada e empilha o próximo vértice.
   - Ao chegar a um beco sem saída, executa **Backtracking** (`pilha.Pop()`), contabilizando o total de retrocessos (`TotalBacktracksGeracao`) até visitar todos os $|V|$ vértices.
   - Suporta reprodutibilidade determinística por meio de **Seed** numérica e modo animado frame a frame no console.

2. **Mapeamento de Tempos e Profundidade (`MapearTemposEProfundidadeDFSRecursivo`):**
   - Executa uma varredura DFS recursiva sobre a `ListaAdjacencia` gerada para calcular os timestamps $d[v]$ e $f[v]$, a ordem de descoberta (`OrdemDescoberta`) e a profundidade de cada vértice.
   - Define automaticamente a sala do **Chefe** (`[B]`) no vértice de **maior profundidade** em relação à Entrada (`[E]`).

3. **Distribuição Estratégica de Conteúdo (`PopularConteudo`):**
   - **Tesouros (`[T]`):** Posicionados exclusivamente em vértices folha (**grau 1**, becos sem saída) distintos da Entrada e do Chefe.
   - **Armadilhas (`[X]`):** Posicionadas fora do caminho crítico entre `Entrada` e `Chefe`, mantendo a rota principal da solução livre de armadilhas.

4. **Resolução de Caminho (`BuscarCaminhoDFS`):**
   - Executa busca em profundidade recursiva entre quaisquer dois vértices (como `Entrada -> Chefe` ou `Posição Atual -> Chefe` na Bússola), retornando um objeto `ResultadoBuscaDFS` com o caminho encontrado, vértices visitados, número de backtracks e salas descartadas em becos (`[.]`).

---

## 🎮 Funcionalidades do Sistema

No menu principal interativo do terminal, o sistema oferece:

- **`[1]` Gerar nova masmorra:** Escolha entre tamanhos pré-definidos (`Pequena 7x11`, `Media 10x18`, `Grande 14x24`), tamanho personalizado (`4..18` linhas × `4..28` colunas) ou informe uma **Seed** específica para reproduzir um mapa.
- **`[2]` Animação passo a passo:** Acompanhe ao vivo o DFS abrindo arestas (`AVANCANDO`) e retornando pelos becos (`BACKTRACKING`), com opção de pular pressionando qualquer tecla.
- **`[3]` Resolver masmorra com DFS:** Exibe o caminho solução (`[*]`) da Entrada até o Chefe e destaca todos os vértices explorados e descartados por backtracking (`[.]`).
- **`[4]` Jogar / Explorar a masmorra:** Navegue pelas arestas usando `W/A/S/D` ou as **Setas** do teclado com `3` vidas, colete tesouros (`+100` pontos), desvie de armadilhas (`-1` vida) e ative a **Bússola DFS em tempo real** pressionando `H`.
- **`[5]` Inspecionar Grafo:** Consulte a **Lista de Adjacência** por faixa de IDs, inspecione qualquer vértice `(linha, coluna)` (grau, profundidade, `d[v]`, `f[v]`, distância até a entrada) ou visualize a ordem de descoberta DFS.
- **`[6]` Estatísticas de Teoria dos Grafos:** Exibe $|V|$, $|E|$, validação de Árvore Geradora ($|E| = |V| - 1$), grau mínimo/médio/máximo, quantidade e porcentagem de nós folha (becos sem saída), profundidade máxima e $f[\text{Entrada}] = 2 \cdot |V|$.
- **`[7]` Exportar relatório `.TXT`:** Salva o arquivo `masmorra_seed_<SEED>.txt` com todas as métricas, caminho da solução, mapa ASCII e lista de adjacência completa.
- **`[8]` Configurações:** Ajusta o atraso da animação (`5` a `300 ms`) e alterna as paredes entre blocos Unicode (`██`) e ASCII puro (`##`).

---

## 🗺️ Exemplo de Mapa Gerado (`7x11`, Seed `482910`)

```text
##############################################
##E ##* * * ##        * * * * * ##B * * * * ##
##* ##* ##* ##########* ######* ##########* ##
##* * * ##* * * * * * * ##* * * ##* * * ##* ##
##########################* ######* ##* ##* ##
##X . . ##* * *   T ##* * * ##* * * ##* ##* ##
##. ######* ##* ######* ######* ######* ##* ##
##. . . ##* ##* * * * * ##T   * ##T . * ##* ##
######. ##* ##################* ######* ##* ##
##. . . ##* ##* * * * * * * ##* * * ##* * * ##
##. ######* ##* ##########* ##  ##* ######  ##
##. ##* * * ##* ##T ##* * * ##T ##* ##  ##X ##
##. ##* ######* ##. ##* ##########* ##  ##  ##
##X . * * * * * ##. . * * * * * * * ##      ##
##############################################
```

**Legenda:** `[E]` Entrada | `[B]` Chefe | `[T]` Tesouro (beco grau 1) | `[X]` Armadilha | `[*]` Caminho DFS | `[.]` Visitado e descartado por Backtracking

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
   cd aed3-2bim-masmorra-dfs
   ```
2. Compile e execute com um único comando:
   ```bash
   dotnet run
   ```
   *(Se quiser apenas compilar sem executar ainda, use `dotnet build`.)*

### Opção 2 — Atalho Rápido no Windows (`ExecutarMasmorra.bat`)

1. Abra a pasta `aed3-2bim-masmorra-dfs` no Explorador de Arquivos.
2. Dê dois cliques no arquivo `ExecutarMasmorra.bat` (ele já configura UTF-8 com `chcp 65001` para exibir corretamente os blocos e acentos e inicia o sistema).
   - *Observação:* Caso nunca tenha compilado o projeto antes na máquina, rode `dotnet build` uma primeira vez dentro da pasta antes de usar o `.bat`.

### Opção 3 — Pelo Visual Studio ou VS Code

- **Visual Studio:** Abra o arquivo `MasmorraDFS.csproj` e pressione `Ctrl + F5` (ou clique em **Iniciar sem Depurar**).
- **VS Code:** Abra a pasta `aed3-2bim-masmorra-dfs`, abra o terminal integrado (`Ctrl + '`) e execute `dotnet run`.

> **Dica de visualização:** Caso os blocos das paredes (`██`) apareçam desalinhados na fonte do seu terminal, acesse a opção **`[8]` Configuracoes** no menu e alterne para **ASCII Puro `[##]`**.

---

## 📂 Arquivos da Pasta

- `Program.cs` — Código-fonte completo em C# (`Vertice`, `GrafoMasmorra`, `ResultadoBuscaDFS`, `EstatisticasGrafo`, `RenderizadorConsole`, `ModoExploracao` e `Program`).
- `MasmorraDFS.csproj` — Arquivo de projeto `.NET 10`.
- `ExecutarMasmorra.bat` — Script de execução rápida para Windows.
- `README.md` — Documentação completa do trabalho.
