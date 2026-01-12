using barcode_gen.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace barcode_gen.ViewModel
{
    public class RightViewModel
    {
        #region varible
        private Popup _popup;
        private Mode _selectedMode = Mode.Large;

        #endregion
        #region properties
        public string ConfigName { get; set; } = string.Empty;
        public string SelectedConfigItemName { get; set; }


        public Mode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                AspectRatio ar = null;
                switch (value)
                {
                    case Mode.Large: ar = new AspectRatio(StickerRatios.Large.Width, StickerRatios.Large.Height); break;
                    case Mode.Small: ar = new AspectRatio(StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                    default: ar = new AspectRatio(StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                }
                ;



                SharedState.HeightBorder = (SharedState.WidthBorder / ar.Width) * ar.Height;
                SharedState.HeightCanvas = (SharedState.WidthCanvas / ar.Width) * ar.Height;
                OnPropertyChanged(nameof(SelectedMode));
                OnPropertyChanged(nameof(SharedState.HeightBorder));
                OnPropertyChanged(nameof(SharedState.HeightCanvas));
            }
        }
        public List<string> ConfingItemNames { get; set; }

        public SharedState SharedState { get; }
        #endregion
        #region ctor
        public RightViewModel(SharedState sharedState)
        {
            SharedState = sharedState;
            AddConfigCommand = new RelayCommand(AddConfig);
            AddingConfigCommand = new RelayCommand(AddingConfig);
            CloseConfigDialogCommand = new RelayCommand(CloseConfigDialog);

        }
        #endregion
        #region command
        public ICommand SelectConfigCommand { get; }
        public ICommand AddConfigCommand { get; }
        public ICommand AddingConfigCommand { get; }
        public ICommand CloseConfigDialogCommand { get; }


        #endregion
        #region methods
        private void AddConfig()
        {
            _popup.IsOpen = true;
        }
        private void AddingConfig()
        {
            _popup.IsOpen = false;
            var conf = new Config()
            {
                Configs =
                {
                    new ConfigItem(){
                        Blocks = SharedState.Blocks.Select(item => new BlockItem(item)).ToList(),
                        Name = ConfigName
                    }
                }
            };
            SaveConfig(conf);


        }
        private void CloseConfigDialog()
        {
            _popup.IsOpen = false;
        }

        public static void SaveConfig(Config config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("config.json", json);
        }
        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
