using barcode_gen.Enum;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
        private int _blockLeft= 100;
        private int _blockTop = 100;
       
        #endregion
        #region property
        public int BlockTop { get => _blockTop; set
            {
                _blockTop = value;
                OnPropertyChanged();
            }
        }
        public int BlockLeft { get => _blockLeft; set 
            {
                _blockLeft = value; 
                OnPropertyChanged();
            } 
        }
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
        public Border Border { get; set; }
        #endregion
        #region command
        public ICommand RemoveCommand { get; }
        public ICommand SwitchVisibilityCommand { get; }
        public ICommand MouseDownCommand {  get; }
        public ICommand MouseUpCommand {  get; }
        public ICommand MouseMoveCommand {  get; }
        #endregion
        #region ctor
        public BlockViewModel(Action<BlockViewModel> removeAction, Border border)
        {
            Border = border;
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
           /* MouseDownCommand = new TicketCommand((UiEventPayload e) =>{ OnMouseDown(e); });
            MouseUpCommand = new TicketCommand( (UiEventPayload e) => { OnMouseUp(e); });
            MouseMoveCommand = new TicketCommand((UiEventPayload e) => { OnMouseMove(e); });*/

        }
    
        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
