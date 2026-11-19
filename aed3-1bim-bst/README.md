# Árvore Binária de Busca — BST (AED3 - 1º Bimestre em C#)

Trabalho do **1º Bimestre** da disciplina de **Algoritmos e Estrutura de Dados 3** desenvolvido em C#.

## Estrutura da árvore inserida

```text
                  8
                /   \
              /       \
            /           \
          4              12
        /   \          /    \
      /       \      /        \
     2         6    10        14
    / \       / \   / \       / \
   1   3     5   7 9   11   13   15
```

## Operações implementadas

- **Inserção recursiva** (`Insert`)
- **Busca binária** (`Search`)
- **Percursos em profundidade:** Pré-Ordem (`PreOrder`), Em-Ordem (`InOrder`) e Pós-Ordem (`PostOrder`)
- **Métricas da árvore:** Altura (`GetHeight`), Menor valor (`GetMin`) e Maior valor (`GetMax`)
- **Remoção de nós nos 3 casos** (`Remove`): nó folha, nó com um filho e nó com dois filhos (sucessor em-ordem)
