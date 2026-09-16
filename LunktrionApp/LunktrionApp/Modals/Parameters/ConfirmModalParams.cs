namespace LunktrionApp.Modals.Parameters
{
    public record ConfirmModalParams(
        string DisplayText, 
        ConfirmType ConfirmType,
        bool CodeInputRequired = false,
        bool IsCodeNullable = true,
        string DisplayCodeText = "Введите код подтверждения",
        string SuccessText = "Выполнить", 
        string DeclineText = "Отмена"
    );
}
