using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace barcode_gen.ViewModel
{
    internal class TicketCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<UiEventPayload> _executePressing;
        private readonly Func<object, bool> _canExecute;

  
        public TicketCommand(Action<UiEventPayload> execute)
        {
            _executePressing = execute;
        }





        public bool CanExecute(object parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
        {
            var e = parameter as UiEventPayload;
            _executePressing((UiEventPayload)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
