namespace VstsGitTool.Desktop.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        public string Message { get; }

        public ErrorViewModel(string message)
        {
            Message = message;
        }
    }
}
