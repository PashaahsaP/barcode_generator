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
        public double X { get; set; }
        public double Y { get; set; }
        public double H { get; set; }
        public double W { get; set; }
        public double mainWidth { get; set; }
        public double mainHeight { get; set; }
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
            X = vm.X;
            Y = vm.Y;
            H = vm.H;
            W = vm.W;
            mainHeight = vm.mainHeight;
            mainWidth = vm.mainWidth;
            Angel = vm.Angel;
            SelectedType = vm.SelectedType;
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
            X = x;
            Y = y;
            H = h;
            W = w;
            this.mainWidth = mainWidth;
            this.mainHeight = mainHeight;
            Angel = angel;
            Visibility = visibility;
            SelectedType = selectedType;
        }
    }
}
