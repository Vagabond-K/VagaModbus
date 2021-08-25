using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagaModbusAnalyzer.Infrastructures;
using Windows.UI;

namespace VagaModbusAnalyzer.ViewModels
{
    [ViewModel(DefaultViewType = typeof(Views.ThemeColorPickerPopupView))]
    public class ThemeColorPickerPopup : NotifyPropertyChangeObject
    {
        public Color Color { get => Get(Colors.Red); set => Set(value); }
        public bool IsCompatibleColor { get => Get(() => IsCompatibleColorCore(Color)); set => Set(value); }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(Color):
                    IsCompatibleColor = IsCompatibleColorCore(Color);
                    break;
            }
        }

        private bool IsCompatibleColorCore(Color color)
        {
            ColorHSV hsv = color.ToHSV();
            return hsv.V >= 25 && Math.Log(hsv.V / 5 - 4) * 52 + 7 >= hsv.S && hsv.V * 2 - 150 <= hsv.S;
        }
    }
}
