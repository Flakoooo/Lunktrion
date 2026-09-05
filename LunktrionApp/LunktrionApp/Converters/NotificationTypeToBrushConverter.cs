using Avalonia.Data.Converters;
using Avalonia.Media;
using LunktrionApp.Models.Enums;
using System;
using System.Globalization;

namespace LunktrionApp.Converters
{
    public class NotificationTypeToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                return type switch
                {
                    NotificationType.Notification => Brush.Parse("#5FA866"),
                    NotificationType.Error => Brush.Parse("#D95C4A"),
                    _ => Brush.Parse("#5FA866")
                };
            }
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
