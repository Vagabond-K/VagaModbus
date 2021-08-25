using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagaModbusAnalyzer.Infrastructures
{
    public class ItemWidthCalculator : NotifyPropertyChangeObject
    {
        public double ItemWidth { get { return Get(350d); } set { Set(value); } }
    }
}
