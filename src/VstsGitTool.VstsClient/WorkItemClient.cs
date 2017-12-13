using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using VstsGitTool.VstsClient.Model;

namespace VstsGitTool.VstsClient
{
    public class WorkItemClient : IDisposable
    {
        private readonly string _collectionUri;
        private readonly string _personalAccessToken;

        private VssConnection _connection;
        private readonly string _teamProjectName;

        public WorkItemClient(string teamProjectName, string collectionUri,
            string personalAccessToken)
        {
            _teamProjectName = teamProjectName;
            _collectionUri = collectionUri;
            _personalAccessToken = personalAccessToken;

            Connect();
        }

        private void Connect()
        {
            _connection = new VssConnection(new Uri(_collectionUri),
                new VssBasicCredential(string.Empty, _personalAccessToken));
        }

        public async Task<VstsQuery[]> GetQueries()
        {
            var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            var queries = await witClient.GetQueriesAsync(_teamProjectName, includeDeleted: false, depth: 2,
                expand: QueryExpand.Wiql);

            return queries.Select(ToVstsQuery).OrderBy(query => query.Name).ToArray();
        }

        private VstsQuery ToVstsQuery(QueryHierarchyItem queryHierarchyItem)
        {
            return new VstsQuery
            {
                Id = queryHierarchyItem.Id,
                Name = queryHierarchyItem.Name,
                IsFolder = queryHierarchyItem.IsFolder ?? false,
                HasChildren = queryHierarchyItem.HasChildren ?? false,
                Children = queryHierarchyItem.Children?.Select(ToVstsQuery).OrderBy(query => query.Name).ToArray()
            };
        }

        public async Task<Guid> CreateStandardQuery()
        {
            var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            var queries = await witClient.GetQueriesAsync(_teamProjectName, includeDeleted: false, depth: 2,
                expand: QueryExpand.Wiql);

            var myQueries = queries.SingleOrDefault(item => item.Name == "My Queries");

            var queryName = "Assigned to me and not Done";

            var myNotDoneItemsQuery = myQueries.Children.SingleOrDefault(item => item.Name == queryName);

            if (myNotDoneItemsQuery == null)
            {
                // if the 'REST Sample' query does not exist, create it.
                myNotDoneItemsQuery = new QueryHierarchyItem
                {
                    Name = queryName,
                    Wiql =
                        "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags],[System.IterationPath] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.AssignedTo] = @me AND [System.State] <> 'Done' AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Product Backlog Item') order by [System.ChangedDate] desc",
                    IsFolder = false
                };
                myNotDoneItemsQuery =
                    await witClient.CreateQueryAsync(myNotDoneItemsQuery, _teamProjectName, myQueries.Name);
            }

            return myNotDoneItemsQuery.Id;
        }

        public async Task<Guid> CreateStandardQuery2()
        {
            var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            var queries = await witClient.GetQueriesAsync(_teamProjectName, includeDeleted: false, depth: 2, expand: QueryExpand.Wiql);

            var myQueries = queries.SingleOrDefault(item => item.Name == "My Queries");

            var queryName = "Committed In Sprint";

            var myNotDoneItemsQuery = myQueries.Children.SingleOrDefault(item => item.Name == queryName);

            if (myNotDoneItemsQuery == null)
            {
                // if the 'REST Sample' query does not exist, create it.
                myNotDoneItemsQuery = new QueryHierarchyItem
                {
                    Name = queryName,
                    Wiql = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags],[System.IterationPath] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.State] = 'Committed' AND [System.BoardColumn] = 'In Sprint' AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Product Backlog Item') order by [System.ChangedDate] desc",
                    IsFolder = false
                };
                myNotDoneItemsQuery = await witClient.CreateQueryAsync(myNotDoneItemsQuery, _teamProjectName, myQueries.Name);
            }

            return myNotDoneItemsQuery.Id;
        }

        public async Task<VstsWorkItem[]> GetWorkItems(Guid queryId)
        {
            var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            var result = await witClient.QueryByIdAsync(queryId);

            if (result.WorkItems.Any())
            {
                var allWorkItems = new List<WorkItem>(result.WorkItems.Count());

                var skip = 0;
                const int batchSize = 100;
                IEnumerable<WorkItemReference> workItemRefs;
                do
                {
                    workItemRefs = result.WorkItems.Skip(skip).Take(batchSize);
                    if (workItemRefs.Any())
                    {
                        // get details for each work item in the batch
                        var workItems = await witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id),
                            expand: WorkItemExpand.Relations);
                        allWorkItems.AddRange(workItems);
                    }
                    skip += batchSize;
                } while (workItemRefs.Count() == batchSize);

                return allWorkItems.Select(item => new VstsWorkItem
                {
                    Id = item.Id,
                    Title = item.Fields["System.Title"] as string,
                    State = item.Fields["System.State"] as string,
                    Type = item.Fields.ContainsKey("System.WorkItemType")
                        ? item.Fields["System.WorkItemType"] as string
                        : null,
                    AssignedTo = item.Fields.ContainsKey("System.AssignedTo")
                        ? item.Fields["System.AssignedTo"] as string
                        : "Unassigned",
                    BranchCount = item.Relations.Count(relation =>
                        relation.Rel == "ArtifactLink" && relation.Attributes != null &&
                        relation.Attributes.ContainsKey("name") && relation.Attributes["name"].ToString() == "Branch")
                }).ToArray();
            }
            else
            {
                Console.WriteLine("No work items were returned from query.");
                return new VstsWorkItem[] { };
            }
        }

        public async Task AddBranchLink(int workItemId, Guid projectId, Guid repositoryId, string branchName)
        {
            var witClient = _connection.GetClient<WorkItemTrackingHttpClient>();

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation {Operation = Operation.Add, Path = "/relations/-", Value = new
                    {
                        rel = "ArtifactLink",
                        url = $"vstfs:///Git/Ref/{projectId}%2F{repositoryId}%2FGB{branchName.Replace("/", "%2F")}",
                        Attributes = new Dictionary<string,object>
                        {
                            { "name", "Branch" }
                        },
                        name = "Branch"
                    }
                }
            };

            await witClient.UpdateWorkItemAsync(patch,
                workItemId);
        }

        public void Dispose()
        {
            _connection.Disconnect();
        }
    }
}
