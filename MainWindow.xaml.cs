using barcode_gen.Enum;
using barcode_gen.Fonts.Resolvers;
using DocumentFormat.OpenXml.Drawing.Charts;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.OneD;
using ZXing.QrCode;
using static barcode_gen.MainWindow;
using Canvas = System.Windows.Controls.Canvas;

namespace barcode_gen
{
    public class RotatedLabelElement : LabelElement
    {
        public double Angel { get; set; }
        public System.Drawing.Point RotatedStartPoint { get; set; }
        public System.Drawing.Point CenterPoint { get; set; }

    }
    public class Config
    {
        public List<Block> Blocks { get; set; } = new List<Block>();
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Config config { get; set; }
        public MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Console.WriteLine(name);
            }
            GlobalFontSettings.FontResolver = new CustomFontResolver();
            var canvas = FieldCanvas;
            var vm = new MainViewModel(FieldCanvas, ConfigPopup);
            DataContext = vm;
            viewModel = vm;
            config = LoadConfig();
        }
        public static Config LoadConfig()
        {
            string path = "config.json";

            // Файл отсутствует → создаём новый конфиг
            if (!File.Exists(path))
            {
                var defaultConfig = new Config
                {
                    Blocks = new List<Block>() // пустая коллекция
                };

                // Сохраняем файл
                var json = JsonSerializer.Serialize(defaultConfig,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);

                return defaultConfig;
            }

            // Если файл есть → читаем его
            var text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(text);
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
        private (int weigth, int height) getSizeOfLabelForPrint(Mode mode)
        {
            switch (mode)
            {
                case Mode.Small: return (60, 40);
                case Mode.Large: return (100, 70);
                default: return (0, 0);
            }

        }
        private string[] GetData(MainWindow mainWindow)
        {
            var data = this.TBData.Text.Split("\r\n".ToCharArray());
            return data;
        }
        public void SaveBitmapToFile(string[] dataList, string curDirectory, int labelWidth, int labelHeight, BarcodeFormat type)
        {
            var contents = viewModel.Blocks
                .Select((block) => block.ContentData).ToList();
            if (contents.Count() == 0)
                return;


            (int width, int height) sizes = getSizeOfLabelForPrint(viewModel.SelectedMode);
            var proportionWidth = sizes.width / viewModel.WidthCanvas;
            var proportionHeight = sizes.height / viewModel.HeightCanvas;
            var queue = new Queue<String>();
            var data = new List<List<RotatedLabelElement>>();
            for (int i = 0; i < contents.First().Count; i++)
            {
                queue = new Queue<string>();
                for (int j = 0; j < contents.Count(); j++)
                {
                    queue.Enqueue(contents[j][i]);
                }
                data.Add(CreateLabels(proportionWidth, proportionHeight, queue));
            }
            //Надо склеить данные, перебрать блоки
            if (data.Count == 0)
                return;
            SaveLabelsToPdf(data, "C:\\Users\\Work\\Pictures\\lable.pdf");

        }
        private List<RotatedLabelElement> CreateLabels(double proportionWidth, double proportionHeight, Queue<string> values)
        {
            var labels = new List<RotatedLabelElement>();
            foreach (var block in viewModel.Blocks)
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

                GeneralTransform transform = border.TransformToAncestor(FieldCanvas);
                Rect bounds = transform.TransformBounds(new Rect(
                    0,
                    0,
                    border.RenderSize.Width,
                    border.RenderSize.Height));

                var fe = (FrameworkElement)border;

                RotateTransform rt = new RotateTransform(0);

                if (fe.RenderTransform is RotateTransform existing)
                {
                    rt = existing; // 🔥 ВАЖНО
                }

                //надо центр новой фигуры, угол поворота и точка начала с учетом поворота
                labels.Add(new RotatedLabelElement
                {
                    Value = values.Dequeue(),
                    X = (int)resultX,
                    Y = (int)resultY == 0 ? 1 : (int)resultY,
                    Width = (int)newBorderWidth,
                    Height = (int)newBorderHeight,
                    Kind = ConvertElementTypesToBarcodeFormat(block.SelectedType),
                    CenterPoint = new System.Drawing.Point((int)newCX, (int)newCY),
                    Angel = rt.Angle,
                    RotatedStartPoint = new System.Drawing.Point((int)(bounds.BottomLeft.X * proportionWidth), (int)(bounds.BottomLeft.Y * proportionHeight))
                });
            }

            return labels;
        }
        public void SaveLabelsToPdf(List<List<RotatedLabelElement>> labels, string path)
        {
            PdfDocument doc = new PdfDocument();
            foreach (var bunchOfLabels in labels)
            {
                PdfPage page = doc.AddPage();
                (int width, int height) sizes = getSizeOfLabelForPrint(viewModel.SelectedMode);
                page.Width = sizes.width;   // в пунктов (примерно px)
                page.Height = sizes.height;
                XGraphics gfx = XGraphics.FromPdfPage(page);

                foreach (var label in bunchOfLabels)
                {

                    if (label.Kind != BarcodeFormat.ITF)
                    {
                        Bitmap bmp = BarcodeRenderer.Render(label.Kind, label.Value, 250, 100);
                        bmp.SetResolution(200, 200);
                        // сохраняем Bitmap в поток
                        using (var ms = new MemoryStream())
                        {
                            bmp.Save(ms, ImageFormat.Png);
                            ms.Position = 0;

                            XImage xImg = XImage.FromStream(ms);

                            if (label.Angel != 0)
                            {
                                var state = gfx.Save();
                                gfx.TranslateTransform(label.CenterPoint.X, label.CenterPoint.Y);
                                gfx.RotateTransform(label.Angel);
                                gfx.TranslateTransform(-label.Width / 3, -label.Height * 1.2);
                                gfx.DrawImage(xImg, label.X, label.Y, label.Width, label.Height);
                                gfx.Restore(state);
                            }
                            else
                            {
                                gfx.DrawImage(xImg, label.X, label.Y, label.Width, label.Height);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(label.Value))
                        {
                            var font = new XFont("Times New Roman", 5);
                            var widthOnOneSign = 2.5;
                            var singsInOneLine = (int)(label.Width / widthOnOneSign);
                            var lines = new List<string>();
                            double lineHeight = font.GetHeight();
                            String temp = "";
                            int counter = 0;
                            for (int i = 0; i < label.Value.Length; i++, counter++)
                            {
                                temp += label.Value[i];
                                if (counter == singsInOneLine - 1)
                                {
                                    lines.Add($"{temp}");
                                    counter = 0;
                                    temp = "";
                                }
                            }
                            lines.Add($"{temp}");

                            for (int i = 0; i < lines.Count; i++)
                            {
                                gfx.DrawString(lines[i],
                                    font,
                                    XBrushes.Black,
                                    new XRect(label.X, label.Y + i * lineHeight, label.Width, label.Height),
                                    XStringFormats.TopLeft);
                            }
                        }

                    }
                }
            }

            // сохраняем PDF
            doc.Save(path);
        }
        public BarcodeFormat ConvertElementTypesToBarcodeFormat(ElementTypes type)
        {
            switch (type)
            {
                case ElementTypes.Text:  return BarcodeFormat.ITF;
                case ElementTypes.Code128 : return BarcodeFormat.CODE_128;
                case ElementTypes.QrCode: return BarcodeFormat.QR_CODE;
                case ElementTypes.Matrix: return BarcodeFormat.DATA_MATRIX;
                default: return BarcodeFormat.ITF;

            }
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
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (DataContext as MainViewModel)?
                .ChangeSize(e.NewSize.Width, e.NewSize.Height, e.PreviousSize.Width, e.PreviousSize.Height);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.PrevWidthCanvas = FieldCanvas.ActualWidth;// Нужен для того чтобы обрабатывать событие изменения основного окна и маштабирования элементов в конструкторе
            viewModel.PrevHeightCanvas = FieldCanvas.ActualHeight;
        }
    }
}
