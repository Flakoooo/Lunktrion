namespace LunktrionApp.Modals.Results
{
    public record ConfirmResult(
        bool IsConfirmed,
        ushort? Code = null
    );
}
