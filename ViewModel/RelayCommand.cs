using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace barcode_gen
{
    internal class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }
        public RelayCommand( 
            Action execute, 
            Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;

        }
        
        


        public bool CanExecute(object parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
            => _execute();

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
