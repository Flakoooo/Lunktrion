using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Modals.Parameters;
using LunktrionApp.Modals.Results;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class ConfirmModalViewModel : ViewModelBase, IModalWindow<ConfirmModalParams, ConfirmResult>
    {
        private readonly TaskCompletionSource<ConfirmResult> _tcs = new();
        private bool _codeInputRequired = false;
        private bool _isCodeNullable = true;
        private const string SuccessWithoutCode = "Продолжить без кода";
        private const string SuccessWithCode = "Продолжить с кодом";


        public Task<ConfirmResult> ResultTask => _tcs.Task;


        [ObservableProperty]
        public partial string DisplayText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SuccessText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string DeclineText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ControlTheme? ActionButtonTheme { get; set; }

        [ObservableProperty]
        public partial bool IsCodeInputVisible { get; set; } = false;

        [ObservableProperty]
        public partial string DisplayCodeText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CodeInput { get; set; } = string.Empty;

        public void Initialize(ConfirmModalParams parameters)
        {
            _codeInputRequired = parameters.CodeInputRequired;
            _isCodeNullable = parameters.IsCodeNullable;

            DisplayText = parameters.DisplayText;
            SuccessText = parameters.SuccessText;
            DeclineText = parameters.DeclineText;
            DisplayCodeText = parameters.DisplayCodeText;

            ActionButtonTheme = SetActionButtonTheme($"{(parameters.ConfirmType is ConfirmType.Confirm ? "Orange" : "Red")}Button");
        }

        public ConfirmModalViewModel()
        {
            if (!Design.IsDesignMode) return;

            var parameters = new ConfirmModalParams("Вы уверены что хотите сделать это?", ConfirmType.Confirm);

            DisplayText = parameters.DisplayText;
            DeclineText = parameters.DeclineText;
            DisplayCodeText = parameters.DisplayCodeText;

            IsCodeInputVisible = true;
            if (IsCodeInputVisible)
            {
                SuccessText = _isCodeNullable ? SuccessWithoutCode : SuccessWithCode;
            }
            else
            {
                SuccessText = parameters.SuccessText;
            }

            CodeInput = "вау, код";

            ActionButtonTheme = SetActionButtonTheme($"{(parameters.ConfirmType is ConfirmType.Confirm ? "Orange" : "Red")}Button");
        }

        [RelayCommand]
        public void Close() => _tcs.SetResult(new ConfirmResult(false));

        [RelayCommand]
        public void Confirm()
        {
            if (_codeInputRequired)
            {
                IsCodeInputVisible = true;

                SuccessText = _isCodeNullable ? SuccessWithoutCode : SuccessWithCode;

                return;
            }

            _tcs.SetResult(new ConfirmResult(true));
        }

        [RelayCommand]
        public void ConfirmWithCode()
        {
            if (_isCodeNullable && string.IsNullOrWhiteSpace(CodeInput))
            {
                _tcs.SetResult(new ConfirmResult(true));
                return;
            }

            if (!int.TryParse(CodeInput, out var code))
            {
                // уведомить что код содержит ошибки, НЕ ЗАКРЫВАЯ ОКНО
            }

            _tcs.SetResult(new ConfirmResult(true, code));
        }

        private static ControlTheme? SetActionButtonTheme(string themeKey) => Application.Current is not null
            && Application.Current.TryFindResource(themeKey, out var resource)
            && resource is ControlTheme theme ? theme : null;
    }
}
