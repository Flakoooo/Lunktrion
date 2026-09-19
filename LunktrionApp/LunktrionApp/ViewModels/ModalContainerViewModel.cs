using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LunktrionApp.Services;
using System;

namespace LunktrionApp.ViewModels
{
    public partial class ModalContainerViewModel : ViewModelBase, IDisposable
    {
        private readonly ModalService _modalService;

        [ObservableProperty]
        public partial bool IsModalOpen { get; set; }

        [ObservableProperty]
        public partial IModalWindow? ActiveModal { get; set; }

        public ModalContainerViewModel(ModalService modalService)
        {
            _modalService = modalService;

            _modalService.ModalStateChanged += OnModalStateChanged;
        }

        public ModalContainerViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _modalService = null!;

            IsModalOpen = true;
            ActiveModal = new ConfirmModalViewModel();
        }

        public void CloseModal() => _modalService.CloseModal();

        public void OnModalStateChanged() => Dispatcher.UIThread.Post(() =>
        {
            IsModalOpen = _modalService.IsModalOpen;
            ActiveModal = _modalService.ActiveModal;
        });

        public void Dispose()
        {
            _modalService.ModalStateChanged -= OnModalStateChanged;
        }
    }
}
