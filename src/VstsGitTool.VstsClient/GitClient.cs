using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using VstsGitTool.VstsClient.Model;

namespace VstsGitTool.VstsClient
{
    public class GitClient : IDisposable
    {
        private readonly string _teamProjectName;
        private readonly string _collectionUri;
        private readonly string _personalAccessToken;
        private VssConnection _connection;

        public GitClient(string teamProjectName, string collectionUri,
            string personalAccessToken)
        {
            _teamProjectName = teamProjectName;
            _collectionUri = collectionUri;
            _personalAccessToken = personalAccessToken;

            Connect();
        }

        private void Connect()
        {
            _connection = new VssConnection(new Uri(_collectionUri), new VssBasicCredential(string.Empty, _personalAccessToken));
        }

        public async Task<VstsGitRepository[]> GetRepos()
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            var vstsRepos = await gitClient.GetRepositoriesAsync(_teamProjectName);

            return vstsRepos.Select(vstsRepo =>
            new VstsGitRepository
            {
                Id = vstsRepo.Id,
                Name = vstsRepo.Name,
                DefaultBranch = vstsRepo.DefaultBranch,
                ProjectId = vstsRepo.ProjectReference.Id,
                WebUrl = vstsRepo.RemoteUrl
            }).OrderBy(repository => repository.Name).ToArray();
        }

        public async Task<VstsGitRepository> GetRepo(string repoName)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            var vstsRepo = await gitClient.GetRepositoryAsync(_teamProjectName, repoName);

            return new VstsGitRepository
            {
                Id = vstsRepo.Id,
                Name = vstsRepo.Name,
                DefaultBranch = vstsRepo.DefaultBranch
            };
        }

        public async Task<VstsGitBranch[]> GetBranches(Guid repoId)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            var vstsBranches = await gitClient.GetBranchesAsync(repoId);

            return vstsBranches.Select(vstsBranch =>
                new VstsGitBranch
            {
                Name = vstsBranch.Name,
                AheadCount = vstsBranch.AheadCount,
                BehindCount = vstsBranch.BehindCount
                //CommitId = vstsBranch.Commit.CommitId
                //CommitComment
            }).ToArray();
        }

        private async Task<GitRef> GetBranchRef(string branchName, Guid repositoryId)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            if (branchName.StartsWith("refs/"))
            {
                branchName = branchName.Remove(0, 5);
            }

            if (!branchName.StartsWith("heads/"))
            {
                branchName = $"heads/{branchName}";
            }

            var sourceRefs = await gitClient.GetRefsAsync(repositoryId, filter: branchName);

            return sourceRefs.FirstOrDefault();
        }

        public async Task CreateBranch(string branchName, string sourceBranchName, Guid repositoryId)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            if (!branchName.StartsWith("refs/heads/"))
            {
                branchName = $"refs/heads/{branchName}";
            }
            
            var sourceRef = await GetBranchRef(sourceBranchName, repositoryId);

            if (sourceRef == null)
            {
                throw new Exception("Source branch ref not found");
            }

            var gitRef = new GitRefUpdate
            {
                Name = branchName,
                IsLocked = false,
                OldObjectId = new string('0', 40),
                NewObjectId = sourceRef.ObjectId
            };

            var gitRefUpdate = await gitClient.UpdateRefsAsync(new[] { gitRef }, repositoryId);
        }

        public async Task DeleteBranch(string branchName, Guid repositoryId)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            if (!branchName.StartsWith("refs/heads/"))
            {
                branchName = $"refs/heads/{branchName}";
            }

            var branchRef = await GetBranchRef(branchName, repositoryId);

            if (branchRef == null)
            {
                throw new Exception("Branch ref not found");
            }

            var gitRef = new GitRefUpdate
            {
                Name = branchName,
                IsLocked = false,
                OldObjectId = branchRef.ObjectId,
                NewObjectId = new string('0', 40)
            };

            var gitRefUpdate = await gitClient.UpdateRefsAsync(new[] { gitRef }, repositoryId);
        }

        public async Task CreatePullRequest(string repositoryId)
        {
            var gitClient = _connection.GetClient<GitHttpClient>();

            var pullRequestToCreate = new GitPullRequest
            {
                
            };

            var vstsPullRequest = await gitClient.CreatePullRequestAsync(pullRequestToCreate, repositoryId);
        }

        public void Dispose()
        {
            _connection.Disconnect();
        }
    }
}
