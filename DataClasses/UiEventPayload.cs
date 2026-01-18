using System.Windows;

namespace barcode_gen
{
    public class UiEventPayload
    {
        public RoutedEventArgs EventArgs { get; set; }
        public FrameworkElement EventSource { get; set; }
        public FrameworkElement ExtraElement { get; set; }
    }
}
