using barcode_gen.Enum;
using barcode_gen.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace barcode_gen
{


    internal class MainViewModel : INotifyPropertyChanged
    {


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
