using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using VstsGitTool.VstsClient.Model;

namespace VstsGitTool.VstsClient
{
    public class ProjectClient : IDisposable
    {
        private readonly string _collectionUri;
        private readonly string _personalAccessToken;
        private VssConnection _connection;

        public ProjectClient(string collectionUri, string personalAccessToken)
        {
            _collectionUri = collectionUri;
            _personalAccessToken = personalAccessToken;

            Connect();
        }

        private void Connect()
        {
            _connection = new VssConnection(new Uri(_collectionUri), new VssBasicCredential(string.Empty, _personalAccessToken));
        }

        public async Task<IEnumerable<VstsProject>> GetProjects()
        {
            var projectClient = _connection.GetClient<ProjectHttpClient>();

            var vstsProjects = await projectClient.GetProjects(ProjectState.All);

            return vstsProjects.Select(p => new VstsProject {Id = p.Id, Name = p.Name}).ToArray();
        }

        public void Dispose()
        {
            _connection.Disconnect();
        }
    }
}
