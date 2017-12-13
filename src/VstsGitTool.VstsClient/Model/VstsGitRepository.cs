using System;

namespace VstsGitTool.VstsClient.Model
{
    public class VstsGitRepository
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DefaultBranch { get; set; }
        public Guid ProjectId { get; internal set; }
        public string WebUrl { get; set; }
    }
}
