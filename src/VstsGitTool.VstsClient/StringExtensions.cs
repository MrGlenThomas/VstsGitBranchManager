namespace VstsGitTool.VstsClient
{
    public static class StringExtensions
    {
        public static string MakeGitBranchNameSafe(this string workItemTitle)
        {
            workItemTitle = workItemTitle.Replace(", ", ".");
            workItemTitle = workItemTitle.TrimStart('.');
            workItemTitle = workItemTitle.TrimEnd('/');
            workItemTitle = workItemTitle.Replace('\\', '-');
            workItemTitle = workItemTitle.Replace('~', '-');
            workItemTitle = workItemTitle.Replace('^', '-');
            workItemTitle = workItemTitle.Replace(':', '-');
            workItemTitle = workItemTitle.Replace(' ', '-');
            workItemTitle = workItemTitle.Replace('*', '-');
            workItemTitle = workItemTitle.Replace('?', '-');
            workItemTitle = workItemTitle.Replace('[', '-');

            if (workItemTitle.EndsWith(".lock"))
            {
                workItemTitle = workItemTitle.Substring(0, workItemTitle.Length - 5);
            }

            while (workItemTitle.Contains(".."))
            {
                workItemTitle = workItemTitle.Replace("..", ".");
            }

            while (workItemTitle.Contains("--"))
            {
                workItemTitle = workItemTitle.Replace("--", "-");
            }

            return workItemTitle;
        }
    }
}
