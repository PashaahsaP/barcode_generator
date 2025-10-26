using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BarcodeStandard;
using ClosedXML.Excel;
using SkiaSharp;

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
        }

        #region event methods
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string curDirectory = Directory.GetCurrentDirectory();
            string[] dataList = GetData(this);
            var sizes = getSizeOfLabel(this);
            var type = getLabelType(this);
            SaveBitmapToFile(dataList, curDirectory, sizes.Item1, sizes.Item2, type);//item1 is width item2 is height
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
        #endregion
        #region helper methods
        private BarcodeStandard.Type getLabelType(MainWindow mainWindow)
        {
            var type = ((ComboBoxItem)this.CBType.SelectedItem).Content;
            switch(type) 
            {
                case "Code 128": return BarcodeStandard.Type.Code128;
                case"EAN 13" : return BarcodeStandard.Type.Ean13;
                default: return BarcodeStandard.Type.Code128;
            }
        }
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
        public void SaveBitmapToFile(string[] dataList, string curDirectory, int labelWidth, int labelHeight, BarcodeStandard.Type type)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Штрихкоды");
            int rowIndex = 1;

            foreach (var data in dataList)
            {
                if (data != "")
                {
                    using (var ms = new MemoryStream())
                    {
                        var barcodeLib = new Barcode();
                        barcodeLib.IncludeLabel = true; // с подписью
                        var font = new SKFont();
                        font.Size = 12;
                        barcodeLib.LabelFont = font;
                        var splited = data.Split("*".ToCharArray());
                        if (splited.Length > 1)
                        {
                            barcodeLib.AlternateLabel = splited[1];
                        }
                        var barcodeData = splited.Length == 0 ? data : splited[0];
                        var bmp = barcodeLib.Encode(type, barcodeData, SKColors.Black, SKColors.White, labelWidth, labelHeight);
                        using (var imgData = bmp.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            imgData.SaveTo(ms);
                        }

                        ms.Position = 0;

                        ws.AddPicture(ms)
                          .MoveTo(ws.Cell(rowIndex + 1, 1))
                          .WithSize(labelWidth, labelHeight);

                        rowIndex += 7; // смещение для следующего блока//5 to small
                    }
                }
            }

            // Сохраняем файл
            wb.SaveAs(curDirectory + "\\barcodes.xlsx");
        }
        #endregion
    }
}
