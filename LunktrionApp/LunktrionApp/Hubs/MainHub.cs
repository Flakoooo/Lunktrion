using LunktrionShared.Models.Entities;
using LunktrionShared.Models.Interfaces;
using LunktrionShared.Models.Requests;
using LunktrionShared.Models.Responses;
using LunktrionShared.Models.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace LunktrionApp.Hubs
{
    public class MainHub
    {
        private readonly HubConnection _connection;

        public event Action<bool>? ConnectionStatusChanged;

        public event Action<string>? NotificationReceived;
        public event Action<string>? ErrorReceived;

        public event Action<DeviceIdentity>? DeviceConnected;
        public event Action<string>? DeviceDisconnected;

        public event Action<DeviceInfoRequest>? DeviceInfoRequestReceived;
        public event Action<DeviceInfoResponse>? DeviceInfoReceived;

        public event Action<DeviceExecuteCommandRequest>? CommandReceived;
        public event Action<DeviceExecuteCommandResponse>? CommandResultReceived;

        public bool IsConnected => _connection.State == HubConnectionState.Connected;

        public MainHub()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{BuildConfig.ApiBaseUrl}/mainhub")
                .WithAutomaticReconnect()
                .Build();

            _connection.Closed += async (error) =>
            {
                ConnectionStatusChanged?.Invoke(false);
                Debug.WriteLine($"Connection closed: {error?.Message}");
            };

            _connection.Reconnecting += async (error) =>
            {
                Debug.WriteLine($"Connection reconnecting: {error?.Message}");
            };

            _connection.Reconnected += async (connectionId) =>
            {
                ConnectionStatusChanged?.Invoke(true);
                Debug.WriteLine($"Connection reconnected: {connectionId}");
            };

            // Прослушивание входящих уведомлений
            _connection.On<string>(HubCommands.Notification, (message) =>
            {
                NotificationReceived?.Invoke(message);
            });

            // Прослушивание входящих ошибок
            _connection.On<string>(HubCommands.Error, (message) =>
            {
                ErrorReceived?.Invoke(message);
            });

            // Прослушивание подключения новых девайсов
            _connection.On<DeviceIdentity>(HubCommands.DeviceOnline, (device) =>
            {
                DeviceConnected?.Invoke(device);
            });

            // Прослушивание отключения новых девайсов
            _connection.On<string>(HubCommands.DeviceOffline, (deviceId) =>
            {
                DeviceDisconnected?.Invoke(deviceId);
            });

            // ЗАПРОС ИНФОРМАЦИИ О УСТРЙОСТВЕ
            // Прослушивание входящих запрсов на передачу информации
            _connection.On<DeviceInfoRequest>(HubCommands.CollectAndSendInfo, (request) => 
            {
                DeviceInfoRequestReceived?.Invoke(request);
            });

            // Прослушивание входящих результатов информации
            _connection.On<DeviceInfoResponse>(HubCommands.DeviceInfoReceived, (response) =>
            {
                DeviceInfoReceived?.Invoke(response);
            });

            // ВЫПОЛНЕНИЕ КОМАНД
            // Прослушивание входящих команд
            _connection.On<DeviceExecuteCommandRequest>(HubCommands.ExecuteCommand, (request) =>
            {
                CommandReceived?.Invoke(request);
            });

            // Прослушивание входящих результатов выполнения команд
            _connection.On<DeviceExecuteCommandResponse>(HubCommands.CommandResult, (response) =>
            {
                CommandResultReceived?.Invoke(response);
            });
        }

        public async Task ConnectAsync(RegisterDeviceReuest request)
        {
            if (_connection.State is HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();

                ConnectionStatusChanged?.Invoke(IsConnected);

                await _connection.InvokeAsync(
                    nameof(IHubContract.RegisterDevice), 
                    request
                );
            }
        }

        public async Task RequestDeviceInfoAsync(string targetDeviceId, string currentDeviceId)
        {
            if (_connection is not null && _connection.State is HubConnectionState.Connected)
            {
                await _connection.SendAsync(
                    nameof(IHubContract.RequestDeviceInfo),
                    new DeviceInfoRequest(targetDeviceId, currentDeviceId)
                );
            }
        }

        public async Task SendDeviceInfoAsync(DeviceInfoResponse response)
        {
            if (_connection.State is HubConnectionState.Connected)
            {
                await _connection.SendAsync(
                    nameof(IHubContract.ReceiveDeviceInfo), 
                    response
                );
            }
        }

        public async Task ExecuteCommandAsync(string targetDeviceId, string command, string currentDeviceId)
        {
            if (_connection.State is HubConnectionState.Connected)
            {
                await _connection.SendAsync(
                    nameof(IHubContract.RequestDeviceCommand), 
                    new DeviceExecuteCommandRequest(targetDeviceId, currentDeviceId, command)
                );
            }
        }

        public async Task SendCommandResultAsync(DeviceExecuteCommandResponse response)
        {
            if (_connection.State is HubConnectionState.Connected)
            {
                await _connection.SendAsync(
                    nameof(IHubContract.ReceiveCommandResult), 
                    response
                );
            }
        }
    }
}
