using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace barcode_gen.ViewModel
{
    public class SharedState : INotifyPropertyChanged
    {
        #region varible
        private double _widthCanvas;
        private double _heightCanvas;
        private double _widthBorder;
        private double _heightBorder;
        #endregion
        #region properties
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
            get => _heightBorder; set
            {
                _heightBorder = value;
                HeightCanvas = value;
                OnPropertyChanged(nameof(HeightBorder));
                OnPropertyChanged(nameof(HeightCanvas));
            }

        }
        public ObservableCollection<BlockViewModel> Blocks { get; }
                    = new ObservableCollection<BlockViewModel>();
        #endregion
        #region ctor
        #endregion
        #region command
        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
