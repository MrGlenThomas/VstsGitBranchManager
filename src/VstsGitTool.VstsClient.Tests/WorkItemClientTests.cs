using System;
using System.Configuration;
using Xunit;

namespace VstsGitTool.VstsClient.Tests
{
    public class WorkItemClientTests
    {
        private const string TeamProjectName = "Backlog";
        private readonly string CollectionUri = ConfigurationManager.AppSettings["CollectionUri"];
        private readonly string AccessToken = ConfigurationManager.AppSettings["AccessToken"];

        [Fact]
        public async void CreateQueries()
        {
            var client = new WorkItemClient(TeamProjectName, CollectionUri, AccessToken);

            var standardQueryId1 = await client.CreateStandardQuery();
            var standardQueryId2 = await client.CreateStandardQuery2();

            Assert.NotEqual(Guid.Empty, standardQueryId1);
            Assert.NotEqual(Guid.Empty, standardQueryId2);
        }

        [Fact]
        public async void GetQueries()
        {
            var client = new WorkItemClient(TeamProjectName, CollectionUri, AccessToken);

            var queries = await client.GetQueries();

            Assert.NotEmpty(queries);
            Assert.Equal(2, queries.Length);
        }

        [Fact]
        public async void GetWorkItems()
        {
            var client = new WorkItemClient(TeamProjectName, CollectionUri, AccessToken);

            var standardQueryId = await client.CreateStandardQuery();

            var workItems = await client.GetWorkItems(standardQueryId);

            Assert.NotEmpty(workItems);
            Assert.Equal(1, workItems.Length);
        }
    }
}
