using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace LunktrionApp.Modals
{
    public partial class ModalContainerView : UserControl
    {
        public ModalContainerView()
        {
            InitializeComponent();
        }

        private void OnBackgroundClick(object? sender, PointerPressedEventArgs e)
        {
            if (ModalContent.Content == null) return;

            if (e.Source is Visual clickedElement && ModalContent.IsVisualAncestorOf(clickedElement))
            {
                e.Handled = true;
                return;
            }

            if (DataContext is ViewModels.ModalContainerViewModel vm)
            {
                vm.CloseModal();

                e.Handled = true;
            }
        }
    }
}