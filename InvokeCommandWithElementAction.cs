using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace barcode_gen
{
    public class InvokeCommandWithElementAction : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(InvokeCommandWithElementAction));

        public static readonly DependencyProperty ExtraElementProperty =
            DependencyProperty.Register(
                nameof(ExtraElement),
                typeof(FrameworkElement),
                typeof(InvokeCommandWithElementAction));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public FrameworkElement ExtraElement
        {
            get => (FrameworkElement)GetValue(ExtraElementProperty);
            set => SetValue(ExtraElementProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            var payload = new UiEventPayload
            {
                EventArgs = parameter as RoutedEventArgs,
                EventSource = AssociatedObject,
                ExtraElement = ExtraElement
            };

            if (Command?.CanExecute(payload) == true)
                Command.Execute(payload);
        }
    }
}
