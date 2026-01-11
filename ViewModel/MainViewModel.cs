using barcode_gen.Enum;
using barcode_gen.ViewModel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Canvas = System.Windows.Controls.Canvas;

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
        public static readonly AspectRatio Small = new AspectRatio(600, 411);
        public static readonly AspectRatio Large = new AspectRatio(10, 7);
    }
    public class ConteinerSizes
    {
        public double LeftX { get; set; }
        public double RightX { get; set; }
        public double TopY { get; set; }
        public double DownY { get; set; }
        public ConteinerSizes(double leftX, double rightX, double topY, double downY)
        {
            LeftX = leftX;
            RightX = rightX;
            TopY = topY;
            DownY = downY;
        }
    }
    /// <summary>
    /// Нужен чтобы нормализовать координаты точек после смещения фигуры.
    /// </summary>
    public class Figure
    {
        public Point LeftTop { get; set; }
        public Point RightTop { get; set; }
        public Point LeftDown { get; set; }
        public Point RightDown { get; set; }
        public UIElement FigureElement { get; set; }  
        public Figure(Point leftTop, Point rightTop, Point leftDown, Point rightDown, UIElement element)
        {
            LeftTop = leftTop;
            RightTop = rightTop;
            LeftDown = leftDown;
            RightDown = rightDown;
            FigureElement = element;
        }
        public Point NormilizedContainer(ConteinerSizes sizes)
        {
            IEnumerable<Point> Points = new[]
            {
                LeftTop,
                RightTop,
                LeftDown,
                RightDown
            };

            double MinX = Points.Min(p => p.X);
            double MaxX = Points.Max(p => p.X);
            double MinY = Points.Min(p => p.Y);
            double MaxY = Points.Max(p => p.Y);

            double dx = 0;
            double dy = 0;

            if (MinX < sizes.LeftX)
                dx = sizes.LeftX - MinX + 2;
            else if (MaxX > sizes.RightX)
                dx = sizes.RightX - MaxX - 2;

            if (MinY < sizes.TopY)
                dy = sizes.TopY - MinY + 2;
            else if (MaxY > sizes.DownY)
                dy = sizes.DownY - MaxY - 2;

            Shift(dx, dy);
            return new Point(dx, dy);
        }
        public void Shift(double dx, double dy)
        {
            
            LeftTop = new Point(LeftTop.X + dx, LeftTop.Y + dy);
            RightTop = new Point(RightTop.X + dx, RightTop.Y + dy);
            LeftDown = new Point(LeftDown.X + dx, LeftDown.Y + dy);
            RightDown = new Point(RightDown.X + dx, RightDown.Y + dy);
        }
    }
    #endregion

    public class MainViewModel : INotifyPropertyChanged
    {
        #region varible
        private double _widthConstructorContainer = 800;
        private double _heightConstructorContainer = 450;
        private double _widthBorder;
        private double _heightBorder;
        private double _widthCanvas;
        private double _heightCanvas;
        private bool _isDragging;
        private System.Windows.Point _lastPos;
        private Canvas _canvas;
        private Popup _popup;
        private WindowState _windowState;
        #endregion
        #region properties

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                if (_windowState == value) return;
                _windowState = value;
                OnPropertyChanged();

                /*   if (_windowState == WindowState.Minimized)
                       //OnMinimized();
                   *//*else if (_windowState == WindowState.Normal)
                       OnRestored();*//*
                   else if (_windowState == WindowState.Maximized)
                      // OnMaximized();*/
            }
        }
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
                    case Mode.Large: ar = new AspectRatio(StickerRatios.Large.Width, StickerRatios.Large.Height); break;
                    case Mode.Small: ar = new AspectRatio(StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                    default: ar = new AspectRatio(StickerRatios.Small.Width, StickerRatios.Small.Height); break;
                }
                ;



                HeightBorder = (WidthBorder / ar.Width) * ar.Height;
                HeightCanvas = (WidthCanvas / ar.Width) * ar.Height;
                OnPropertyChanged(nameof(SelectedMode));
                OnPropertyChanged(nameof(HeightBorder));
                OnPropertyChanged(nameof(HeightCanvas));
            }
        }
        public Config Config { get; set; }
        
        public List<string> ConfingItemNames{ get;set;}
        public double WidthConstructorContainer
        {
            get => _widthConstructorContainer;
            set
            {
                _widthConstructorContainer = value;
                WidthBorder = value / 3 - 45;
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
            get => _heightBorder; set
            {
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
        public double PrevWidthCanvas { get; set; } = 0;
        public double PrevHeightCanvas { get; set; } = 0;
        public string ConfigName { get; set; } = string.Empty;
        public string SelectedConfigItemName { get; set; }
        public CenterViewModel CenterViewModel { get; set; }
        #endregion
        #region command
        public ICommand AddBlockCommand { get; }
        public ICommand SelectConfigCommand { get; }
        public ICommand AddConfigCommand { get; }
        public ICommand CloseConfigDialogCommand { get; }
        public ICommand AddingConfigCommand { get; }
        #endregion
        #region ctor
        public MainViewModel(Canvas canvas, Popup popup)
        {
            CenterViewModel = new CenterViewModel();
            AddBlockCommand = new RelayCommand(AddBlock);
            /*SelectConfigCommand = new RelayCommand(AddConfig);*/
            AddConfigCommand = new RelayCommand(AddConfig);
            AddingConfigCommand = new RelayCommand(AddingConfig);
            CloseConfigDialogCommand = new RelayCommand(CloseConfigDialog);
            _canvas = canvas;
            _popup = popup;
        }
        #endregion
        #region methods
        /// <summary>
        /// For change elements in canvas
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
 
        public void ChangeSize()
        {
            if (Blocks.Count == 0)//TODO сделать рабочее маштибирование для повернутых объектов. Ширина в высоту перетекает и обратно.
                return;

            var proportionWidth = WidthCanvas / PrevWidthCanvas;
            var proportionHeight = HeightCanvas / PrevHeightCanvas;
            foreach (var block in Blocks)
            {
                var border = block.Border;
                var topLeftX = Canvas.GetLeft(border);
                var topLeftY = Canvas.GetTop(border);
                var wi = border.Width;
                var wiA = border.ActualWidth;
                var he = border.Height;
                var heA = border.ActualHeight;
                var cX = topLeftX + (border.ActualWidth / 2);
                var cY = topLeftY + (border.ActualHeight / 2);
                var newCX = cX * proportionWidth;
                var newCY = cY * proportionHeight;
                var newBorderWidth = border.ActualWidth * proportionWidth;
                var newBorderHeight = border.ActualHeight * proportionHeight;
                var resultX = newCX - (newBorderWidth / 2);
                var resultY = newCY - (newBorderHeight / 2);




                border.Width = newBorderWidth;
                border.Height = newBorderHeight;
                Canvas.SetLeft(border, resultX);
                Canvas.SetTop(border, resultY);
            }

            PrevWidthCanvas = WidthCanvas;
            PrevHeightCanvas = HeightCanvas;
        }
        private void AddBlock()
        {

            #region adding Block element
            var border = new Border
            {
                Width = 125,
                Height = 75,
                Background = Brushes.MediumSlateBlue,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Margin = new Thickness(0),

            };

            border.MouseLeftButtonUp += OnMouseUp;
            border.MouseLeftButtonDown += OnMouseDown;
            border.MouseMove += OnMouseMove;
            border.PreviewMouseWheel += Element_MouseWheel;

            Canvas.SetLeft(border, 33);
            Canvas.SetTop(border, 33);

            // Grid внутри Border
            var grid = new Grid() { Margin = new Thickness(0)};
            border.Child = grid;
            var text = new TextBlock() { Text = "Text", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};

            grid.Children.Add(text);
            #region thumb
            var thumb = new Thumb
            {
                Width = 12,
                Height = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE
            };
            var template = new ControlTemplate(typeof(Thumb));
            var factory = new FrameworkElementFactory(typeof(Grid));
            factory.SetValue(Grid.BackgroundProperty, Brushes.Transparent);
            template.VisualTree = factory;
            thumb.Template = template;
            thumb.DragDelta += (s, e) =>
            {
                var target = border;
                double newWidth = target.DesiredSize.Width + e.HorizontalChange;
                double newHeigth = target.DesiredSize.Height + e.VerticalChange;
                var canvas = _canvas;
                var elementLeft = Canvas.GetLeft(border);
                var elementTop = Canvas.GetTop(border);

                if (newWidth < 30) {
                    target.Width = 30;
                    return;
                }
                if (newHeigth < 20)
                {
                    target.Height = 20;
                    return;
                }



                if ( e.HorizontalChange < 5 ||   e.VerticalChange < 5 )
                {
                    target.Width = newWidth;
                    target.Height = newHeigth;
                    var containerLeftX = 0;
                    var containerRightX = 0 + canvas.ActualWidth;
                    var containerTopY = 0;
                    var containerDownY = 0 + canvas.ActualHeight;
                    GeneralTransform transform = target.TransformToAncestor(canvas);
                    Rect bounds = transform.TransformBounds(new Rect(
                        0,
                        0,
                        target.RenderSize.Width,
                        target.RenderSize.Height));

                    Point topLeft = new Point(bounds.TopLeft.X, bounds.TopLeft.Y);
                    Point topRight = new Point(bounds.TopRight.X, bounds.TopRight.Y);
                    Point downLeft = new Point(bounds.BottomLeft.X, bounds.BottomLeft.Y);
                    Point downRight = new Point(bounds.BottomRight.X, bounds.BottomRight.Y);

                    var figure = new Figure(topLeft, topRight, downLeft, downRight, target);
                    var delta = figure.NormilizedContainer(new ConteinerSizes(containerLeftX, containerRightX, containerTopY, containerDownY));

                    var leftTop = new Point(elementLeft ,  elementTop);


                    if (delta.X != 0 || delta.Y != 0)
                    {
                        target.Width = newWidth - e.HorizontalChange - 2;
                        target.Height = newHeigth - e.VerticalChange - 2;
                        /*Canvas.SetLeft(border, leftTop.X);
                        Canvas.SetTop(border, leftTop.Y);*/
                    }
                  
                    /*  if (delta.X != 0 || delta.Y != 0)
                      {
                          target.Width = newWidth - e.HorizontalChange;
                          target.Height = newHeigth - e.VerticalChange;
                      }*/
                }
            };
            #endregion
            grid.Children.Add(thumb);
            _canvas.Children.Add(border);
            #endregion
            var newVm = new BlockViewModel(RemoveBlock, border, text);
            newVm.H = 75;
            newVm.W = 125;
            newVm.X = 33 + (125 / 2);
            newVm.Y = 33 + (75 / 2);
            newVm.Angel = 0;
            Blocks.Add(newVm);
        }
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
                        Blocks = Blocks.Select(item => new BlockItem(item)).ToList(),
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
        private void RemoveBlock(BlockViewModel block)
        {
            Blocks.Remove(block);
            _canvas.Children.Remove(block.Border);
        }
        private void Element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var border = (Border)sender;
            var fe = (FrameworkElement)sender;

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
            var delta = e.Delta > 0 ? 5 : -5;
            rt.Angle += delta;
            

            var source = sender;
            var canvas = _canvas;
            var element = (UIElement)source;

            GeneralTransform transform = element.TransformToAncestor(canvas);
            Rect bounds = transform.TransformBounds(new Rect(
                0,
                0,
                element.RenderSize.Width,
                element.RenderSize.Height));


            

            var mouse = e;
            var pos = mouse.GetPosition(element);
            var shiftScan = mouse.GetPosition(canvas);
            var elementLeft = Canvas.GetLeft(element);
            var elementTop = Canvas.GetTop(element);
            var elementLeftBounds = bounds.X;
            var elementTopBounds = bounds.Y;
            var dx = shiftScan.X - _lastPos.X;
            var dy = shiftScan.Y - _lastPos.Y;
            var containerLeftX = 0;
            var containerRightX = 0 + canvas.ActualWidth;
            var containerTopY = 0;
            var containerDownY = 0 + canvas.ActualHeight;



            Point topLeft = new Point(bounds.TopLeft.X + dx, bounds.TopLeft.Y + dy);
            Point topRight = new Point(bounds.TopRight.X + dx, bounds.TopRight.Y + dy);
            Point downLeft = new Point(bounds.BottomLeft.X + dx, bounds.BottomLeft.Y + dy);
            Point downRight = new Point(bounds.BottomRight.X + dx, bounds.BottomRight.Y + dy);

            var figure = new Figure(topLeft, topRight, downLeft, downRight, element);
            var deltaa = figure.NormilizedContainer(new ConteinerSizes(containerLeftX, containerRightX, containerTopY, containerDownY));

            var leftTop = new Point(elementLeft + dx + deltaa.X, elementTop + dy + deltaa.Y);


            if(deltaa.X != 0 || deltaa.Y != 0)
            {
                if(delta < 0)
                    rt.Angle += 5;
                else rt.Angle -= 5;
                
            }

            _lastPos = shiftScan;
            e.Handled = true;
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mouse = e;
            var source = sender;
            var canvas = _canvas;
            _isDragging = true;
            _lastPos = mouse.GetPosition(canvas);
            ((UIElement)source).CaptureMouse();
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mouse = e;
            var source = sender;
            var canvas = _canvas;
            _isDragging = false;
            ((UIElement)source).ReleaseMouseCapture();
        }
        private void OnMouseMove(object sender, MouseEventArgs e)

        {
            var mouse = e;
            var source = sender;
            var canvas = _canvas;
            var element = (UIElement)source;

            GeneralTransform transform = element.TransformToAncestor(canvas);
            Rect bounds = transform.TransformBounds(new Rect(
                0,
                0,
                element.RenderSize.Width,
                element.RenderSize.Height));


            if (!_isDragging) return;

            
            var pos = mouse.GetPosition(element);
            var shiftScan = mouse.GetPosition(canvas);
            var elementLeft = Canvas.GetLeft(element);
            var elementTop = Canvas.GetTop(element);
            var elementLeftBounds = bounds.X;
            var elementTopBounds = bounds.Y;
            var dx = shiftScan.X - _lastPos.X;
            var dy = shiftScan.Y - _lastPos.Y;
            var containerLeftX = 0;
            var containerRightX = 0 + canvas.ActualWidth;
            var containerTopY = 0;
            var containerDownY = 0 + canvas.ActualHeight;
 
            
            
            Point topLeft = new Point(bounds.TopLeft.X + dx, bounds.TopLeft.Y + dy);
            Point topRight = new Point(bounds.TopRight.X + dx, bounds.TopRight.Y + dy);
            Point downLeft = new Point(bounds.BottomLeft.X + dx, bounds.BottomLeft.Y + dy);
            Point downRight = new Point(bounds.BottomRight.X + dx, bounds.BottomRight.Y + dy);

            var figure = new Figure(topLeft, topRight, downLeft, downRight, element);
            var delta = figure.NormilizedContainer(new ConteinerSizes(containerLeftX, containerRightX, containerTopY, containerDownY));

            var leftTop = new Point(elementLeft + dx + delta.X, elementTop + dy + delta.Y);


            Canvas.SetLeft(element, leftTop.X);
            Canvas.SetTop(element, leftTop.Y);

            _lastPos = shiftScan;
        }
        public static void SaveConfig(Config config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("config.json", json);
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
