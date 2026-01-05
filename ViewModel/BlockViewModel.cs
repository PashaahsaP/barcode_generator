using barcode_gen.Enum;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace barcode_gen.ViewModel
{
    public class BlockViewModel : INotifyPropertyChanged
    {
        #region varibles
      
        private ElementTypes _selectedType;
        //private String _textBoxState = "+";
        private Visibility _visibility = Visibility.Collapsed;
        private String _content = string.Empty;
        private int linesCount = 0;
        #endregion
        #region property
        
        public string Title { get; }
        public ElementTypes SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                }
            }
        }
        public String TextBoxState
        {
            get 
                {
                if (Visibility == Visibility.Collapsed)
                    return "+";
                return "-";
            }
           /* set
            {
                if (_textBoxState != value)
                {
                    _textBoxState = value;
                    OnPropertyChanged();
                }
            }*/
        }
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(this.TextBoxState));
                }
            }
        }
        public int LinesCount { get; set; }
        public ObservableCollection<ElementTypes> Types { get; }
        public String Content { get => _content;
            set
            {
                var data = value.Split('\n').Where(item => item.Replace(" ", "").Length != 0).Count();
                _content = value;
                LinesCount = data;
                OnPropertyChanged(nameof(this.Content));
                OnPropertyChanged(nameof(this.LinesCount));

            } 
        }
        #endregion
        #region command
        public ICommand RemoveCommand { get; }
        public ICommand SwitchVisibilityCommand { get; }

        #endregion
        #region ctor
        public BlockViewModel(Action<BlockViewModel> removeAction)
        {
            Title = $"Block {Guid.NewGuid().ToString()}";

            Types = new ObservableCollection<ElementTypes>(
            (ElementTypes[])barcode_gen.Enum.Mode.GetValues(typeof(ElementTypes))
        );

            // Можно задать начальное значение
            SelectedType = Types.First();
            RemoveCommand = new RelayCommand(() =>
            {
                removeAction(this);
            });
            SwitchVisibilityCommand = new RelayCommand(() =>
            {
                if (Visibility.Collapsed == Visibility)
                    Visibility = Visibility.Visible;
                else
                    Visibility = Visibility.Collapsed;

            });
        }
        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
