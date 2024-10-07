using System.Windows.Input;

namespace CarrotMessenger.Wpf
{
    public class SimpleCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action<object> _action;

        public SimpleCommand(Action<object> action)
        {
            _action = action;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}