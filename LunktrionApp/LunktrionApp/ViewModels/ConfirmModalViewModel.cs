using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using static LunktrionApp.ViewModels.ConfirmModalViewModel;

namespace LunktrionApp.ViewModels
{
    public partial class ConfirmModalViewModel : ViewModelBase, IModalWindow<ConfirmModalParams, bool?>
    {
        public record ConfirmModalParams(
            string DisplayText, ConfirmType ConfirmType, Func<bool?> Action,
            string SuccessText = "Выполнить", string DeclineText = "Отмена"
        );

        public enum ConfirmType
        {
            Confirm,
            Delete
        }

        private readonly TaskCompletionSource<bool?> _tcs = new();
        public Task<bool?> ResultTask => _tcs.Task;

        [ObservableProperty]
        public partial string DisplayText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SuccessText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string DeclineText { get; set; } = string.Empty;

        public Func<bool?>? _action;

        [ObservableProperty]
        public partial ControlTheme? ActionButtonTheme { get; set; }

        private static ControlTheme? SetActionButtonTheme(string themeKey) => Application.Current is not null
            && Application.Current.TryFindResource(themeKey, out var resource)
            && resource is ControlTheme theme ? theme : null;

        public void Initialize(ConfirmModalParams parameters)
        {
            DisplayText = parameters.DisplayText;
            SuccessText = parameters.SuccessText;
            DeclineText = parameters.DeclineText;

            ActionButtonTheme = SetActionButtonTheme($"{(parameters.ConfirmType is ConfirmType.Confirm ? "Orange" : "Red")}Button");

            _action = parameters.Action;
        }

        public ConfirmModalViewModel()
        {
            if (!Design.IsDesignMode) return;

            DisplayText = "Вы уверены что хотите сделать это?";
            SuccessText = "Выполнить";
            DeclineText = "Отмена";

            ActionButtonTheme = SetActionButtonTheme("OrangeButton");

            _action = null!;
        }

        [RelayCommand]
        public void Close() => _tcs.SetResult(null);

        [RelayCommand]
        public void Confirm() => _tcs.SetResult(_action?.Invoke() ?? false);
    }
}
