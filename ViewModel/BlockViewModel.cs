using barcode_gen.Enum;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace barcode_gen.ViewModel
{
    public class BlockViewModel
    {
        #region varibles
        private ElementTypes _selectedType;
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
        public ObservableCollection<ElementTypes> Types { get; }
        #endregion
        #region command
        public ICommand RemoveCommand { get; }
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
        }
        #endregion

        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
