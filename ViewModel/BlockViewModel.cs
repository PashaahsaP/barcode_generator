using barcode_gen.Enum;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
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
        #region Helper class
        public class ProportionalPoint {
            public double X;
            public double Y;
            public ProportionalPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
        #endregion
        #region varibles

        private ElementTypes _selectedType = ElementTypes.Text;
        //private String _textBoxState = "+";
        private Visibility _visibility = Visibility.Collapsed;
        private String _content = string.Empty;
        private List<String> _contentData = new List<string>();
        private int _blockLeft= 100;
        private int _blockTop = 100;
        private ProportionalPoint _leftTop;
        private ProportionalPoint _rightTop;
        private ProportionalPoint _leftDown;
        private ProportionalPoint _rightDown;
        
       
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
                ContentData = value.Split('\n').Select(item => item.TrimEnd()).Where(item => item != "").ToList();
                LinesCount = data;
                OnPropertyChanged(nameof(this.Content));
                OnPropertyChanged(nameof(this.LinesCount));

            } 
        }
        public List<String> ContentData
        {
            get { return _contentData; }
            set
            {
                _contentData = value;
                     
            }
        }
        public Border Border{ get; set; }
        public TextBlock Text {  get; set; }
        public ProportionalPoint LeftTop { get => _leftTop; set
            {
                if (Border == null)
                    return;
                _leftTop = value;
                OnPropertyChanged();
            }
        }
        public ProportionalPoint RightTop
        {
            get => _rightTop; set
            {
                if (Border == null)
                    return;
                _rightTop = value;
                OnPropertyChanged();
            }
        }
        public ProportionalPoint LeftDown
        {
            get => _leftDown; set
            {
                if (Border == null)
                    return;
                _leftDown = value;
                OnPropertyChanged();
            }
        }
        public ProportionalPoint RightDown
        {
            get => _rightDown; set
            {
                if (Border == null)
                    return;
                _rightDown = value;
                OnPropertyChanged();
            }
        }
        public double X { get; set; }
        public double Y { get; set;  }
        public double H { get; set; }
        public double W { get; set; }
        public double Angel { get; set; }

        #endregion
        #region command
        public ICommand OptionChangedCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SwitchVisibilityCommand { get; }
        public ICommand MouseDownCommand {  get; }
        public ICommand MouseUpCommand {  get; }
        public ICommand MouseMoveCommand {  get; }
        #endregion
        #region ctor
        public BlockViewModel(Action<BlockViewModel> removeAction, Border border, TextBlock text)
        {
            Border = border;
            Title = $"Block {Guid.NewGuid().ToString()}";
            Types = new ObservableCollection<ElementTypes>(
            (ElementTypes[])barcode_gen.Enum.Mode.GetValues(typeof(ElementTypes))
        );

            // Можно задать начальное значение
            SelectedType = Types.First();
            Text = text;
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
            OptionChangedCommand = new ComboBoxCommand((s) => 
            {
                if (s == null)
                    return;
                SelectedType = (Enum.ElementTypes)s;
                Text.Text = SelectedType.ToString();
            }, (q) => { return true; }
            );
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
