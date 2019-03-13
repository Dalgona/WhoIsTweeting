using System;
using System.Windows.Input;

namespace Wit.UI.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executed;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action executed) : this(_ => executed(), (Predicate<object>)null) { }
        public RelayCommand(Action<object> executed) : this(executed, (Predicate<object>)null) { }
        public RelayCommand(Action executed, Func<bool> canExecute) : this(_ => executed(), _ => canExecute()) { }
        public RelayCommand(Action executed, Predicate<object> canExecute) : this(_ => executed(), canExecute) { }
        public RelayCommand(Action<object> executed, Func<bool> canExecute) : this(executed, _ => canExecute()) { }

        public RelayCommand(Action<object> executed, Predicate<object> canExecute)
        {
            _executed = executed ?? throw new ArgumentNullException(nameof(executed));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _executed?.Invoke(parameter);

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
