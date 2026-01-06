using barcode_gen.Enum;
using barcode_gen.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace barcode_gen
{
    #region Helper class
    /// <summary>
    /// Cоотношение высоты и ширины
    /// </summary>
    public sealed class AspectRatio
    {
        public int Width { get; }
        public int Height { get; }

        public AspectRatio(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public double Value => (double)Width / Height;
    }
    /// <summary>
    /// Отдает соотношение сторон наклеек
    /// </summary>
    /// 
    public static class StickerRatios
    {
        public static readonly AspectRatio Small = new AspectRatio(3, 2);
        public static readonly AspectRatio Large = new AspectRatio(10, 7);
    }
    #endregion

    internal class MainViewModel : INotifyPropertyChanged
    {
        #region varible
        private double _widthConstructorContainer = 800 ;
        private double _heightConstructorContainer = 450 ;
        private double _widthBorder;
        private double _heightBorder;
        private double _widthCanvas;
        private double _heightCanvas;
        #endregion
        #region properties
        private Mode _selectedMode = Mode.Large;
        public ObservableCollection<BlockViewModel> Blocks { get; }
                            = new ObservableCollection<BlockViewModel>();
        public Mode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                AspectRatio ar = null;
                switch (value)
                {
                    case Mode.Large: ar =  new AspectRatio( StickerRatios.Large.Width, StickerRatios.Large.Height); break;
                    case Mode.Small: ar = new AspectRatio( StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                    default: ar = new AspectRatio(StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                };



               // HeightBorder = (WidthBorder / ar.Width) * ar.Height;
               // HeightCanvas = (WidthCanvas / ar.Width) * ar.Height; 
                OnPropertyChanged(nameof(SelectedMode));
                OnPropertyChanged(nameof(HeightBorder));
                OnPropertyChanged(nameof(HeightCanvas));
            }
        }
        public double WidthConstructorContainer
        {
            get => _widthConstructorContainer;
            set
            {
                _widthConstructorContainer = value;
                WidthBorder  = value / 3 - 45;
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
                HeightBorder = value / 3;
                OnPropertyChanged(nameof(HeightConstructorContainer));
                OnPropertyChanged(nameof(HeightBorder));

            }
        }
        public double WidthBorder
        {
            get => _widthBorder; set 
            {
                _widthBorder = value;
                WidthCanvas = value;
                OnPropertyChanged(nameof(WidthBorder));
                OnPropertyChanged(nameof(WidthCanvas));
            }


        }
        public double HeightBorder
        {
            get => _heightBorder; set {
                _heightBorder = value;
                HeightCanvas = value;
                OnPropertyChanged(nameof(HeightBorder));
                OnPropertyChanged(nameof(HeightCanvas));
            }

        }
        public double WidthCanvas
        {
            get => _widthBorder - 6; set 
            {
                _widthCanvas = value; 
                OnPropertyChanged(nameof(WidthCanvas));
            }

        }
        public double HeightCanvas
        {
            get => _heightBorder - 6; set 
            { 
                _heightCanvas = value; 
                OnPropertyChanged(nameof(HeightCanvas)); 
            }

        }
        #endregion
        #region command
        public ICommand AddBlockCommand { get; }
        #endregion
        #region ctor
        public MainViewModel()
        {
            AddBlockCommand = new RelayCommand(AddBlock);
        }
        #endregion
        #region methods
        private void AddBlock()
        {
            Blocks.Add(new BlockViewModel(RemoveBlock));
        }
        private void RemoveBlock(BlockViewModel block)
        {
            Blocks.Remove(block);
        }
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
        #endregion
    }
}
