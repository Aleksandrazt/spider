using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spider.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ScreenshotEditorWindow.xaml
    /// </summary>
    public partial class ScreenshotEditorWindow : Window
    {
        private BitmapSource _screenshot;
        private DrawingMode _currentMode = DrawingMode.Pen;
        private Point _startPoint;
        private Shape? _currentShape;
        private SolidColorBrush _selectedColor = new SolidColorBrush(Colors.Red);
        private double _brushSize = 4;
        private readonly List<UIElement> _shapes = new();

        public enum DrawingMode
        {
            Pen,
            Arrow,
            Rectangle,
            Ellipse,
            Text,
            Highlight
        }

        public Brush SelectedColor => _selectedColor;
        public BitmapSource? EditedScreenshot { get; private set; }
        public bool SaveToFile { get; private set; }
        public bool CopyToClipboard { get; private set; }

        public ScreenshotEditorWindow(BitmapSource screenshot)
        {
            InitializeComponent();
            
            _screenshot = screenshot;
            
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Width = _screenshot.PixelWidth;
            DrawingCanvas.Height = _screenshot.PixelHeight;
            DrawingCanvas.Background = new ImageBrush(_screenshot);
            
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            UpdateDrawingAttributes();
            
            PenButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
        }

        private void UpdateDrawingAttributes()
        {
            if (DrawingCanvas == null) return;
            
            var attributes = new DrawingAttributes
            {
                Color = _selectedColor.Color,
                Height = _brushSize,
                Width = _brushSize,
                StylusTip = StylusTip.Ellipse,
                IgnorePressure = true
            };

            if (_currentMode == DrawingMode.Highlight)
            {
                attributes.Color = Color.FromArgb(100, _selectedColor.Color.R, _selectedColor.Color.G, _selectedColor.Color.B);
                attributes.Height = 20;
                attributes.Width = 20;
            }

            DrawingCanvas.DefaultDrawingAttributes = attributes;
        }

        private void SetActiveButton(Button activeButton)
        {
            PenButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            ArrowButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            RectangleButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            EllipseButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            TextButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            HighlightButton.Background = new SolidColorBrush(Color.FromRgb(62, 62, 62));
            
            activeButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
        }

        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Pen;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            UpdateDrawingAttributes();
            SetActiveButton(PenButton);
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Arrow;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
            SetActiveButton(ArrowButton);
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Rectangle;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
            SetActiveButton(RectangleButton);
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Ellipse;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
            SetActiveButton(EllipseButton);
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Text;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
            SetActiveButton(TextButton);
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            _currentMode = DrawingMode.Highlight;
            DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            UpdateDrawingAttributes();
            SetActiveButton(HighlightButton);
        }

        private void ColorBorder_Click(object sender, MouseButtonEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _selectedColor = new SolidColorBrush(Color.FromArgb(
                    colorDialog.Color.A,
                    colorDialog.Color.R,
                    colorDialog.Color.G,
                    colorDialog.Color.B));
                
                UpdateDrawingAttributes();
                OnPropertyChanged(nameof(SelectedColor));
            }
        }

        private void BrushSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrushSizeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string sizeStr)
            {
                _brushSize = double.Parse(sizeStr);
                UpdateDrawingAttributes();
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentMode == DrawingMode.Pen || _currentMode == DrawingMode.Highlight)
                return;

            _startPoint = e.GetPosition(DrawingCanvas);

            if (_currentMode == DrawingMode.Text)
            {
                var textBox = new TextBox
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    BorderBrush = _selectedColor,
                    Foreground = _selectedColor,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    MinWidth = 100
                };

                InkCanvas.SetLeft(textBox, _startPoint.X);
                InkCanvas.SetTop(textBox, _startPoint.Y);
                DrawingCanvas.Children.Add(textBox);
                _shapes.Add(textBox);
                textBox.Focus();
                return;
            }

            _currentShape = _currentMode switch
            {
                DrawingMode.Rectangle => new System.Windows.Shapes.Rectangle
                {
                    Stroke = _selectedColor,
                    StrokeThickness = _brushSize,
                    Fill = Brushes.Transparent
                },
                DrawingMode.Ellipse => new Ellipse
                {
                    Stroke = _selectedColor,
                    StrokeThickness = _brushSize,
                    Fill = Brushes.Transparent
                },
                DrawingMode.Arrow => new Line
                {
                    Stroke = _selectedColor,
                    StrokeThickness = _brushSize,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Triangle
                },
                _ => null
            };

            if (_currentShape != null)
            {
                DrawingCanvas.Children.Add(_currentShape);
                _shapes.Add(_currentShape);
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _currentShape == null)
                return;

            var currentPoint = e.GetPosition(DrawingCanvas);

            if (_currentShape is System.Windows.Shapes.Rectangle rect)
            {
                var x = Math.Min(_startPoint.X, currentPoint.X);
                var y = Math.Min(_startPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);

                InkCanvas.SetLeft(rect, x);
                InkCanvas.SetTop(rect, y);
                rect.Width = width;
                rect.Height = height;
            }
            else if (_currentShape is Ellipse ellipse)
            {
                var x = Math.Min(_startPoint.X, currentPoint.X);
                var y = Math.Min(_startPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);

                InkCanvas.SetLeft(ellipse, x);
                InkCanvas.SetTop(ellipse, y);
                ellipse.Width = width;
                ellipse.Height = height;
            }
            else if (_currentShape is Line line)
            {
                line.X1 = _startPoint.X;
                line.Y1 = _startPoint.Y;
                line.X2 = currentPoint.X;
                line.Y2 = currentPoint.Y;
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _currentShape = null;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.Strokes.Count > 0)
            {
                DrawingCanvas.Strokes.RemoveAt(DrawingCanvas.Strokes.Count - 1);
            }
            else if (_shapes.Count > 0)
            {
                var lastShape = _shapes[_shapes.Count - 1];
                DrawingCanvas.Children.Remove(lastShape);
                _shapes.RemoveAt(_shapes.Count - 1);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Strokes.Clear();
            foreach (var shape in _shapes)
            {
                DrawingCanvas.Children.Remove(shape);
            }
            _shapes.Clear();
        }

        private BitmapSource RenderToBitmap()
        {
            var renderBitmap = new RenderTargetBitmap(
                (int)DrawingCanvas.ActualWidth,
                (int)DrawingCanvas.ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);

            renderBitmap.Render(DrawingCanvas);
            return renderBitmap;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            EditedScreenshot = RenderToBitmap();
            SaveToFile = true;
            CopyToClipboard = false;
            DialogResult = true;
            Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            EditedScreenshot = RenderToBitmap();
            SaveToFile = false;
            CopyToClipboard = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P) PenButton_Click(sender, e);
            else if (e.Key == Key.A) ArrowButton_Click(sender, e);
            else if (e.Key == Key.R) RectangleButton_Click(sender, e);
            else if (e.Key == Key.E) EllipseButton_Click(sender, e);
            else if (e.Key == Key.T) TextButton_Click(sender, e);
            else if (e.Key == Key.H) HighlightButton_Click(sender, e);
            else if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control) UndoButton_Click(sender, e);
            else if (e.Key == Key.Escape) CancelButton_Click(sender, e);
            else if (e.Key == Key.Enter) SaveButton_Click(sender, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
        }
    }
}

