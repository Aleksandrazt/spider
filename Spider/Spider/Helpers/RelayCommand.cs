using System.Windows.Input;

namespace Spider.Helpers
{
    /// <summary>
    /// Реализация ICommand для привязки команд в MVVM
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;


        /// <summary>
        /// Конструктор команды
        /// </summary>
        /// <param name="execute">Действие для выполнения</param>
        /// <param name="canExecute">Условие возможности выполнения (опционально)</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Проверка возможности выполнения команды
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Событие изменения возможности выполнения команды
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
