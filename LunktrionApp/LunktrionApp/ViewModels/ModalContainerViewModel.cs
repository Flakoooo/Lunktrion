using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class ModalContainerViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial bool IsModalOpen { get; set; }

        [ObservableProperty]
        public partial IModalWindow? ActiveModal { get; set; }

        public ModalContainerViewModel()
        {
            if (!Design.IsDesignMode) return;

            IsModalOpen = true;
            ActiveModal = new ConfirmModalViewModel();
        }

        public async Task<TResult> OpenModalAsync<TModal, TParam, TResult>(
            TParam parameters
        ) where TModal : class, IModalWindow<TParam, TResult>, new()
        {
            var modal = new TModal();
            modal.Initialize(parameters);

            ActiveModal = modal;
            IsModalOpen = true;

            TResult result = await modal.ResultTask;

            IsModalOpen = false;
            ActiveModal = null;

            return result;
        }

        public void CloseModal()
        {
            ActiveModal = null;
            IsModalOpen = false;
        }
    }
}
