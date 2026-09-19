using System.Threading.Tasks;

namespace LunktrionApp.Abstractions
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
