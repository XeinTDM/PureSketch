using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PureSketch
{
    public partial class CanvasSizeDialog : UserControl
    {
        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; private set; }

        public event EventHandler DialogConfirmed; // Event to notify when the user confirms the dialog

        public CanvasSizeDialog() => InitializeComponent();

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) && double.TryParse(HeightTextBox.Text, out double height))
            {
                CanvasWidth = width;
                CanvasHeight = height;

                DialogConfirmed?.Invoke(this, EventArgs.Empty);
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for width and height.");
            }
        }

        private Point initialMousePosition;

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            initialMousePosition = e.GetPosition(this);
            this.CaptureMouse();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this.Parent as UIElement);
                var transform = this.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    this.RenderTransform = transform;
                }
                transform.X = currentPosition.X - initialMousePosition.X;
                transform.Y = currentPosition.Y - initialMousePosition.Y;
            }
        }

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}