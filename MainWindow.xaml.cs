using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace PureSketch
{
    public partial class MainWindow : Window
    {
        private readonly StrokeCollection _undoStrokes = new();

        public MainWindow()
        {
            InitializeComponent();
            paintCanvas.DefaultDrawingAttributes.Color = Colors.Black; // default color
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            paintCanvas.Strokes.Clear();
        }

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Strokes.Count > 0)
            {
                var lastStroke = paintCanvas.Strokes[paintCanvas.Strokes.Count - 1];
                _undoStrokes.Add(lastStroke);
                paintCanvas.Strokes.Remove(lastStroke);
            }
        }

        private void OnRedoClick(object sender, RoutedEventArgs e)
        {
            if (_undoStrokes.Count > 0)
            {
                var lastUndoStroke = _undoStrokes[_undoStrokes.Count - 1];
                paintCanvas.Strokes.Add(lastUndoStroke);
                _undoStrokes.Remove(lastUndoStroke);
            }
        }

        private void OnColorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorPicker.SelectedItem != null)
            {
                var selectedColor = ((ComboBoxItem)colorPicker.SelectedItem).Content.ToString();
                paintCanvas.DefaultDrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(selectedColor);
            }
        }

        private void OnThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            paintCanvas.DefaultDrawingAttributes.Width = thicknessSlider.Value;
            paintCanvas.DefaultDrawingAttributes.Height = thicknessSlider.Value;
        }
    }
}