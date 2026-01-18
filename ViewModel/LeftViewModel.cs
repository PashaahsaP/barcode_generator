using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace barcode_gen.ViewModel
{
    public class LeftViewModel : INotifyPropertyChanged
    {
        #region varible
        private bool _isDragging;
        private System.Windows.Point _lastPos;

        #endregion
        #region properties
        public SharedState SharedState { get; }
        #endregion
        #region ctor
        public LeftViewModel(SharedState sharedState, Canvas canvas)
        {
            SharedState = sharedState;
            AddBlockCommand = new RelayCommand(AddBlock);
            
            SharedState._canvas = canvas;

        }
        #endregion
        #region method
        public void AddBlock()
        {

            #region adding Block element
            var text = new TextBlock() { Text = "Text", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
            var border = new System.Windows.Controls.Border
            {
                Width = 125,
                Height = 75,
                Background = Brushes.MediumSlateBlue,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Margin = new Thickness(0),

            };
            var newVm = new BlockViewModel(RemoveBlock, border, text);

            border.MouseLeftButtonUp += OnMouseUp;
            border.MouseLeftButtonDown += OnMouseDown;
            /*border.MouseMove += OnMouseMove;*/
            border.MouseMove += (s, e) =>
            {
                var mouse = e;
                var source = s;
                var canvas = SharedState._canvas;
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

                newVm.CX = leftTop.X + (newVm.W / 2);
                newVm.CY = leftTop.Y + (newVm.H / 2);
                Console.WriteLine($"canvasW:{SharedState._canvas.ActualWidth} canvasH:{SharedState._canvas.ActualHeight} X:{leftTop.X} Y: {leftTop.Y}");
                _lastPos = shiftScan;
            };
            border.PreviewMouseWheel += (s, e) =>
            {
                var fe = (FrameworkElement)s;

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
                newVm.Angel = rt.Angle;


                var source = s;
                var canvas = SharedState._canvas;
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


                if (deltaa.X != 0 || deltaa.Y != 0)
                {
                    if (delta < 0)
                    {
                        rt.Angle += 5;
                    }
                    else
                    {
                        rt.Angle -= 5;
                    }

                }
                Console.WriteLine($"canvasW:{SharedState._canvas.ActualWidth} canvasH:{SharedState._canvas.ActualHeight} X:{leftTop.X} Y: {leftTop.Y}");

                _lastPos = shiftScan;
                e.Handled = true;
            };

            Canvas.SetLeft(border, 33);
            Canvas.SetTop(border, 33);

            // Grid внутри Border
            var grid = new Grid() { Margin = new Thickness(0) };
            border.Child = grid;

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
                var canvas = SharedState._canvas;
                var elementLeft = Canvas.GetLeft(border);
                var elementTop = Canvas.GetTop(border);

                if (newWidth < 30)
                {
                    target.Width = 30;
                    return;
                }
                if (newHeigth < 20)
                {
                    target.Height = 20;
                    return;
                }



                if (e.HorizontalChange < 5 || e.VerticalChange < 5)
                {
                    target.Width = newWidth;
                    target.Height = newHeigth;
                    newVm.W = newWidth;
                    newVm.H = newHeigth;
                    newVm.CX = elementLeft + (newVm.W / 2);
                    newVm.CY = elementTop + (newVm.H / 2);

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

                    var leftTop = new Point(elementLeft, elementTop);


                    if (delta.X != 0 || delta.Y != 0)
                    {
                        target.Width = newWidth - e.HorizontalChange - 2;
                        target.Height = newHeigth - e.VerticalChange - 2;
                        newVm.W = newWidth;
                        newVm.H = newHeigth;
                        newVm.CX = leftTop.X + (newVm.W / 2);
                        newVm.CY = leftTop.Y + (newVm.H / 2);
                    }
                    Console.WriteLine($"canvasW:{SharedState._canvas.ActualWidth} canvasH:{SharedState._canvas.ActualHeight} X:{leftTop.X} Y: {leftTop.Y}");

                }
            };
            #endregion
            grid.Children.Add(thumb);
            SharedState._canvas.Children.Add(border);
            #endregion
            newVm.H = 75;
            newVm.W = 125;
            newVm.CX = 33 + (125 / 2);
            newVm.CY = 33 + (75 / 2);
            newVm.Angel = 0;
            SharedState.Blocks.Add(newVm);
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mouse = e;
            var source = sender;
            var canvas = SharedState._canvas;
            _isDragging = true;
            _lastPos = mouse.GetPosition(canvas);
            ((UIElement)source).CaptureMouse();
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var mouse = e;
            var source = sender;
            var canvas = SharedState._canvas;
            _isDragging = false;
            ((UIElement)source).ReleaseMouseCapture();
        }

        private void RemoveBlock(BlockViewModel block)
        {
            SharedState.Blocks.Remove(block);
            SharedState._canvas.Children.Remove(block.Border);
        }
        #endregion
        #region command
        public ICommand AddBlockCommand { get; }

        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
