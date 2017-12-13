using System;
using System.Configuration;
using Xunit;

namespace VstsGitTool.VstsClient.Tests
{
    public class GitClientTests
    {
        private const string TeamProjectName = "Backlog";
        private readonly string CollectionUri = ConfigurationManager.AppSettings["CollectionUri"];
        private readonly string AccessToken = ConfigurationManager.AppSettings["AccessToken"];

        [Fact]
        public async void CreateBranch()
        {
            var client = new GitClient(TeamProjectName, CollectionUri, AccessToken);

            await client.CreateBranch("refs/heads/test/123", "heads/develop", Guid.Parse("6e3239e6-e054-464a-81f5-6e95d72ba79a"));
        }
    }
}
