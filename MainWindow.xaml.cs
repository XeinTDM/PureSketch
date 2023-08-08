using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Controls;

namespace PureSketch
{
    public partial class MainWindow : Window
    {
        private readonly StrokeCollection _undoStrokes = new StrokeCollection();
        private StrokeCollection _clipboardStrokes = new StrokeCollection();

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
            if (paintCanvas != null && thicknessSlider != null)
            {
                paintCanvas.DefaultDrawingAttributes.Width = thicknessSlider.Value;
                paintCanvas.DefaultDrawingAttributes.Height = thicknessSlider.Value;
            }
        }

        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            paintCanvas.Strokes.Clear();
        }

        // Save Canvas Strokes to a File
        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Ink Files (*.ink)|*.ink"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
                paintCanvas.Strokes.Save(fs);
                fs.Close();
            }
        }

        // Load Canvas Strokes from a File
        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Ink Files (*.ink)|*.ink"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                using var fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                StrokeCollection strokes = new StrokeCollection(fs);
                paintCanvas.Strokes = strokes;
                fs.Close();
            }
        }

        // Cut Selected Strokes
        private void OnCutClick(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.GetSelectedStrokes().Count == 0) return;

            _clipboardStrokes = new StrokeCollection(paintCanvas.GetSelectedStrokes());
            paintCanvas.Strokes.Remove(paintCanvas.GetSelectedStrokes());
        }

        // Copy Selected Strokes
        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.GetSelectedStrokes().Count == 0) return;

            _clipboardStrokes = new StrokeCollection(paintCanvas.GetSelectedStrokes());
        }

        // Paste Strokes
        private void OnPasteClick(object sender, RoutedEventArgs e)
        {
            if (_clipboardStrokes.Count == 0) return;

            foreach (var stroke in _clipboardStrokes)
            {
                paintCanvas.Strokes.Add(stroke.Clone());
            }
        }
    }
}