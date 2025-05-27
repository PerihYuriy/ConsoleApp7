using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GraphVisualization
{
    public partial class MainWindow : Window
    {
        private Graph graph;
        private Dictionary<int, Point> positions;
        private Dictionary<(int, int), Line> edgeLines;
        private Dictionary<int, Ellipse> nodeEllipses;

        public MainWindow()
        {
            InitializeComponent();
            BuildAndDrawGraph();
        }

        private void BuildAndDrawGraph()
        {
            // --- 1) Build a more irregular static graph (10 nodes) ---
            graph = new Graph();
            // some random-looking connections (but fixed)
            graph.AddEdge(0, 3);
            graph.AddEdge(0, 7);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 5);
            graph.AddEdge(1, 9);
            graph.AddEdge(2, 4);
            graph.AddEdge(2, 6);
            graph.AddEdge(3, 4);
            graph.AddEdge(3, 8);
            graph.AddEdge(4, 7);
            graph.AddEdge(5, 6);
            graph.AddEdge(5, 8);
            graph.AddEdge(6, 9);
            graph.AddEdge(7, 9);
            graph.AddEdge(8, 0);

            // --- 2) Log adjacency list ---
            Log("Список суміжності графа:");
            foreach (var u in graph.Vertices)
                Log($"  Вершина {u}: {string.Join(" ", graph.Adjacent(u))}");
            Log("");

            // --- 3) Log edges ---
            Log("Список ребер:");
            foreach (var u in graph.Vertices)
                foreach (var v in graph.Adjacent(u))
                    if (u < v)  // undirected
                        Log($"  {u} --- {v}");
            Log("");

            // --- 4) Prepare for drawing + then draw when ready ---
            positions = new Dictionary<int, Point>();
            edgeLines = new Dictionary<(int, int), Line>();
            nodeEllipses = new Dictionary<int, Ellipse>();

            Loaded += (_, __) =>
            {
                LayoutGraph();
                DrawGraph();
            };
        }


        private void LayoutGraph()
        {
            var verts = graph.Vertices;
            int n = verts.Count;
            double r = Math.Min(GraphCanvas.ActualWidth, GraphCanvas.ActualHeight) / 2 - 50;
            var center = new Point(GraphCanvas.ActualWidth / 2, GraphCanvas.ActualHeight / 2);

            for (int i = 0; i < n; i++)
            {
                double θ = 2 * Math.PI * i / n;
                positions[verts[i]] = new Point(
                    center.X + r * Math.Cos(θ),
                    center.Y + r * Math.Sin(θ)
                );
            }
        }

        private void DrawGraph()
        {
            // draw edges
            foreach (var u in graph.Vertices)
                foreach (var v in graph.Adjacent(u))
                {
                    var p1 = positions[u];
                    var p2 = positions[v];
                    var line = new Line
                    {
                        X1 = p1.X,
                        Y1 = p1.Y,
                        X2 = p2.X,
                        Y2 = p2.Y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 2
                    };
                    GraphCanvas.Children.Add(line);
                    edgeLines[(u, v)] = line;
                }

            // draw nodes + labels
            foreach (var v in graph.Vertices)
            {
                var p = positions[v];
                var circle = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(circle, p.X - 15);
                Canvas.SetTop(circle, p.Y - 15);
                GraphCanvas.Children.Add(circle);
                nodeEllipses[v] = circle;

                var lbl = new System.Windows.Controls.TextBlock
                {
                    Text = v.ToString(),
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(lbl, p.X - 5);
                Canvas.SetTop(lbl, p.Y - 10);
                GraphCanvas.Children.Add(lbl);
            }
        }

        private void HighlightStep(int current, int? from = null)
        {
            // reset colors
            foreach (var L in edgeLines.Values) L.Stroke = Brushes.LightGray;
            foreach (var N in nodeEllipses.Values) N.Fill = Brushes.LightBlue;

            // highlight node
            nodeEllipses[current].Fill = Brushes.Red;
            // highlight edge
            if (from.HasValue && edgeLines.TryGetValue((from.Value, current), out var ln))
                ln.Stroke = Brushes.Red;

            // force redraw
            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        }

        private void DfsButton_Click(object sender, RoutedEventArgs e)
            => RunTraversal(isDfs: true);

        private void BfsButton_Click(object sender, RoutedEventArgs e)
            => RunTraversal(isDfs: false);

        private void RunTraversal(bool isDfs)
        {
            if (!int.TryParse(StartVertexBox.Text, out int start) ||
                !graph.Vertices.Contains(start))
            {
                MessageBox.Show("Enter a valid start vertex (0–9).");
                return;
            }

            // run on background to keep UI fluid
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var visited = new HashSet<int>();

                if (isDfs)
                {
                    Log($"Починаємо DFS з вершини {start}");
                    var stack = new Stack<(int? from, int node)>();
                    stack.Push((null, start));

                    int step = 0;
                    while (stack.Count > 0)
                    {
                        var (from, node) = stack.Pop();
                        if (!visited.Add(node)) continue;
                        step++;
                        Log($"DFS крок {step}: відвідано вершину {node}");
                        Dispatcher.Invoke(() => HighlightStep(node, from));
                        Thread.Sleep(500);

                        foreach (var nbr in graph.Adjacent(node).Reverse())
                            if (!visited.Contains(nbr))
                                stack.Push((node, nbr));
                    }
                    Log("DFS завершено.\n");
                }
                else
                {
                    Log($"Починаємо BFS з вершини {start}");
                    var queue = new Queue<(int? from, int node)>();
                    queue.Enqueue((null, start));

                    int step = 0;
                    while (queue.Count > 0)
                    {
                        var (from, node) = queue.Dequeue();
                        if (!visited.Add(node)) continue;
                        step++;
                        Log($"BFS крок {step}: відвідано вершину {node}");
                        Dispatcher.Invoke(() => HighlightStep(node, from));
                        Thread.Sleep(500);

                        foreach (var nbr in graph.Adjacent(node))
                            if (!visited.Contains(nbr))
                                queue.Enqueue((node, nbr));
                    }
                    Log("BFS завершено.\n");
                }
            });
        }

        // Helper to append a line to the LogBox on the UI thread
        private void Log(string line)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(line + Environment.NewLine);
                LogBox.ScrollToEnd();
            });
        }
    }
}
