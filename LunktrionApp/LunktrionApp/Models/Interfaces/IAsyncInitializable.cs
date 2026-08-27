using System.Threading.Tasks;

namespace LunktrionApp.Models.Interfaces
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }

    public interface IAsyncInitializable<in T>
    {
        Task InitializeAsync(T parameter);
    }
}
