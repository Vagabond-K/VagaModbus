using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using VagabondK.Protocols;
using VagabondK.Protocols.Logging;
using VagabondK.Protocols.Modbus;
using VagaModbusAnalyzer.Data;
using VagaModbusAnalyzer.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VagaModbusAnalyzer.Views
{
    [View]
    public sealed partial class LogsView : UserControl
    {
        public LogsView()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            PART_Logger.SelectionChanged += PART_Logger_SelectionChanged;
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Logs logs)
            {
                logs.PropertyChanged += LogsPropertyChanged;
                if (!logs.Channels.Contains(logs.SelectedChannel))
                {
                    logs.SelectedChannel = logs.Channels.FirstOrDefault();
                }
            }

            if (PART_Logger.SelectedItem != null)
                PART_Logger.ScrollIntoView(PART_Logger.SelectedItem, ScrollIntoViewAlignment.Default);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Logs logs)
                logs.PropertyChanged -= LogsPropertyChanged;

            if (logger != null)
                (logger.ItemsSource as INotifyCollectionChanged).CollectionChanged -= LogsViewCollectionChanged;
        }

        private void LogsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Logs logs && e.PropertyName == nameof(logs.IsAutoScroll) && logs.IsAutoScroll)
            {
                var message = logs.SelectedChannel?.Logger?.ItemsSource?.LastOrDefault();
                if (message != null)
                {
                    PART_Logger.SelectedItem = message;
                    PART_Logger.ScrollIntoView(PART_Logger.SelectedItem, ScrollIntoViewAlignment.Default);
                }
            }
        }

        private ModbusLogger logger = null;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };

        private void LogsDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (logger != null)
                (logger.ItemsSource as INotifyCollectionChanged).CollectionChanged -= LogsViewCollectionChanged;

            if (PART_Logger.DataContext is ModbusLogger messageLogs)
            {
                logger = messageLogs;
                if (DataContext is Logs logs && logs.IsAutoScroll)
                {
                    var message = messageLogs.ItemsSource.LastOrDefault();
                    if (message != null)
                    {
                        PART_Logger.SelectedItem = message;
                        PART_Logger.ScrollIntoView(PART_Logger.SelectedItem, ScrollIntoViewAlignment.Default);
                    }
                }
                (messageLogs.ItemsSource as INotifyCollectionChanged).CollectionChanged += LogsViewCollectionChanged;
            }
        }

        private void LogsViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataContext is Logs logs && e.NewItems != null && (logs.IsAutoScroll || PART_Logger.SelectedItem == null))
            {
                var message = e.NewItems.Cast<ChannelLog>().LastOrDefault();
                if (message != null)
                {
                    PART_Logger.SelectedItem = message;
                }
            }
        }

        private void PART_Logger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_Logger.SelectedItem != null)
            {
                if (dispatcherTimer.IsEnabled)
                    dispatcherTimer.Stop();
                dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            dispatcherTimer.Stop();
            if (PART_Logger.SelectedItem != null)
            {
                PART_Logger.ScrollIntoView(PART_Logger.SelectedItem, ScrollIntoViewAlignment.Default);
            }
        }



        private void DetailViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var logsDetailRow = Application.Current.Resources["LogsDetailRow"] as RowDefinition;
            logsDetailRow.Height = new GridLength(e.NewSize.Height);
        }
    }

    //public class MessageRowStyleSelector : StyleSelector
    //{
    //    public Style DefaultStyle { get; set; }
    //    public Style RequestStyle { get; set; }
    //    public Style ResponseStyle { get; set; }
    //    public Style ExceptionStyle { get; set; }
    //    public Style ErrorStyle { get; set; }

    //    protected override Style SelectStyleCore(object item, DependencyObject container)
    //    {
    //        if (item is ChannelErrorLog || item is UnrecognizedErrorLog)
    //            return ErrorStyle ?? DefaultStyle ?? base.SelectStyleCore(item, container);
    //        else if (item is ModbusExceptionLog)
    //            return ExceptionStyle ?? DefaultStyle ?? base.SelectStyleCore(item, container);
    //        else if (item is ChannelRequestLog)
    //            return RequestStyle ?? DefaultStyle ?? base.SelectStyleCore(item, container);
    //        else if (item is ChannelResponseLog)
    //            return ResponseStyle ?? DefaultStyle ?? base.SelectStyleCore(item, container);

    //        return DefaultStyle ?? base.SelectStyleCore(item, container);
    //    }
    //}


    //public class MessageDirectionIconConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value == null) return null;

    //        if (value is ChannelErrorLog || value is UnrecognizedErrorLog)
    //            return 0xEA39;
    //        else if (value is ModbusExceptionLog)
    //            return 0xE896;
    //        else if (value is ChannelOpenEventLog)
    //            return 0xE8CE;
    //        else if (value is ChannelCloseEventLog)
    //            return 0xE8CD;
    //        else if (value is ChannelRequestLog)
    //            return 0xE898;
    //        else if (value is ChannelResponseLog)
    //            return 0xE896;
    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class MessageLogColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value == null) return null;

    //        if (value is ChannelErrorLog || value is UnrecognizedErrorLog)
    //            return Color.FromArgb(0x20, 0xFF, 0x00, 0x00);
    //        else if (value is ModbusExceptionLog)
    //            return Color.FromArgb(0x20, 0xFF, 0x80, 0x00);
    //        else if (value is ChannelRequestLog)
    //            return Colors.Transparent;
    //        else if (value is ChannelResponseLog)
    //            return Color.FromArgb(0x20, 0x00, 0x80, 0x00);
    //        return Colors.Transparent;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class MessageNameConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value == null) return null;

    //        if (value is ChannelErrorLog || value is UnrecognizedErrorLog)
    //            return "ERROR";
    //        else if (value is ModbusExceptionLog)
    //            return "RECV";
    //        else if (value is ChannelOpenEventLog)
    //            return "OPEN";
    //        else if (value is ChannelCloseEventLog)
    //            return "CLOSE";
    //        else if (value is ChannelRequestLog)
    //            return "SEND";
    //        else if (value is ChannelResponseLog)
    //            return "RECV";

    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    //public class TimeStampStringConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value is DateTime dateTime)
    //        {
    //            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    //        }
    //        return "N/A";
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class BytesToHexStringConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value is IEnumerable<byte> bytes)
    //        {
    //            StringBuilder stringBuilder = new StringBuilder();
    //            foreach (var byteValue in bytes)
    //            {
    //                if (stringBuilder.Length > 0)
    //                    stringBuilder.Append(' ');
    //                stringBuilder.Append(byteValue.ToString("X2"));
    //            }
    //            return stringBuilder.ToString();
    //        }

    //        return null;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class LogsViewRowContentTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate ErrorCodeTemplate { get; set; }
    //    public DataTemplate ErrorMessageTemplate { get; set; }
    //    public DataTemplate UnrecognizedErrorTemplate { get; set; }
    //    public DataTemplate RawAndExceptionCodeTemplate { get; set; }
    //    public DataTemplate RawTemplate { get; set; }
    //    public DataTemplate ChannelOpenTemplate { get; set; }
    //    public DataTemplate ChannelCloseTemplate { get; set; }

    //    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    //    {
    //        if (item is ChannelErrorLog channelErrorLog)
    //            if (channelErrorLog.Exception is ErrorCodeException<ModbusCommErrorCode>)
    //                return ErrorCodeTemplate ?? base.SelectTemplateCore(item, container);
    //            else
    //                return ErrorMessageTemplate ?? base.SelectTemplateCore(item, container);
    //        else if (item is UnrecognizedErrorLog)
    //            return UnrecognizedErrorTemplate ?? base.SelectTemplateCore(item, container);
    //        else if (item is ModbusExceptionLog)
    //            return RawAndExceptionCodeTemplate ?? base.SelectTemplateCore(item, container);
    //        else if (item is ChannelMessageLog messageLog && messageLog.RawMessage != null)
    //            return RawTemplate ?? base.SelectTemplateCore(item, container);
    //        else if (item is ChannelOpenEventLog)
    //            return ChannelOpenTemplate ?? base.SelectTemplateCore(item, container);
    //        else if (item is ChannelCloseEventLog)
    //            return ChannelCloseTemplate ?? base.SelectTemplateCore(item, container);

    //        return ErrorCodeTemplate ?? base.SelectTemplateCore(item, container);
    //    }
    //}




    //public class MessageToValuesConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        switch (value?.GetType()?.Name)
    //        {
    //            case nameof(ModbusReadRegisterResponse):
    //                if (value is ModbusReadRegisterResponse readRegisterResponse)
    //                {
    //                    return Enumerable.Range(0, readRegisterResponse.Request.Length).Select(i =>
    //                    {
    //                        var result = new ModbusRegister { Address = (ushort)(readRegisterResponse.Request.Address + i) };
    //                        result.SetRegisterValue(readRegisterResponse.Bytes, i * 2);
    //                        return result;
    //                    }).ToArray();
    //                }
    //                break;
    //            case nameof(ModbusReadBooleanResponse):
    //                if (value is ModbusReadBooleanResponse readBooleanResponse)
    //                {
    //                    return Enumerable.Range(0, readBooleanResponse.Request.Length).Select(i =>
    //                    {
    //                        var result = new ModbusBoolean
    //                        {
    //                            Address = (ushort)(readBooleanResponse.Request.Address + i),
    //                            Value = readBooleanResponse.Values[i]
    //                        };
    //                        return result;
    //                    }).ToArray();
    //                }
    //                break;
    //            case nameof(ModbusWriteResponse):
    //                if (value is ModbusWriteResponse writeResponse)
    //                {
    //                    switch (writeResponse.Request.ObjectType)
    //                    {
    //                        case ModbusObjectType.InputRegister:
    //                        case ModbusObjectType.HoldingRegister:
    //                            if (writeResponse.Request is ModbusWriteHoldingRegisterRequest writeHoldingRegister)
    //                                return GetWriteHoldingRegisterRequestValues(writeHoldingRegister);
    //                            break;
    //                        case ModbusObjectType.DiscreteInput:
    //                        case ModbusObjectType.Coil:
    //                            if (writeResponse.Request is ModbusWriteCoilRequest writeCoil)
    //                                return GetWriteCoilRequestValues(writeCoil);
    //                            break;
    //                        default:
    //                            return null;
    //                    }
    //                }
    //                break;
    //            case nameof(ModbusWriteCoilRequest):
    //                if (value is ModbusWriteCoilRequest writeCoilRequest)
    //                    return GetWriteCoilRequestValues(writeCoilRequest);
    //                break;
    //            case nameof(ModbusWriteHoldingRegisterRequest):
    //                if (value is ModbusWriteHoldingRegisterRequest writeHoldingRegisterRequest)
    //                    return GetWriteHoldingRegisterRequestValues(writeHoldingRegisterRequest);
    //                break;
    //        }
    //        return null;
    //    }

    //    private object GetWriteHoldingRegisterRequestValues(ModbusWriteHoldingRegisterRequest writeHoldingRegisterRequest)
    //    {
    //        var bytes = writeHoldingRegisterRequest.Bytes.ToArray();
    //        if (writeHoldingRegisterRequest.Bytes.Count() > 2)
    //        {
    //            return Enumerable.Range(0, writeHoldingRegisterRequest.Length).Select(i =>
    //            {
    //                var result = new ModbusRegister { Address = (ushort)(writeHoldingRegisterRequest.Address + i) };
    //                result.SetRegisterValue(bytes, i * 2);
    //                return result;
    //            }).ToArray();
    //        }
    //        else
    //        {
    //            var result = new ModbusRegister { Address = writeHoldingRegisterRequest.Address };
    //            result.SetRegisterValue(bytes, 0);
    //            return result;
    //        }
    //    }

    //    private object GetWriteCoilRequestValues(ModbusWriteCoilRequest writeCoilRequest)
    //    {
    //        if (writeCoilRequest.Values.Count() > 1)
    //        {
    //            bool[] values = writeCoilRequest.Values.ToArray();
    //            return Enumerable.Range(0, writeCoilRequest.Length).Select(i =>
    //            {
    //                return new ModbusBoolean
    //                {
    //                    Address = (ushort)(writeCoilRequest.Address + i),
    //                    Value = values[i]
    //                };
    //            }).ToArray();
    //        }
    //        else
    //        {
    //            return new ModbusBoolean
    //            {
    //                Address = writeCoilRequest.Address,
    //                Value = writeCoilRequest.SingleBooleanValue
    //            };
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class MessageToValuesTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate SingleRegisterDataTemplate { get; set; }
    //    public DataTemplate SingleBitDataTemplate { get; set; }
    //    public DataTemplate MultiRegisterDataTemplate { get; set; }
    //    public DataTemplate MultiBitDataTemplate { get; set; }
    //    public DataTemplate EmptyTemplate { get; set; }

    //    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    //    {
    //        if (item is ModbusRegister[])
    //            return MultiRegisterDataTemplate;
    //        else if (item is ModbusBoolean[])
    //            return MultiBitDataTemplate;
    //        else if (item is ModbusRegister)
    //            return SingleRegisterDataTemplate;
    //        else if (item is ModbusBoolean)
    //            return SingleBitDataTemplate;
    //        return EmptyTemplate;
    //    }
    //}

}
