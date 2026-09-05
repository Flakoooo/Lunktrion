namespace LunktrionApi.Models.Interfaces
{
    public interface IAsyncInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
