namespace VstsGitTool.VstsClient
{
    public class VstsClientFactory
    {
        private readonly string _collectionUri;
        private readonly string _accessToken;

        public VstsClientFactory(string collectionUri, string accessToken)
        {
            _collectionUri = collectionUri;
            _accessToken = accessToken;
        }

        public ProjectClient CreateProjectClient()
        {
            return new ProjectClient(_collectionUri, _accessToken);
        }

        public WorkItemClient CreateWorkItemClient(string teamProjectName)
        {
            return new WorkItemClient(teamProjectName, _collectionUri, _accessToken);
        }

        public GitClient CreateGitClient(string teamProjectName)
        {
            return new GitClient(teamProjectName, _collectionUri, _accessToken);
        }
    }
}
