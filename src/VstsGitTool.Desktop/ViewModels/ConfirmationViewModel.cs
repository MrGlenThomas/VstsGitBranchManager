using System;
using VstsGitTool.Desktop.Command;

namespace VstsGitTool.Desktop.ViewModels
{
    public class ConfirmationViewModel : ViewModelBase
    {
        private readonly ConfirmationButtons _confirmationButtons;

        public ConfirmationViewModel(string title, string message, ConfirmationButtons confirmationButtons, Action<ConfirmationResult> performAction)
        {
            _confirmationButtons = confirmationButtons;
            Title = title;
            Message = message;
            PerformAction = performAction;
        }

        public string Title { get; }

        public string Message { get; }

        public Action<ConfirmationResult> PerformAction { get; }

        public bool IsNoAvailable => _confirmationButtons == ConfirmationButtons.YesNo ||
                                         _confirmationButtons == ConfirmationButtons.YesNoCancel;

        public bool IsCancelAvailable => _confirmationButtons == ConfirmationButtons.YesCancel ||
                                         _confirmationButtons == ConfirmationButtons.YesNoCancel;
    }
}
