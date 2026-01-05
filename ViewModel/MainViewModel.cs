using barcode_gen.Enum;
using barcode_gen.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace barcode_gen
{


    internal class MainViewModel : INotifyPropertyChanged
    {

        private double _widthConstructorContainer = 800 ;
        private double _heightConstructorContainer = 450 ;
        private double _widthBorder;
        private double _heightBorder;
        private double _widthCanvas;
        private double _heightCanvas;

        private void AddBlock()
        {
            Blocks.Add(new BlockViewModel(RemoveBlock));
        }
        private void RemoveBlock(BlockViewModel block)
        {
            Blocks.Remove(block);
        }
        private Mode _selectedMode = Mode.Large;
        public ObservableCollection<BlockViewModel> Blocks { get; }
                            = new ObservableCollection<BlockViewModel>();
        public ICommand AddBlockCommand { get; }
        public Mode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                OnPropertyChanged();
            }
        }
        public double WidthConstructorContainer
        {
            get => _widthConstructorContainer;
            set
            {
                _widthConstructorContainer = value;
                _widthBorder  = value / 3 - 45;
                OnPropertyChanged(nameof(WidthConstructorContainer));
                OnPropertyChanged(nameof(WidthBorder));
            }
        }
        public double HeightConstructorContainer
        {
            get => _heightConstructorContainer;
            set
            {
                _heightConstructorContainer = value;
                _heightBorder = value / 3;
                OnPropertyChanged(nameof(HeightConstructorContainer));
                OnPropertyChanged(nameof(HeightBorder));

            }
        }
        public double WidthBorder
        {
            get => _widthBorder; set 
            {
                _widthBorder = value;
                _widthCanvas = value;
                OnPropertyChanged(nameof(WidthCanvas));
                OnPropertyChanged(nameof(WidthBorder));

            }


        }
        public double HeightBorder
        {
            get => _heightBorder; set {
                _heightBorder = value;
                _heightCanvas = value;
                OnPropertyChanged(nameof(HeightCanvas));
                OnPropertyChanged(nameof(HeightBorder));

            }

        }
        public double WidthCanvas
        {
            get => _widthBorder - 6; set => _widthCanvas = value;

        }
        public double HeightCanvas
        {
            get => _heightBorder - 6; set => _heightCanvas = value;

        }
        public MainViewModel()
        {
            AddBlockCommand = new RelayCommand(AddBlock);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
