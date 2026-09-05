using LunktrionApp.Models.Entities;
using LunktrionApp.Models.Enums;
using System.Collections.ObjectModel;

namespace LunktrionApp.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        public ObservableCollection<NotificationItem> Notifications { get; set; } = [];

        public NotificationViewModel()
        {
            Notifications.Add(new NotificationItem("тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1тест 1", NotificationType.Error));
            Notifications.Add(new NotificationItem("тест 2", NotificationType.Notification));
        }

    }
}
