using System;
using System.Collections.Generic;

namespace VstsGitTool.VstsClient.Model
{
    public class VstsQuery
    {
        public bool HasChildren { get; set; }

        public IEnumerable<VstsQuery> Children { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool? IsFolder { get; set; }
    }
}
