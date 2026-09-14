using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public interface IModalWindow { }

    public interface IModalWindow<TParam, TResult> : IModalWindow
    {
        void Initialize(TParam parameters);

        Task<TResult> ResultTask { get; }
    }
}
