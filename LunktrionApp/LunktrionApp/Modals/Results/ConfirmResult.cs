namespace LunktrionApp.Modals.Results
{
    public record ConfirmResult(
        bool IsConfirmed,
        int? Code = null
    );
}
