using System;
using System.Collections.Generic;
using System.Threading;

class Graph
{
    private Dictionary<int, List<int>> adjList;
    private bool isDirected;

    public Graph(bool directed = false)
    {
        adjList = new Dictionary<int, List<int>>();
        isDirected = directed;
    }

    public void AddEdge(int u, int v)
    {
        if (!adjList.ContainsKey(u)) adjList[u] = new List<int>();
        adjList[u].Add(v);

        if (!isDirected)
        {
            if (!adjList.ContainsKey(v)) adjList[v] = new List<int>();
            adjList[v].Add(u);
        }
    }

    public void PrintAdjacencyList()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Список суміжності графа:");
        foreach (var kvp in adjList)
        {
            Console.Write($"Вершина {kvp.Key}: ");
            foreach (var neighbor in kvp.Value)
            {
                Console.Write(neighbor + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public void PrintEdges()
    {
        Console.WriteLine("Список ребер:");
        foreach (var u in adjList)
        {
            foreach (var v in u.Value)
            {
                if (isDirected || u.Key < v)
                    Console.WriteLine($"{u.Key} --- {v}");
            }
        }
        Console.WriteLine();
    }

    public void DFS(int start)
    {
        var visited = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(start);

        int step = 0;
        Console.WriteLine("Пошук у глибину (DFS):");

        while (stack.Count > 0)
        {
            int node = stack.Pop();
            if (!visited.Contains(node))
            {
                visited.Add(node);
                step++;
                Console.WriteLine($"Крок {step}: відвідано вершину {node}");
                Console.Write("Стека: ");
                foreach (var s in stack) Console.Write(s + " ");
                Console.WriteLine("\n");
                Thread.Sleep(500);

                if (adjList.ContainsKey(node))
                {
                    for (int i = adjList[node].Count - 1; i >= 0; i--)
                    {
                        int neighbor = adjList[node][i];
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }
        }
    }

    public void BFS(int start)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(start);

        int step = 0;
        Console.WriteLine("Пошук у ширину (BFS):");

        while (queue.Count > 0)
        {
            int node = queue.Dequeue();
            if (!visited.Contains(node))
            {
                visited.Add(node);
                step++;
                Console.WriteLine($"Крок {step}: відвідано вершину {node}");
                Console.Write("Черга: ");
                foreach (var q in queue) Console.Write(q + " ");
                Console.WriteLine("\n");
                Thread.Sleep(500);

                if (adjList.ContainsKey(node))
                {
                    foreach (var neighbor in adjList[node])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Graph g = new Graph();
        g.AddEdge(0, 1);
        g.AddEdge(0, 2);
        g.AddEdge(1, 3);
        g.AddEdge(2, 4);
        g.AddEdge(3, 4);
        g.AddEdge(4, 5);

        g.PrintAdjacencyList();
        g.PrintEdges();

        Console.Write("Введіть стартову вершину для DFS: ");
        int dfsStart = int.Parse(Console.ReadLine());
        g.DFS(dfsStart);

        Console.Write("\nВведіть стартову вершину для BFS: ");
        int bfsStart = int.Parse(Console.ReadLine());
        g.BFS(bfsStart);

        Console.WriteLine("\n=== Кінець програми ===");
    }
}
