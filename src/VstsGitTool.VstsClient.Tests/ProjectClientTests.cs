using System.Configuration;
using Xunit;

namespace VstsGitTool.VstsClient.Tests
{
    public class ProjectClientTests
    {
        private readonly string CollectionUri = ConfigurationManager.AppSettings["CollectionUri"];
        private readonly string AccessToken = ConfigurationManager.AppSettings["AccessToken"];

        [Fact]
        public async void GetProjects()
        {
            var client = new ProjectClient(CollectionUri, AccessToken);

            var projects = await client.GetProjects();
        }
    }
}
