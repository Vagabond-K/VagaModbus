﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VagabondK.Protocols.Logging;
using Windows.UI.Xaml.Data;

namespace VagaModbusAnalyzer.Converters
{
    public class ChannelLogToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChannelMessageLog messageLog)
            {
                return messageLog.Message;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
