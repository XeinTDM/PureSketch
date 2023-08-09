﻿using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System;

namespace PureSketch
{
    public partial class MainWindow : Window
    {
        private readonly StrokeCollection _undoStrokes = new();
        private StrokeCollection _clipboardStrokes = new();
        private double _zoomLevel = 1.0;
        private const double ZoomFactor = 0.2;

        public MainWindow()
        {
            InitializeComponent();

            CanvasSizeDialog sizeDialog = new CanvasSizeDialog();
            if (sizeDialog.ShowDialog() == true)
            {
                paintCanvas.Width = sizeDialog.CanvasWidth;
                paintCanvas.Height = sizeDialog.CanvasHeight;
            }
            else
            {
                // Handle case where user closes the dialog without entering values, e.g., close the application.
                this.Close();
            }

            paintCanvas.DefaultDrawingAttributes.Color = Colors.Black;
        }

        private void OnMainWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        OnUndoClick(sender, e);
                        break;
                    case Key.Y:
                        OnRedoClick(sender, e);
                        break;
                    case Key.S:
                        OnSaveClick(sender, e);
                        break;
                    case Key.O:
                        OnOpenClick(sender, e);
                        break;
                    case Key.X:
                        OnCutClick(sender, e);
                        break;
                    case Key.C:
                        OnCopyClick(sender, e);
                        break;
                    case Key.V:
                        OnPasteClick(sender, e);
                        break;
                }
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            paintCanvas.Strokes.Clear();
        }

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Strokes.Count > 0)
            {
                var lastStroke = paintCanvas.Strokes[^1];
                _undoStrokes.Add(lastStroke);
                paintCanvas.Strokes.Remove(lastStroke);
            }
        }

        private void OnRedoClick(object sender, RoutedEventArgs e)
        {
            if (_undoStrokes.Count > 0)
            {
                var lastUndoStroke = _undoStrokes[^1];
                paintCanvas.Strokes.Add(lastUndoStroke);
                _undoStrokes.Remove(lastUndoStroke);
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

        private static void SaveCanvasAsPng(string filename, InkCanvas canvas, double width, double height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                pngEncoder.Save(fs);
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "PNG Files (*.png)|*.png" };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveCanvasAsPng(saveFileDialog.FileName, paintCanvas, paintCanvas.Width, paintCanvas.Height);
            }
        }


        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Files (*.png)|*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                Image image = new Image
                {
                    Source = bitmap,
                    Width = bitmap.Width,
                    Height = bitmap.Height
                };

                paintCanvas.Strokes.Clear();
                paintCanvas.Children.Add(image);
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

        private void OnZoomInClick(object sender, RoutedEventArgs e)
        {
            _zoomLevel += ZoomFactor;
            ApplyZoom();
        }

        private void OnZoomOutClick(object sender, RoutedEventArgs e)
        {
            _zoomLevel -= ZoomFactor;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            if (_zoomLevel < 0.2) _zoomLevel = 0.2;
            if (_zoomLevel > 5) _zoomLevel = 5;
            ScaleTransform scale = new ScaleTransform(_zoomLevel, _zoomLevel);
            paintCanvas.LayoutTransform = scale;
        }

        private void OnColorPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                paintCanvas.DefaultDrawingAttributes.Color = e.NewValue.Value;
            }
        }

        private void OnPaintCanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(paintCanvas);
            double relativeX = mousePosition.X / paintCanvas.ActualWidth;
            double relativeY = mousePosition.Y / paintCanvas.ActualHeight;

            paintCanvas.RenderTransformOrigin = new Point(relativeX, relativeY);

            if (e.Delta > 0)
            {
                _zoomLevel += ZoomFactor;
            }
            else
            {
                _zoomLevel -= ZoomFactor;
            }
            ApplyZoom();
        }


    }
}