using System.Windows;

namespace PureSketch {


    public partial class CanvasSizeDialog : Window
    {
        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; private set; }

        public CanvasSizeDialog()
        {
            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) && double.TryParse(HeightTextBox.Text, out double height))
            {
                CanvasWidth = width;
                CanvasHeight = height;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for width and height.");
            }
        }
    }
}