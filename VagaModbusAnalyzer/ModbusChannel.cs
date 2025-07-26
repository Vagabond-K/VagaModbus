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
        private readonly object channelLock = new object();
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
                result.CollectionChanged += OnModbusScansCollectionChanged;
                return result;
            });
        }

        public IList<ModbusWriter> ModbusWriters
        {
            get => Get(() =>
            {
                var result = new ObservableCollection<ModbusWriter>();
                result.CollectionChanged += OnModbusWritersCollectionChanged;
                return result;
            });
        }

        private void OnModbusScansCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ModbusScan item in e.OldItems)
                    item.PropertyChanged -= OnModbusScanPropertyChanged;
            if (e.NewItems != null)
                foreach (ModbusScan item in e.NewItems)
                    item.PropertyChanged += OnModbusScanPropertyChanged;

            scanOptionEventWaitHandle.Set();
        }

        private void OnModbusScanPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModbusScan modbusScan && e.PropertyName == nameof(modbusScan.RunScan) && modbusScan.RunScan)
            {
                scanOptionEventWaitHandle.Set();
            }
        }

        private void OnModbusWritersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ModbusWriter item in e.OldItems)
                    item.Channel = null;
            if (e.NewItems != null)
                foreach (ModbusWriter item in e.NewItems)
                    item.Channel = this;
        }


        [JsonIgnore]
        public ModbusLogger Logger { get => Get(() => new ModbusLogger(this)); }

        public void StartScan(IChannelFactory channelFactory, ICrossThreadDispatcher dispatcher)
        {
            lock (channelLock)
            {
                if (scanCancellationTokenSource != null) return;
                scanCancellationTokenSource = new CancellationTokenSource();

                Logger.dispatcher = dispatcher;

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

                var channel = channelFactory.CreateChannel(ChannelSetting);
                if (channel != null)
                {
                    channel.Logger = Logger;
                    modbusMaster.Channel = channel;
                    (channel as ChannelProvider)?.Start();

                    scanTask = Task.Run(() =>
                    {
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

                                if (modbusScan != null && !completedScans.Contains(modbusScan))
                                {
                                    ModbusRequest request = null;
                                    byte slaveAddress = 0;
                                    lock (modbusScan)
                                    {
                                        if (modbusScan.RunScan)
                                        {
                                            request = modbusScan.Request;
                                            request.TransactionID = null;
                                            if (!modbusScan.DetectSlaveAddress)
                                                slaveAddress = modbusScan.SlaveAddress;
                                            else
                                            {
                                                var start = modbusScan.DetectSlaveAddrStart;
                                                var end = modbusScan.DetectSlaveAddrEnd;
                                                var current = modbusScan.CurrentSlaveAddress;
                                                if (start < end)
                                                {
                                                    if (current == null || current >= end)
                                                        current = start;
                                                    else
                                                        current++;
                                                }
                                                else if (start > end)
                                                {
                                                    if (current == null || current <= end)
                                                        current = start;
                                                    else
                                                        current--;
                                                }
                                                else
                                                    current = start;
                                                current = (byte)(current % 256);
                                                slaveAddress = current.Value;
                                            }
                                        }
                                    }

                                    if (request != null)
                                    {
                                        request.SlaveAddress = slaveAddress;
                                        dispatcher?.Invoke(() =>
                                        {
                                            lock (modbusScan)
                                                modbusScan.CurrentSlaveAddress = modbusScan.SlaveAddress = slaveAddress;
                                        });

                                        ModbusResponse response = null;
                                        Exception error = null;
                                        lock (modbusMaster)
                                        {
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
                                                lock (modbusScan)
                                                    if (modbusScan.DetectSlaveAddress)
                                                        modbusScan.DetectSlaveAddress = false;

                                                switch (modbusScan.ObjectType)
                                                {
                                                    case ModbusObjectType.InputRegister:
                                                    case ModbusObjectType.HoldingRegister:
                                                        var bytes = (response as ModbusReadWordResponse)?.Bytes;
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
                                                        var values = (response as ModbusReadBitResponse)?.Values;
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
                                    else if (modbusScan.Status != null)
                                    {
                                        dispatcher?.Invoke(() =>
                                        {
                                            modbusScan.Status = null;
                                        });
                                    }
                                }

                                if (scanCancellationTokenSource?.IsCancellationRequested != false)
                                    break;
                            }

                            scanOptionEventWaitHandle.WaitOne((int)Math.Max(0, ScanInterval - stopwatch.ElapsedMilliseconds - 1));
                        }
                    }, scanCancellationTokenSource.Token);
                }
            }
        }

        public void StopScan()
        {
            lock (channelLock)
            {
                scanCancellationTokenSource?.Cancel();
                scanCancellationTokenSource = null;
                scanOptionEventWaitHandle.Set();
                if (modbusMaster.Channel != null)
                {
                    (modbusMaster.Channel as ChannelProvider)?.Stop();
                    modbusMaster.Channel.Logger = null;
                    modbusMaster.Channel.Dispose();
                }
                scanTask.Wait();
                scanCancellationTokenSource = null;
            }
        }

        public async Task Write(ModbusWriter writer, IStringLocalizer stringLocalizer)
        {
            ModbusResponse response = null;
            IChannel channel = null;

            try
            {
                writer.IsBusy = true;
                writer.Status = null;

                await Task.Run(() =>
                {
                    lock (channelLock)
                    {
                        channel = modbusMaster.Channel;
                        if (channel != null)
                        {
                            lock (modbusMaster)
                            {
                                response = modbusMaster.Request(writer.Request, writer.ResponseTimeout);
                            }
                        }
                    }
                });

                writer.IsBusy = false;
                writer.Status = $"{stringLocalizer["Text_Succeed/Text"]} ({DateTime.Now:yyyy-MM-dd HH:mm:ss.fff})";
            }
            catch (Exception error)
            {
                writer.IsBusy = false;
                error = new MessageException($"{error.Message} ({DateTime.Now:yyyy-MM-dd HH:mm:ss.fff})");
                if (channel != null && error.GetType() == typeof(Exception))
                {
                    channel.Logger.Log(new ChannelErrorLog(channel, error));
                }
                writer.Status = error;
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
