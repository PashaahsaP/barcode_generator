using barcode_gen.Enum;
using barcode_gen.Fonts.Resolvers;
using DocumentFormat.OpenXml.Drawing.Charts;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.OneD;
using ZXing.QrCode;
using static barcode_gen.MainWindow;
using static barcode_gen.ViewModel.BlockViewModel;
using Canvas = System.Windows.Controls.Canvas;

namespace barcode_gen
{

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
    


    public partial class MainWindow : Window
    {
        #region Properties
        public Config config { get; set; }
        public MainViewModel viewModel;
        #endregion
        #region ctor
        public MainWindow()
        {
            InitializeComponent();
            GlobalFontSettings.FontResolver = new CustomFontResolver();
            var canvas = FieldCanvas;
            var vm = new MainViewModel(FieldCanvas, ConfigPopup);
            DataContext = vm;
            viewModel = vm;
            config = LoadConfig();
        }
        #endregion
        #region I/O region
        public static Config LoadConfig()
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
            return JsonSerializer.Deserialize<Config>(text);
        }
        public void SaveLabelsToPdf(List<List<RotatedLabelElement>> labels, string path)
        {
            PdfDocument doc = new PdfDocument();
            foreach (var bunchOfLabels in labels)
            {
                PdfPage page = doc.AddPage();
                (int width, int height) sizes = getSizeOfLabelForPrint(viewModel.RightViewModel.SelectedMode);
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
                            var widthOnOneSign = 2.5;//эксперементальный выбор
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
        #endregion
        #region event methods
        private void Button_Click(object sender, RoutedEventArgs e)//refac 12.01
        {
            GetDataFromLeftSideFields();
        }
       
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (DataContext as MainViewModel)?
                .ChangeSize();

            foreach (var item in viewModel.SharedState.Blocks)
            {
                item.mainHeight = e.NewSize.Height;
                item.mainWidth = e.NewSize.Width;
            }
        }//refac 12.01
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.PrevWidthCanvas = FieldCanvas.ActualWidth;// Нужен для того чтобы обрабатывать событие изменения основного окна и маштабирования элементов в конструкторе
            viewModel.PrevHeightCanvas = FieldCanvas.ActualHeight;
        }//refac 12.01
        #endregion
        #region helper methods
        private (int weigth, int height) getSizeOfLabelForPrint(Mode mode)
        {
            switch (mode)
            {
                case Mode.Small: return (60, 40);
                case Mode.Large: return (100, 70);
                default: return (0, 0);
            }

        }//refac 12.01
        public BarcodeFormat ConvertElementTypesToBarcodeFormat(ElementTypes type)
        {
            switch (type)
            {
                case ElementTypes.Text: return BarcodeFormat.ITF;
                case ElementTypes.Code128: return BarcodeFormat.CODE_128;
                case ElementTypes.QrCode: return BarcodeFormat.QR_CODE;
                case ElementTypes.Matrix: return BarcodeFormat.DATA_MATRIX;
                default: return BarcodeFormat.ITF;

            }
        }//refac 12.01
        #endregion

        public void GetDataFromLeftSideFields() //refac 12.01
        {
            var contents = viewModel.SharedState.Blocks// If isn't data in left
                .Select((block) => block.ContentData).ToList();
            if (contents.Count() == 0)
                return;


            (int width, int height) sizes = getSizeOfLabelForPrint(viewModel.RightViewModel.SelectedMode);
            var proportionWidth = sizes.width / viewModel.SharedState.WidthCanvas;
            var proportionHeight = sizes.height / viewModel.SharedState.HeightCanvas;
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
            
            if (data.Count == 0)
                return;
            SaveLabelsToPdf(data, "lable.pdf");

        }
        private List<RotatedLabelElement> CreateLabels(double proportionWidth, double proportionHeight, Queue<string> values)
        {
            var labels = new List<RotatedLabelElement>();
            foreach (var block in viewModel.SharedState.Blocks)
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
                    rt = existing; 
                }

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
        }//refac 12.01





    }
}
