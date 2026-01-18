using barcode_gen.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace barcode_gen.ViewModel
{
    public class RightViewModel
    {
        #region varible
        private Popup _popup;
        private Mode _selectedMode = Mode.Large;
        private double _heightContainer = 450;
        private double _widthContainer = 800;
        #endregion
        #region properties
        public double WidthContainer
        {
            get => _widthContainer; set
            {
                _widthContainer = value;
                OnPropertyChanged(nameof(WidthContainer));
            }
        }
        public double HeightContainer
        {
            get => _heightContainer; set
            {
                _heightContainer = value;
                OnPropertyChanged(nameof(HeightContainer));
            }
        }
        public string ConfigName { get; set; } = string.Empty;
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
        public Config Config { get; set; }
        public List<ConfigItem> Configs { get; set; } = new List<ConfigItem>();
        public ConfigItem SelectedConfig { get; set; }

        public SharedState SharedState { get; }
        public LeftViewModel LeftVM { get; }
        #endregion
        #region ctor
        public RightViewModel(SharedState sharedState, Popup popup, LeftViewModel vm)
        {
            SharedState = sharedState;
            LeftVM = vm;
            AddConfigCommand = new RelayCommand(AddConfig);
            AddingConfigCommand = new RelayCommand(AddingConfig);
            CloseConfigDialogCommand = new RelayCommand(CloseConfigDialog);
            SelectConfigCommand = new RelayCommand(() =>
                DisplaySelectedConfig(SelectedConfig)
            );

            _popup = popup;
            Config = LoadConfig();
        }

        private void DisplaySelectedConfig(ConfigItem selectedConfig)
        {
            
            LeftVM.SharedState.Blocks.Clear();
            SharedState._canvas.Children.Clear();
            HeightContainer = selectedConfig.Blocks[0].MainHeight;
            WidthContainer = selectedConfig.Blocks[0].MainWidth;
            
            foreach (var block in selectedConfig.Blocks)
            {
                LeftVM.AddBlock();
            }
            for (int i = 0; i < SharedState.Blocks.Count; i++)
            {
                var block = selectedConfig.Blocks[i];
                SharedState.Blocks[i].Border.Width = block.W;
                SharedState.Blocks[i].Border.Height = block.H;
                Canvas.SetLeft(SharedState.Blocks[i].Border, block.CX - (block.W / 2));
                Canvas.SetTop(SharedState.Blocks[i].Border, block.CY - (block.H / 2));

                var fe = (FrameworkElement)SharedState.Blocks[i].Border;

                RotateTransform rt;

                if (fe.RenderTransform is RotateTransform existing)
                {
                    rt = existing; // 🔥 ВАЖНО
                }
                else
                {
                    rt = new RotateTransform(0);
                    fe.RenderTransform = rt;
                    fe.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                var delta = selectedConfig.Blocks[i].Angel;
                rt.Angle += delta;
                SharedState.Blocks[i].H = block.H;
                SharedState.Blocks[i].W = block.W;
                SharedState.Blocks[i].CX = block.CX;
                SharedState.Blocks[i].CY = block.CY;
                SharedState.Blocks[i].Angel = block.Angel;
                SharedState.Blocks[i].SelectedType = block.SelectedType;
            }
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
            Config.Configs.Add(
                 new ConfigItem()
                 {
                     Blocks = SharedState.Blocks.Select(item => new BlockItem(item)).ToList(),
                     Name = ConfigName
                 }
                );

            SaveConfig(Config);


        }
        private void CloseConfigDialog()
        {
            _popup.IsOpen = false;
        }
        public Config LoadConfig()
        {
            string path = "config.json";

            if (!File.Exists(path))
            {
                var defaultConfig = new Config
                {
                    Configs = new List<ConfigItem>()
                };

                // Сохраняем файл
                var json = JsonSerializer.Serialize(defaultConfig,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);

                return defaultConfig;
            }

            var text = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Config>(text);
            Configs = config.Configs;
            return config;
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
