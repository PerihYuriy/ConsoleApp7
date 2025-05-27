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
            InitializeEmptyGraph();
        }

        private void InitializeEmptyGraph()
        {
            graph = new Graph();
            positions = new Dictionary<int, Point>();
            edgeLines = new Dictionary<(int, int), Line>();
            nodeEllipses = new Dictionary<int, Ellipse>();

            Log("Програма запущена. Додайте вершини та ребра для створення графа.");
            Log("");
        }

        private void LoadDemoGraph()
        {
            graph = new Graph();
            var demoEdges = new[] {
                (0, 1), (0, 2), (1, 3),
                (2, 4), (3, 4), (4, 5)
        };

            foreach (var (u, v) in demoEdges)
            {
                graph.AddEdge(u, v);
            }

            RefreshVisualization();
            Log("Демо-граф завантажено.");
        }

        private void RefreshVisualization()
        {
            if (graph.Vertices.Count == 0)
            {
                GraphCanvas.Children.Clear();
                return;
            }

            Log("Оновлено список суміжності:");
            foreach (var u in graph.Vertices.OrderBy(x => x))
            {
                var adjacent = graph.Adjacent(u).OrderBy(x => x);
                if (adjacent.Any())
                    Log($"  Вершина {u}: {string.Join(" ", adjacent)}");
                else
                    Log($"  Вершина {u}: (ізольована)");
            }
            Log("");

            if (GraphCanvas.ActualWidth > 0 && GraphCanvas.ActualHeight > 0)
            {
                LayoutGraph();
                DrawGraph();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LayoutGraph();
                    DrawGraph();
                }), DispatcherPriority.Loaded);
            }
        }

        private void LayoutGraph()
        {
            var verts = graph.Vertices.OrderBy(x => x).ToList();
            int n = verts.Count;

            if (n == 0) return;

            double canvasWidth = Math.Max(GraphCanvas.ActualWidth, 400);
            double canvasHeight = Math.Max(GraphCanvas.ActualHeight, 300);

            if (n == 1)
            {
                positions[verts[0]] = new Point(canvasWidth / 2, canvasHeight / 2);
                return;
            }

            double r = Math.Min(canvasWidth, canvasHeight) / 2 - 50;
            var center = new Point(canvasWidth / 2, canvasHeight / 2);

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
            GraphCanvas.Children.Clear();
            edgeLines.Clear();
            nodeEllipses.Clear();

            foreach (var u in graph.Vertices)
            {
                foreach (var v in graph.Adjacent(u))
                {
                    if (u < v)
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
                        edgeLines[(v, u)] = line;
                    }
                }
            }

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

                var lbl = new TextBlock
                {
                    Text = v.ToString(),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(lbl, p.X - 8);
                Canvas.SetTop(lbl, p.Y - 8);
                GraphCanvas.Children.Add(lbl);
            }
        }

        private void HighlightStep(int current)
        {
            foreach (var node in nodeEllipses.Values)
                node.Fill = Brushes.LightBlue;

            if (nodeEllipses.ContainsKey(current))
                nodeEllipses[current].Fill = Brushes.Red;

            Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        }

        private void AddVertexButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(VertexBox.Text, out int vertex))
                {
                    MessageBox.Show("Введіть правильне числове значення для вершини.", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (graph.Vertices.Contains(vertex))
                {
                    MessageBox.Show($"Вершина {vertex} вже існує в графі.", "Увага",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                graph.EnsureVertex(vertex);
                RefreshVisualization();
                Log($"Додано вершину: {vertex}");
                VertexBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні вершини: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(EdgeFromBox.Text, out int from) ||
                    !int.TryParse(EdgeToBox.Text, out int to))
                {
                    MessageBox.Show("Введіть правильні числові значення для обох вершин ребра.", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (from == to)
                {
                    MessageBox.Show("Вершини ребра повинні бути різними (петлі не підтримуються).", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (graph.Adjacent(from).Contains(to))
                {
                    MessageBox.Show($"Ребро {from}-{to} вже існує в графі.", "Увага",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                graph.AddEdge(from, to);
                RefreshVisualization();
                Log($"Додано ребро: {from} --- {to}");
                EdgeFromBox.Clear();
                EdgeToBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні ребра: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearGraphButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете видалити весь граф?", "Підтвердження",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                InitializeEmptyGraph();
                RefreshVisualization();
                Log("Граф очищено.");
            }
        }

        private void LoadDemoButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDemoGraph();
        }

        private void DfsButton_Click(object sender, RoutedEventArgs e)
            => RunTraversal(isDfs: true);

        private void BfsButton_Click(object sender, RoutedEventArgs e)
            => RunTraversal(isDfs: false);

        private void RunTraversal(bool isDfs)
        {
            if (graph == null || graph.Vertices.Count == 0)
            {
                MessageBox.Show("Спочатку створіть граф.", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StartVertexBox.Text, out int start) ||
                !graph.Vertices.Contains(start))
            {
                MessageBox.Show($"Введіть правильну початкову вершину. Доступні вершини: {string.Join(", ", graph.Vertices.OrderBy(x => x))}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var visited = new HashSet<int>();
                var allVertices = graph.Vertices.ToHashSet();

                if (isDfs)
                {
                    Log($"Починаємо DFS з вершини {start}");
                    RunDFS(start, visited);

                    var unvisited = allVertices.Except(visited).OrderBy(x => x).ToList();
                    foreach (var vertex in unvisited)
                    {
                        if (!visited.Contains(vertex))
                        {
                            Log($"Знайдено нову компоненту зв'язності. Продовжуємо DFS з вершини {vertex}");
                            RunDFS(vertex, visited);
                        }
                    }
                    Log("DFS завершено.\n");
                }
                else
                {
                    Log($"Починаємо BFS з вершини {start}");
                    RunBFS(start, visited);

                    var unvisited = allVertices.Except(visited).OrderBy(x => x).ToList();
                    foreach (var vertex in unvisited)
                    {
                        if (!visited.Contains(vertex))
                        {
                            Log($"Знайдено нову компоненту зв'язності. Продовжуємо BFS з вершини {vertex}");
                            RunBFS(vertex, visited);
                        }
                    }
                    Log("BFS завершено.\n");
                }
            });
        }

        private void RunDFS(int start, HashSet<int> visited)
        {
            var stack = new Stack<int>();
            stack.Push(start);
            int step = 0;

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!visited.Add(node)) continue;

                step++;
                Log($"DFS крок {step}: відвідано вершину {node}");
                Dispatcher.Invoke(() => HighlightStep(node));
                Thread.Sleep(1000);

                var neighbors = graph.Adjacent(node).Where(n => !visited.Contains(n)).OrderByDescending(x => x);
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }
        }

        private void RunBFS(int start, HashSet<int> visited)
        {
            var queue = new Queue<int>();
            queue.Enqueue(start);
            visited.Add(start);
            int step = 0;

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                step++;
                Log($"BFS крок {step}: відвідано вершину {node}");
                Dispatcher.Invoke(() => HighlightStep(node));
                Thread.Sleep(1000);

                foreach (var neighbor in graph.Adjacent(node).OrderBy(x => x))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private void Log(string line)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(line + Environment.NewLine);
                LogBox.ScrollToEnd();
            });
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Clear();
        }
    }
}