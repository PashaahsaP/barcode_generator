using DocumentFormat.OpenXml.InkML;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.OneD;
using ZXing.QrCode;
using Canvas = System.Windows.Controls.Canvas;

namespace barcode_gen
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var canvas = FieldCanvas;
            var vm = new MainViewModel(FieldCanvas);
            DataContext = vm;
        }

        #region event methods
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string curDirectory = Directory.GetCurrentDirectory();
            string[] dataList = GetData(this);
            var sizes = getSizeOfLabel(this);
            //var type = getLabelType(this);
            SaveBitmapToFile(dataList, curDirectory, sizes.Item1, sizes.Item2, BarcodeFormat.CODE_128);//item1 is width item2 is height/ REMOVE  BarcodeFormat.CODE_128 it is stub
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var prefix = this.TBPrefix.Text;
            var suffix = this.TBSuffix.Text;
            var from = Int32.Parse(this.TBFrom.Text);
            var to = Int32.Parse(this.TBTo.Text);
            var step = Int32.Parse(this.TBStep.Text);
            var mask = this.TBMask.Text;
            var current = from;
            bool isGenerate = current < to;
            var collection = new List<String>();

            while (isGenerate)
            {
                var sb = new StringBuilder();
                if (mask.Length > from.ToString().Length)
                {
                    sb.Append('0', mask.Length);
                    string sb2 = new string(current.ToString().Reverse().ToArray());
                    for (int i = 0; i < sb2.Length; i++)
                    {
                        sb[i] = sb2[i];
                    }
                    var reverse = new string(sb.ToString().Reverse().ToArray());
                    collection.Add(prefix + reverse + suffix);

                }
                else
                {
                    collection.Add(prefix + current + suffix);
                }
                current = current + step;
                isGenerate = current <= to;
            }

            foreach (var item in collection)
            {
                TBData.Text = TBData.Text + item + "\r\n";
            }

        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TBData.Text = "";

        }
        #endregion
        #region helper methods
        /*        private BarcodeFormat getLabelType(MainWindow mainWindow)
                {
                    var type = ((ComboBoxItem)this.CBType.SelectedItem).Content;
                    switch(type) 
                    {
                        case "Code 128": return BarcodeFormat.CODE_128;
                        case"Matrix" : return BarcodeFormat.DATA_MATRIX;
                        case "Qr code": return BarcodeFormat.QR_CODE;
                        default: return BarcodeFormat.CODE_128;
                    }
                }*/
        // TODO перенести во view model
        private (int, int) getSizeOfLabel(MainWindow mainWindow)
        {
            foreach (System.Windows.Controls.RadioButton item in this.GSizes.Children)
            {
                if (item.IsChecked == true)
                {
                    switch (item.Content)
                    {
                        case "Малая": return (125, 85);
                        case "Большая": return (240, 130);
                        default: return (0, 0);
                    }
                }
            }
            return (0, 0);
        }
        private string[] GetData(MainWindow mainWindow)
        {
            var data = this.TBData.Text.Split("\r\n".ToCharArray());
            return data;
        }
        public void SaveBitmapToFile(string[] dataList, string curDirectory, int labelWidth, int labelHeight, BarcodeFormat type)
        {
            List<LabelElement> labels = new List<LabelElement> {
                new LabelElement
                {
                    Value = "ABC123",
                    X = 5,
                    Y = 5,
                    Width = 50,
                    Height = 30,
                    Kind = BarcodeFormat.CODE_128,
                    Text = "Товар 1"
                },
                new LabelElement
                {
                    Value = "XYZ456",
                    X = 5,
                    Y = 5,
                    Width = 50,
                    Height = 30,
                    Kind = BarcodeFormat.CODE_128,
                    Text = "Товар 2"
                },
                new LabelElement
                {
                    Value = "111222",
                    X = 5,
                    Y = 5,
                    Width = 50,
                    Height = 30,
                    Kind = BarcodeFormat.CODE_128,
                    Text = "Штрихкод"
                }
            };
            SaveLabelsToPdf(labels, "C:\\Users\\Work\\Pictures\\lable.pdf");
        }
        public void SaveLabelsToPdf(List<LabelElement> labels, string path)
        {
            PdfDocument doc = new PdfDocument();

            foreach (var label in labels)
            {
                // создаём новую страницу
                PdfPage page = doc.AddPage();

                // можно настроить размер страницы под наклейку
                page.Width = 60;   // в пунктов (примерно px)
                page.Height = 40;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                // генерируем штрих-код
                Bitmap bmp = BarcodeRenderer.Render(label.Kind, label.Value, 60, 40);

                // сохраняем Bitmap в поток
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    XImage xImg = XImage.FromStream(ms);
                    gfx.DrawImage(xImg, label.X, label.Y, label.Width, label.Height); // рисуем на всю страницу
                }

                // опционально добавить текст
                /* if (!string.IsNullOrEmpty(label.Text))
                 {
                     gfx.DrawString(label.Text,
                         new XFont("Arial", 12),
                         XBrushes.Black,
                         new XRect(0, label.Height - 20, label.Width, 20),
                         XStringFormats.Center);
                 }*/
            }

            // сохраняем PDF
            doc.Save(path);
        }
        #endregion
        public class LabelElement
        {
            public string Value { get; set; }       // данные для штрих-кода
            public int X { get; set; }              // позиция слева на странице (px)
            public int Y { get; set; }              // позиция сверху на странице (px)
            public int Width { get; set; }          // ширина наклейки (px)
            public int Height { get; set; }         // высота наклейки (px)
            public BarcodeFormat Kind { get; set; }   // тип штрих-кода
            public string Text { get; set; }        // текст надписи (если нужен)
        }
        public static class BarcodeRenderer
        {
            public static Bitmap Render(
                BarcodeFormat kind,
                string value,
                int widthPx,
                int heightPx)
            {
                switch (kind)
                {
                    case BarcodeFormat.QR_CODE: return RenderQr(value, widthPx, heightPx);
                    case BarcodeFormat.DATA_MATRIX: return RenderDataMatrix(value, widthPx, heightPx);
                    case BarcodeFormat.CODE_128: return RenderCode128(value, widthPx, heightPx);

                    default:
                        throw new NotSupportedException(
                            "Barcode type not supported: " + kind);
                }
            }

            private static Bitmap RenderQr(string value, int w, int h)
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = w,
                        Height = h,
                        Margin = 0,
                        ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M
                    }
                };
                return writer.Write(value);
            }
            private static Bitmap RenderCode128(string value, int w, int h)
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new Code128EncodingOptions
                    {
                        Width = w,
                        Height = h,
                        Margin = 0,
                        PureBarcode = true
                    }
                };
                return writer.Write(value);
            }
            private static Bitmap RenderDataMatrix(string value, int w, int h)
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.DATA_MATRIX,
                    Options = new EncodingOptions
                    {
                        Width = w,
                        Height = h,
                        Margin = 0
                    }
                };
                return writer.Write(value);
            }
        }
        public static class BitmapConverter
        {
            public static ImageSource ToImageSource(Bitmap bitmap)
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();

                    return image;
                }
            }
        }
        #region dnd
       /* private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            lastPos = e.GetPosition(LabelCanvas);
            ((UIElement)sender).CaptureMouse();
        }

        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {

            if (!isDragging) return;

            var element = (UIElement)sender;
            var pos = e.GetPosition(LabelCanvas);
            var elementLeft = System.Windows.Controls.Canvas.GetLeft(element) + lastPos.X;
            var elementTop = System.Windows.Controls.Canvas.GetTop(element) + lastPos.Y;
            var dx = pos.X - lastPos.X;
            var dy = pos.Y - lastPos.Y;
            var leftX = pos.X;
            var rightX = pos.X + LabelCanvas.ActualWidth;
            var topY = pos.Y;
            var downY = pos.Y + LabelCanvas.ActualHeight;
            #region shifting 
            if (topY < (elementTop + dy) && downY > (elementTop + element.DesiredSize.Height + dy))
            {
                System.Windows.Controls.Canvas.SetTop(element, Canvas.GetTop(element) + dy);
            }
            else
            {
                if (topY >= (elementTop + dy))
                {
                    Canvas.SetTop(element, Canvas.GetTop(element) + 1);
                }
                else
                {
                    Canvas.SetTop(element, Canvas.GetTop(element) - 1);
                }
            }
            if (leftX < (elementLeft + dx) && rightX > (elementLeft + element.DesiredSize.Width + dx))
            {
                Canvas.SetLeft(element, Canvas.GetLeft(element) + dx);
            }
            else
            {
                if (leftX >= (elementLeft + dx))
                {
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + 1);
                }
                else
                {
                    Canvas.SetLeft(element, Canvas.GetLeft(element) - 1);
                }
            }
            #endregion
            lastPos = pos;
            //Console.WriteLine($"canvas: {leftX}:{rightX} {topY}:{downY}");
            //Console.WriteLine($"element: {Canvas.GetLeft(element)}:{Canvas.GetTop(element)}");
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }*/

        #endregion

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            /*var border = ResizableBorder;
            //var text = TextBoxD;
            var element = (UIElement)sender;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(element);
            GeneralTransform transform =
                element.TransformToAncestor(LabelCanvas);
            Rect transformedBounds = transform.TransformBounds(bounds);
            System.Windows.Point realLeft = transformedBounds.TopLeft;

            double newWidth = border.ActualWidth + e.HorizontalChange;
            double newHeight = border.ActualHeight + e.VerticalChange;
            if (border.ActualHeight + e.VerticalChange > 20)
            {
                border.Height = newHeight;
            }
            if (border.ActualWidth + e.HorizontalChange > 30)
            {
                border.Width = newWidth;
            }*/
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
