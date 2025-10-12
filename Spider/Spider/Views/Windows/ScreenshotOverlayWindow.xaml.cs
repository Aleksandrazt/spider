using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Spider.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ScreenshotOverlayWindow.xaml
    /// </summary>
    public partial class ScreenshotOverlayWindow : Window
    {
        private Point _startPoint;
        private bool _isSelecting;
        public Rect SelectedArea { get; private set; }
        public bool IsCancelled { get; private set; }

        public ScreenshotOverlayWindow()
        {
            InitializeComponent();
            
            Loaded += (s, e) => OverlayCanvas.Focus();
            
            var left = SystemParameters.VirtualScreenLeft;
            var top = SystemParameters.VirtualScreenTop;
            var width = SystemParameters.VirtualScreenWidth;
            var height = SystemParameters.VirtualScreenHeight;
            
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = true;
            _startPoint = e.GetPosition(OverlayCanvas);
            
            SelectionRectangle.Visibility = Visibility.Visible;
            InfoPanel.Visibility = Visibility.Visible;
            
            Canvas.SetLeft(SelectionRectangle, _startPoint.X);
            Canvas.SetTop(SelectionRectangle, _startPoint.Y);
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            var currentPoint = e.GetPosition(OverlayCanvas);
            
            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _startPoint.X);
            var height = Math.Abs(currentPoint.Y - _startPoint.Y);
            
            Canvas.SetLeft(SelectionRectangle, x);
            Canvas.SetTop(SelectionRectangle, y);
            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;
            
            InfoText.Text = $"{(int)width} × {(int)height}";
            Canvas.SetLeft(InfoPanel, x);
            Canvas.SetTop(InfoPanel, y - 30);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting) return;
            
            _isSelecting = false;
            
            var x = Canvas.GetLeft(SelectionRectangle);
            var y = Canvas.GetTop(SelectionRectangle);
            var width = SelectionRectangle.Width;
            var height = SelectionRectangle.Height;
            
            if (width > 5 && height > 5)
            {
                SelectedArea = new Rect(x, y, width, height);
                DialogResult = true;
                Close();
            }
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsCancelled = true;
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                SelectedArea = new Rect(0, 0, ActualWidth, ActualHeight);
                DialogResult = true;
                Close();
            }
        }
    }
}

