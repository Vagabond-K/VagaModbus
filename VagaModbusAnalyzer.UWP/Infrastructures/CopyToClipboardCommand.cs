using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class CopyToClipboardCommand : ICommand
    {
        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(parameter.ToString());
            Clipboard.SetContent(dataPackage);
        }
    }
}
