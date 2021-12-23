using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VagabondK.Protocols.Channels;
using VagabondK.Protocols.Logging;
using VagabondK.Protocols.Modbus;
using VagabondK.Protocols.Modbus.Serialization;
using VagaModbusAnalyzer.ChannelSetting;
using VagaModbusAnalyzer.Data;

namespace VagaModbusAnalyzer
{
    public class ModbusChannel : NotifyPropertyChangeObject, IDisposable
    {
        public ModbusChannel()
        {
        }

        ~ModbusChannel()
        {
            Dispose();
        }

        public void Dispose()
        {
            modbusMaster?.Dispose();
        }

        public ModbusChannel Copy() =>
            new ModbusChannel
            {
                Name = Name,
                ModbusType = ModbusType,
                ChannelSetting = ChannelSetting.Copy(),
                ScanInterval = ScanInterval,
            };

        private readonly ModbusMaster modbusMaster = new ModbusMaster();
        private readonly Dictionary<ChannelType, IChannelSetting> tempChannelSettings = new Dictionary<ChannelType, IChannelSetting>();
        private readonly object taskRunStopLock = new object();
        private readonly EventWaitHandle scanOptionEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private CancellationTokenSource scanCancellationTokenSource = null;
        private Task scanTask = null;


        public string Name { get => Get<string>(); set => Set(value); }
        public ModbusType ModbusType { get => Get(ModbusType.TCP); set => Set(value); }

        [JsonIgnore]
        public ChannelType ChannelType
        {
            get => Get(() => ChannelSetting?.ChannelType ?? ChannelType.None);
            set => Set(value);
        }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public IChannelSetting ChannelSetting
        {
            get => Get<IChannelSetting>(() =>
            {
                var result = new TcpClientChannelSetting();
                tempChannelSettings[result.ChannelType] = result;
                return result;
            });
            set => Set(value);
        }

        public int ScanInterval { get => Get(1000); set => Set(value); }

        public IList<ModbusScan> ModbusScans
        {
            get => Get(() =>
            {
                var result = new ObservableCollection<ModbusScan>();
                result.CollectionChanged += ModbusScansCollectionChanged;
                return result;
            }); private set => Set(value);
        }

        private void ModbusScansCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ModbusScan oldItem in e.OldItems)
                    oldItem.PropertyChanged -= ModbusScanPropertyChanged;
            if (e.NewItems != null)
                foreach (ModbusScan newItem in e.NewItems)
                    newItem.PropertyChanged += ModbusScanPropertyChanged;

            scanOptionEventWaitHandle.Set();
        }

        private void ModbusScanPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModbusScan modbusScan && e.PropertyName == nameof(modbusScan.RunScan) && modbusScan.RunScan)
            {
                scanOptionEventWaitHandle.Set();
            }
        }


        [JsonIgnore]
        public ModbusLogger Logger { get => Get(() => new ModbusLogger(this)); }

        public void StartScan(IChannelFactory channelFactory, ICrossThreadDispatcher dispatcher)
        {
            lock (taskRunStopLock)
            {
                if (scanCancellationTokenSource != null) return;
                scanCancellationTokenSource = new CancellationTokenSource();

                Logger.dispatcher = dispatcher;

                scanTask = Task.Run(() =>
                {
                    switch (ModbusType)
                    {
                        case ModbusType.RTU:
                            modbusMaster.Serializer = new ModbusRtuSerializer();
                            break;
                        case ModbusType.TCP:
                            modbusMaster.Serializer = new ModbusTcpSerializer();
                            break;
                        case ModbusType.ASCII:
                            modbusMaster.Serializer = new ModbusAsciiSerializer();
                            break;
                    }


                    if (modbusMaster.Channel != null)
                    {
                        (modbusMaster.Channel as ChannelProvider)?.Stop();
                        modbusMaster.Channel.Logger = null;
                    }

                    using (var channel = channelFactory.CreateChannel(ChannelSetting))
                    {
                        channel.Logger = Logger;
                        modbusMaster.Channel = channel;
                        (channel as ChannelProvider)?.Start();

                        while (scanCancellationTokenSource?.IsCancellationRequested == false)
                        {
                            var stopwatch = Stopwatch.StartNew();
                            HashSet<ModbusScan> completedScans = new HashSet<ModbusScan>();
                            for (int i = 0; i < ModbusScans.Count; i++)
                            {
                                ModbusScan modbusScan = null;
                                lock (ModbusScans)
                                {
                                    if (i < ModbusScans.Count)
                                        modbusScan = ModbusScans[i];
                                }

                                if (modbusScan != null && modbusScan.RunScan && !completedScans.Contains(modbusScan))
                                {
                                    ModbusResponse response = null;
                                    Exception error = null;
                                    lock (modbusScan)
                                    {
                                        lock (modbusMaster)
                                        {
                                            var request = modbusScan.Request;
                                            request.TransactionID = null;
                                            try
                                            {
                                                if (((modbusMaster.Channel as Channel) ?? (modbusMaster.Channel as ChannelProvider)?.PrimaryChannel) != null)
                                                    response = modbusMaster.Request(request, modbusScan.ResponseTimeout);
                                            }
                                            catch (Exception ex)
                                            {
                                                error = ex;
                                            }
                                        }
                                        completedScans.Add(modbusScan);


                                        dispatcher?.Invoke(() =>
                                        {
                                            if (response != null)
                                            {
                                                switch (modbusScan.ObjectType)
                                                {
                                                    case ModbusObjectType.InputRegister:
                                                    case ModbusObjectType.HoldingRegister:
                                                        var bytes = (response as ModbusReadRegisterResponse)?.Bytes;
                                                        if (bytes != null)
                                                        {
                                                            modbusScan.LastUpdated = DateTime.Now;
                                                            modbusScan.Status = "Text_CommNormal/Text";
                                                            int index = 0;
                                                            foreach (var data in modbusScan.Data.Cast<ModbusRegister>())
                                                            {
                                                                data.SetRegisterValue(bytes, index * 2);
                                                                index++;
                                                            }
                                                        }
                                                        break;
                                                    case ModbusObjectType.DiscreteInput:
                                                    case ModbusObjectType.Coil:
                                                        var values = (response as ModbusReadBooleanResponse)?.Values;
                                                        if (values != null)
                                                        {
                                                            modbusScan.LastUpdated = DateTime.Now;
                                                            modbusScan.Status = "Text_CommNormal/Text";
                                                            int index = 0;
                                                            foreach (var data in modbusScan.Data.Cast<ModbusBoolean>())
                                                            {
                                                                data.Value = values[index];
                                                                index++;
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                            else if (error != null)
                                            {
                                                if (error.GetType() == typeof(Exception))
                                                {
                                                    error = new MessageException(error.Message);
                                                    channel.Logger.Log(new ChannelErrorLog(channel, error));
                                                }

                                                modbusScan.Status = error;
                                            }
                                        });
                                    }
                                }

                                if (scanCancellationTokenSource?.IsCancellationRequested != false)
                                    break;
                            }

                            scanOptionEventWaitHandle.WaitOne((int)Math.Max(0, ScanInterval - stopwatch.ElapsedMilliseconds - 1));
                        }
                    }
                }, scanCancellationTokenSource.Token);
            }
        }

        public class MessageException : Exception
        {
            public MessageException(string exception) : base(exception) { }
        }

        public void StopScan()
        {
            lock (taskRunStopLock)
            {
                modbusMaster?.Channel?.Dispose();
                scanCancellationTokenSource?.Cancel();
                scanCancellationTokenSource = null;
                scanOptionEventWaitHandle.Set();
                scanTask.Wait();
                scanCancellationTokenSource = null;
            }
        }



        protected override bool OnPropertyChanging(QueryPropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChannelType):
                    tempChannelSettings[ChannelType] = ChannelSetting;
                    break;
            }

            return base.OnPropertyChanging(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(ChannelType):
                    OnChannelTypeChanged();
                    break;
            }
        }


        private void OnChannelTypeChanged()
        {
            tempChannelSettings.TryGetValue(ChannelType, out var channelType);
            if (channelType != null)
            {
                ChannelSetting = channelType;
            }
            else
            {
                switch (ChannelType)
                {
                    case ChannelType.TcpClient:
                        ChannelSetting = new TcpClientChannelSetting();
                        break;
                    case ChannelType.TcpServer:
                        ChannelSetting = new TcpServerChannelSetting();
                        break;
                    case ChannelType.UdpSocket:
                        ChannelSetting = new UdpSocketChannelSetting();
                        break;
                    case ChannelType.SerialPort:
                        ChannelSetting = new SerialPortChannelSetting();
                        break;
                    case ChannelType.None:
                        ChannelSetting = null;
                        break;
                }
                if (ChannelSetting != null)
                    tempChannelSettings[ChannelSetting.ChannelType] = ChannelSetting;
            }
        }

    }
}
