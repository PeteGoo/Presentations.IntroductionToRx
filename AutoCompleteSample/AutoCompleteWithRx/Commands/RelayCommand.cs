using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace AutoCompleteWithRx.Commands {
    public class RelayCommand : ICommand {

        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand()
            : this(o => { }, null) {
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null) {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public IObservable<object> Executed {
            get { return executed; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            return canExecute == null || canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) {
            execute(parameter);
            executed.OnNext(parameter);
        }

        private readonly Subject<object> executed = new Subject<object>();
    }
}


