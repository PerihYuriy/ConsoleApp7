using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphVisualization
{
    public class Graph
    {
        private readonly Dictionary<int, List<int>> adjList;
        public bool IsDirected { get; }
        public IReadOnlyList<int> Vertices => adjList.Keys.ToList();

        public Graph(bool directed = false)
        {
            adjList = new Dictionary<int, List<int>>();
            IsDirected = directed;
        }

        public void AddEdge(int u, int v)
        {
            EnsureVertex(u);
            EnsureVertex(v);

            adjList[u].Add(v);
            if (!IsDirected)
            {
                adjList[v].Add(u);
            }
        }

        public void EnsureVertex(int vertex)
        {
            if (!adjList.ContainsKey(vertex))
            {
                adjList[vertex] = new List<int>();
            }
        }

        public IReadOnlyList<int> Adjacent(int v)
            => adjList.ContainsKey(v) ? adjList[v] : Array.Empty<int>();
    }
}