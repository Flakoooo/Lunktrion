using Avalonia.Controls;
using Avalonia.Threading;
using LunktrionApp.Api;
using LunktrionApp.Hubs;
using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LunktrionApp.ViewModels
{
    public class NotificationViewModel : ViewModelBase, IDisposable
    {
        private readonly MainHub _mainHub;
        private readonly MainApi _mainApi;

        public ObservableCollection<NotificationItem> Notifications { get; set; } = [];

        public NotificationViewModel(
            MainHub mainHub,
            MainApi mainApi
        )
        {
            _mainHub = mainHub;
            _mainApi = mainApi;

            _mainHub.ConnectionStatusChanged += OnConnectionStatusChanged;
            _mainHub.NotificationReceived += OnNotificationReceived;
            _mainHub.ErrorReceived += OnErrorReceived;

            _mainApi.ErrorReceived += OnErrorReceived;
        }

        public NotificationViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _mainHub = null!;
            _mainApi = null!;

            Notifications.Add(new NotificationItem("тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1", NotificationType.Error));
            Notifications.Add(new NotificationItem("тест 2", NotificationType.Notification));
        }

        private void CreateNotification(NotificationItem notification) => Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine("создается уведомление");

            Notifications.Add(notification);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                Notifications.Remove(notification);
            };

            timer.Start();
        });

        private void OnConnectionStatusChanged(bool isConnected) => CreateNotification(
            new NotificationItem(
                isConnected ? "Соединение установлено" : "Потеряно соединение",
                isConnected ? NotificationType.Notification : NotificationType.Error
            )
        );

        private void OnNotificationReceived(string message) => CreateNotification(
            new NotificationItem(message, NotificationType.Notification)
        );

        private void OnErrorReceived(string message) => CreateNotification(
            new NotificationItem(message, NotificationType.Error)
        );

        public void Dispose()
        {
            _mainHub.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _mainHub.NotificationReceived -= OnNotificationReceived;
            _mainHub.ErrorReceived -= OnErrorReceived;

            _mainApi.ErrorReceived -= OnErrorReceived;
        }
    }
}
