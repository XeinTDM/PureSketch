using System.Windows.Ink;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PureSketch
{
    public class Layer : INotifyPropertyChanged
    {
        private string _name;
        private bool _isVisible = true;
        private StrokeCollection _strokes = new StrokeCollection();

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public StrokeCollection Strokes
        {
            get => _strokes;
            set
            {
                _strokes = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
