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
            if (!adjList.ContainsKey(u)) adjList[u] = new List<int>();
            adjList[u].Add(v);

            if (!IsDirected)
            {
                if (!adjList.ContainsKey(v)) adjList[v] = new List<int>();
                adjList[v].Add(u);
            }
        }

        public IReadOnlyList<int> Adjacent(int v)
            => adjList.ContainsKey(v) ? adjList[v] : Array.Empty<int>();
    }
}
