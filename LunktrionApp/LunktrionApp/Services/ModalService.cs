using LunktrionApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.Services
{
    public class ModalService(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public event Action? ModalStateChanged;

        public IModalWindow? ActiveModal { get; set; }
        public bool IsModalOpen => ActiveModal is not null;

        public async Task<TResult> ShowModalAsync<TModal, TParam, TResult>(
            TParam parameters
        ) where TModal : class, IModalWindow<TParam, TResult>
        {
            var modal = _serviceProvider.GetRequiredService<TModal>();
            modal.Initialize(parameters);

            ActiveModal = modal;
            ModalStateChanged?.Invoke();

            TResult result = await modal.ResultTask;

            CloseModal();
            return result;
        }

        public void CloseModal()
        {
            ActiveModal = null;
            ModalStateChanged?.Invoke(); 
        }
    }
}
