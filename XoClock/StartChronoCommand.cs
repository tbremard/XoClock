using System;
using System.Windows.Input;
using System.Diagnostics;

namespace XoClock
{
    internal class StartChronoCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            Debug.Print(this.GetType().Name);
        }
    }
}