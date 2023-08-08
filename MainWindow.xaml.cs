using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PureSketch
{
    public partial class MainWindow : Window
    {
        private readonly StrokeCollection _undoStrokes = new();
        private StrokeCollection _clipboardStrokes = new();

        public MainWindow()
        {
            InitializeComponent();
            paintCanvas.DefaultDrawingAttributes.Color = Colors.Black;
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

        private static void SaveCanvasAsPng(string filename, InkCanvas canvas)
        {
            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("Canvas size is invalid.");
                return;
            }

            int toolboxWidth = 150; // Adjust as per your toolbox width

            // Create a temporary RenderTargetBitmap to capture InkCanvas
            RenderTargetBitmap tempRtb = new((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Default);
            tempRtb.Render(canvas);

            width -= toolboxWidth;

            RenderTargetBitmap finalRtb = new(width, height, 96d, 96d, PixelFormats.Default);

            // Offset the drawing to "crop" out the toolbox area
            DrawingVisual drawingVisual = new();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(tempRtb, new Rect(0, 0, width, height));
            }
            finalRtb.Render(drawingVisual);

            PngBitmapEncoder pngEncoder = new();
            pngEncoder.Frames.Add(BitmapFrame.Create(finalRtb));

            using (FileStream fs = File.OpenWrite(filename))
            {
                pngEncoder.Save(fs);
            }
        }


        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Files (*.png)|*.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveCanvasAsPng(saveFileDialog.FileName, paintCanvas);
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
                StrokeCollection strokes = new(fs);
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