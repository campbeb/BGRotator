using System;
using System.Windows;
using System.Windows.Input;

namespace BGRotator.Commands
{
    public class ActivateCommand : ICommand
    {
        public void Execute(object parameter)
        {
            MainWindow _mainWindow = (Application.Current as App).GetMainWindow;

            if (_mainWindow == null)
            {
                return;
            }

            if (_mainWindow.IsVisible)
                return;

            _mainWindow.DisplayWindow();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
