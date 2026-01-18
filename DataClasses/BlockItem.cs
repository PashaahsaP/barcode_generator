using barcode_gen.Enum;
using barcode_gen.ViewModel;
using System.Windows;

namespace barcode_gen
{
    public class BlockItem
    {
        public int BlockTop { get; set; }
        public int BlockLeft { get; set; }
        public int LinesCount { get; set; }
        public string TextBoxState { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public double CX { get; set; }
        public double CY { get; set; }
        public double H { get; set; }
        public double W { get; set; }
        public double MainWidth { get; set; }
        public double MainHeight { get; set; }
        public double Angel { get; set; }
        public Visibility Visibility { get; set; }
        public ElementTypes SelectedType { get; set; }
        public BlockItem(BlockViewModel vm)
        {
            BlockTop = vm.BlockTop;
            BlockLeft = vm.BlockLeft;
            LinesCount = vm.LinesCount;
            TextBoxState = vm.TextBoxState;
            Content = vm.Content;
            Title = vm.Title;
            Visibility = vm.Visibility;
            CX = vm.CX;
            CY = vm.CY;
            H = vm.H;
            W = vm.W;
            MainHeight = vm.mainHeight;
            MainWidth = vm.mainWidth;
            Angel = vm.Angel;
            SelectedType = ElementTypes.Text;
        }
        public BlockItem()
        {

        }
        public BlockItem(int blockTop, int blockLeft, int linesCount, string textBoxState, string content, string title, double x, double y, double h, double w, double mainWidth, double mainHeight, double angel, Visibility visibility, ElementTypes selectedType)
        {
            BlockTop = blockTop;
            BlockLeft = blockLeft;
            LinesCount = linesCount;
            TextBoxState = textBoxState;
            Content = content;
            Title = title;
            CX = x;
            CY = y;
            H = h;
            W = w;
            this.MainWidth = mainWidth;
            this.MainHeight = mainHeight;
            Angel = angel;
            Visibility = visibility;
            SelectedType = selectedType;
        }
    }
}
