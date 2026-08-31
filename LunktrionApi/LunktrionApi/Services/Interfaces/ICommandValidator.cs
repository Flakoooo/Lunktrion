namespace LunktrionApi.Services.Interfaces
{
    public interface ICommandValidator
    {
        static abstract bool IsSafe(string command);
    }
}
