using Avalonia.Controls;
using Avalonia.Threading;
using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
using LunktrionApp.Services;
using System;
using System.Collections.ObjectModel;

namespace LunktrionApp.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        private readonly NotificationService _notificationService;

        public ObservableCollection<NotificationItem> Notifications { get; set; } = [];

        public NotificationViewModel(
            NotificationService notificationService
        )
        {
            _notificationService = notificationService;

            Notifications = _notificationService.Notifications;
        }

        public NotificationViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _notificationService = null!;

            Notifications.Add(new NotificationItem("тест 2", NotificationType.Info));
            Notifications.Add(new NotificationItem("тест 3", NotificationType.Success));
            Notifications.Add(new NotificationItem("тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1", NotificationType.Error));
        }
    }
}
