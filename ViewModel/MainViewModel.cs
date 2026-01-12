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
        public Config Config { get; set; }
        public double WidthConstructorContainer
        {
            get => _widthConstructorContainer;
            set
            {
                _widthConstructorContainer = value;
                SharedState.WidthBorder = value / 3 - 45;
                OnPropertyChanged(nameof(WidthConstructorContainer));
                OnPropertyChanged(nameof(SharedState.WidthBorder));

            }
        }
        public double HeightConstructorContainer
        {
            get => _heightConstructorContainer;
            set
            {
                _heightConstructorContainer = value;
                SharedState.HeightBorder = value / 3;
                OnPropertyChanged(nameof(HeightConstructorContainer));
                OnPropertyChanged(nameof(SharedState.HeightBorder));

            }
        }
        public double PrevWidthCanvas { get; set; } = 0;
        public double PrevHeightCanvas { get; set; } = 0;
        public LeftViewModel LeftViewModel { get; set; }
        public CenterViewModel CenterViewModel { get; set; }
        public RightViewModel RightViewModel { get; set; }
        public SharedState SharedState { get; set; }
        #endregion
        #region ctor
        public MainViewModel(Canvas canvas, Popup popup)
        {
            SharedState = new SharedState();
            RightViewModel = new RightViewModel(SharedState, popup);
            LeftViewModel = new LeftViewModel(SharedState, canvas);
            CenterViewModel = new CenterViewModel();
        }
        #endregion
        #region methods
        public void ChangeSize()
        {
            if (SharedState.Blocks.Count == 0)//TODO сделать рабочее маштибирование для повернутых объектов. Ширина в высоту перетекает и обратно.
                return;

            var proportionWidth = SharedState.WidthCanvas / PrevWidthCanvas;
            var proportionHeight = SharedState.HeightCanvas / PrevHeightCanvas;
            foreach (var block in SharedState.Blocks)
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

            PrevWidthCanvas = SharedState.WidthCanvas;
            PrevHeightCanvas = SharedState.HeightCanvas;
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
