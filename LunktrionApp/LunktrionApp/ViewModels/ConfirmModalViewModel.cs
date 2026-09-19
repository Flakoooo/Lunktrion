using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LunktrionApp.Api;
using LunktrionApp.Modals.Parameters;
using LunktrionApp.Modals.Results;
using LunktrionApp.Services;
using System;
using System.Threading.Tasks;

namespace LunktrionApp.ViewModels
{
    public partial class ConfirmModalViewModel : ViewModelBase, IModalWindow<ConfirmModalParams, ConfirmResult>
    {
        private readonly NotificationService _notificationService;
        private readonly MainApi _mainApi;

        private readonly TaskCompletionSource<ConfirmResult> _tcs = new();
        private bool _codeInputRequired = false;
        private bool _isCodeNullable = true;
        private const string SuccessWithoutCode = "Продолжить без кода";
        private const string SuccessWithCode = "Продолжить с кодом";


        public Task<ConfirmResult> ResultTask => _tcs.Task;


        [ObservableProperty]
        public partial string DisplayText { get; set; } = string.Empty;

        private string _successText = string.Empty;

        public string SuccessText
        {
            get
            {
                if (IsCodeInputVisible)
                {
                    if (string.IsNullOrWhiteSpace(CodeInput))
                    {
                        return _isCodeNullable ? SuccessWithoutCode : SuccessWithCode;
                    }

                    return SuccessWithCode;
                }

                return _successText;
            }
        }

        [ObservableProperty]
        public partial string DeclineText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ControlTheme? ActionButtonTheme { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SuccessText))]
        public partial bool IsCodeInputVisible { get; set; } = false;

        [ObservableProperty]
        public partial string DisplayCodeText { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SuccessText))]
        public partial string CodeInput { get; set; } = string.Empty;

        public void Initialize(ConfirmModalParams parameters)
        {
            _codeInputRequired = parameters.CodeInputRequired;
            _isCodeNullable = parameters.IsCodeNullable;

            DisplayText = parameters.DisplayText;
            _successText = parameters.SuccessText;
            DeclineText = parameters.DeclineText;
            DisplayCodeText = parameters.DisplayCodeText;

            ActionButtonTheme = SetActionButtonTheme($"{(parameters.ConfirmType is ConfirmType.Confirm ? "Orange" : "Red")}Button");

            OnPropertyChanged(nameof(SuccessText));
        }

        public ConfirmModalViewModel(
            NotificationService notificationService,
            MainApi mainApi
        )
        {
            _notificationService = notificationService;
            _mainApi = mainApi;
        }

        public ConfirmModalViewModel()
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException(
                    "Этот конструктор предназначен только для дизайнера Avalonia и не должен вызываться в рантайме"
                );
            }

            _notificationService = null!;
            _mainApi = null!;

            var parameters = new ConfirmModalParams("Вы уверены что хотите сделать это?", ConfirmType.Confirm);

            DisplayText = parameters.DisplayText;
            DeclineText = parameters.DeclineText;
            DisplayCodeText = parameters.DisplayCodeText;

            IsCodeInputVisible = true;
            if (IsCodeInputVisible)
            {
                _successText = _isCodeNullable ? SuccessWithoutCode : SuccessWithCode;
            }
            else
            {
                _successText = parameters.SuccessText;
            }

            CodeInput = "вау, код";

            ActionButtonTheme = SetActionButtonTheme($"{(parameters.ConfirmType is ConfirmType.Confirm ? "Orange" : "Red")}Button");
        }

        [RelayCommand]
        public void Close() => _tcs.SetResult(new ConfirmResult(false));

        [RelayCommand]
        public async Task Confirm()
        {
            if (_codeInputRequired && !IsCodeInputVisible)
            {
                IsCodeInputVisible = true;
                return;
            }

            if (IsCodeInputVisible)
            {
                await ConfirmWithCode();
                return;
            }

            _tcs.SetResult(new ConfirmResult(true));
        }

        [RelayCommand]
        public async Task ConfirmWithCode()
        {
            if (_isCodeNullable && string.IsNullOrWhiteSpace(CodeInput))
            {
                _tcs.SetResult(new ConfirmResult(true));
                return;
            }

            if (!ushort.TryParse(CodeInput, out var code))
            {
                _notificationService.ShowError("Введенный код содержит ошибки");
                return;
            }

            if (!await _mainApi.VerifyCodeAsync(code))
            {
                _notificationService.ShowError("Введенный код неверный");
                return;
            }

            _tcs.SetResult(new ConfirmResult(true, code));
        }

        private static ControlTheme? SetActionButtonTheme(string themeKey) => Application.Current is not null
            && Application.Current.TryFindResource(themeKey, out var resource)
            && resource is ControlTheme theme ? theme : null;
    }
}
