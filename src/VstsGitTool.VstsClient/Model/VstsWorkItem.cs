namespace VstsGitTool.VstsClient.Model
{
    public class VstsWorkItem
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string AssignedTo { get; set; }
        public int BranchCount { get; set; }
        public bool HasBranches => BranchCount > 0;
    }
}
