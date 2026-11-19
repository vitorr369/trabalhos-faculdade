/*
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
*/

using System;

namespace TrabalhoBST
{
    class Node
    {
        public int Value;
        public Node Left;
        public Node Right;

        public Node(int value)
        {
            Value = value;
            Left = null;
            Right = null;
        }
    }

    class BinarySearchTree
    {
        public Node Root;

        public BinarySearchTree()
        {
            Root = null;
        }

        public void Insert(int value)
        {
            Root = InsertRecursive(Root, value);
        }

        private Node InsertRecursive(Node root, int value)
        {
            if (root == null)
            {
                root = new Node(value);
                return root;
            }

            if (value < root.Value)
            {
                root.Left = InsertRecursive(root.Left, value);
            }
            else if (value > root.Value)
            {
                root.Right = InsertRecursive(root.Right, value);
            }

            return root;
        }

        public bool Search(int value)
        {
            return SearchRecursive(Root, value);
        }

        private bool SearchRecursive(Node root, int value)
        {
            if (root == null)
            {
                return false;
            }

            if (root.Value == value)
            {
                return true;
            }

            if (value < root.Value)
            {
                return SearchRecursive(root.Left, value);
            }

            return SearchRecursive(root.Right, value);
        }

        public void PreOrder()
        {
            PreOrderRecursive(Root);
            Console.WriteLine();
        }

        private void PreOrderRecursive(Node root)
        {
            if (root != null)
            {
                Console.Write(root.Value + " ");
                PreOrderRecursive(root.Left);
                PreOrderRecursive(root.Right);
            }
        }

        public void InOrder()
        {
            InOrderRecursive(Root);
            Console.WriteLine();
        }

        private void InOrderRecursive(Node root)
        {
            if (root != null)
            {
                InOrderRecursive(root.Left);
                Console.Write(root.Value + " ");
                InOrderRecursive(root.Right);
            }
        }

        public void PostOrder()
        {
            PostOrderRecursive(Root);
            Console.WriteLine();
        }

        private void PostOrderRecursive(Node root)
        {
            if (root != null)
            {
                PostOrderRecursive(root.Left);
                PostOrderRecursive(root.Right);
                Console.Write(root.Value + " ");
            }
        }

        public void Remove(int value)
        {
            Root = RemoveRecursive(Root, value);
        }

        private Node RemoveRecursive(Node root, int value)
        {
            if (root == null)
            {
                return root;
            }

            if (value < root.Value)
            {
                root.Left = RemoveRecursive(root.Left, value);
            }
            else if (value > root.Value)
            {
                root.Right = RemoveRecursive(root.Right, value);
            }
            else
            {
                if (root.Left == null)
                {
                    return root.Right;
                }
                else if (root.Right == null)
                {
                    return root.Left;
                }

                root.Value = FindMinValue(root.Right);
                root.Right = RemoveRecursive(root.Right, root.Value);
            }

            return root;
        }

        public int GetHeight()
        {
            return GetHeightRecursive(Root);
        }

        private int GetHeightRecursive(Node root)
        {
            if (root == null)
            {
                return -1;
            }

            int leftHeight = GetHeightRecursive(root.Left);
            int rightHeight = GetHeightRecursive(root.Right);

            return Math.Max(leftHeight, rightHeight) + 1;
        }

        public int GetMin()
        {
            if (Root == null)
            {
                throw new InvalidOperationException();
            }
            return FindMinValue(Root);
        }

        private int FindMinValue(Node root)
        {
            int minValue = root.Value;
            while (root.Left != null)
            {
                minValue = root.Left.Value;
                root = root.Left;
            }
            return minValue;
        }

        public int GetMax()
        {
            if (Root == null)
            {
                throw new InvalidOperationException();
            }
            return FindMaxValue(Root);
        }

        private int FindMaxValue(Node root)
        {
            int maxValue = root.Value;
            while (root.Right != null)
            {
                maxValue = root.Right.Value;
                root = root.Right;
            }
            return maxValue;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BinarySearchTree bst = new BinarySearchTree();
            int[] elements = { 8, 4, 12, 2, 6, 10, 14, 1, 3, 5, 7, 9, 11, 13, 15 };

            foreach (int element in elements)
            {
                bst.Insert(element);
            }

            Console.WriteLine("=== ESTRUTURA DA ARVORE ===");
            
            Console.Write("Pre-Ordem: ");
            bst.PreOrder();

            Console.Write("Em-Ordem: ");
            bst.InOrder();

            Console.Write("Pos-Ordem: ");
            bst.PostOrder();

            Console.WriteLine("\n=== INFORMACOES ===");
            Console.WriteLine("Altura da arvore: " + bst.GetHeight());
            Console.WriteLine("Menor valor: " + bst.GetMin());
            Console.WriteLine("Maior valor: " + bst.GetMax());

            Console.WriteLine("\n=== BUSCA ===");
            Console.WriteLine("O numero 10 existe na arvore? " + bst.Search(10));
            Console.WriteLine("O numero 99 existe na arvore? " + bst.Search(99));

            Console.WriteLine("\n=== REMOCAO DE NOS ===");
            bst.Remove(1);
            Console.WriteLine("Apos remover 1 (Caso 1 - Folha), Em-Ordem:");
            bst.InOrder();

            bst.Remove(14);
            Console.WriteLine("Apos remover 14 (Caso 2 - Apenas filho esquerdo devido a arvore anterior), Em-Ordem:");
            bst.InOrder();

            bst.Remove(8);
            Console.WriteLine("Apos remover 8 (Caso 3 - Dois filhos / Raiz), Em-Ordem:");
            bst.InOrder();
        }
    }
}
