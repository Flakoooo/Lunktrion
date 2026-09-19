using Avalonia.Threading;
using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
using System;
using System.Collections.ObjectModel;

namespace LunktrionApp.Services
{
    public class NotificationService
    {
        public ObservableCollection<NotificationItem> Notifications { get; set; } = [];

        public void ShowNotification(
            string message, NotificationType type
        ) => CreateNotification(new NotificationItem(message, type));

        public void ShowInfo(
            string message
        ) => CreateNotification(new NotificationItem(message, NotificationType.Info));

        public void ShowSuccess(
            string message
        ) => CreateNotification(new NotificationItem(message, NotificationType.Success));

        public void ShowError(
            string message
        ) => CreateNotification(new NotificationItem(message, NotificationType.Error));

        private void CreateNotification(NotificationItem notification) => Dispatcher.UIThread.Post(() =>
        {
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
    }
}
