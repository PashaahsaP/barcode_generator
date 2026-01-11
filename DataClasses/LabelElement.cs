using ZXing;

namespace barcode_gen
{
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
}
