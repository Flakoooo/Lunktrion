using Avalonia.Controls;

namespace LunktrionApp.Views
{
    public partial class NotificationView : UserControl
    {
        public NotificationView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (TopLevel.GetTopLevel(this) is Window window)
            {
                window.SizeChanged += Window_SizeChanged;

                UpdateNotificationWidth(window.Bounds.Width);
            }
        }

        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateNotificationWidth(e.NewSize.Width);
        }

        private void UpdateNotificationWidth(double windowWidth)
        {
            NotificationsList.MaxWidth = windowWidth / 3;
        }
    }
}