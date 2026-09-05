using LunktrionApp.Models.Enums;

namespace LunktrionApp.Models.Entities
{
    public record class NotificationItem(
        string Message, 
        NotificationType Type
    );
}
