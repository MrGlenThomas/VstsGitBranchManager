using System.Configuration;

namespace VstsGitTool.Desktop.Configuration
{
    internal class VstsConfiguration
    {
        private static string _accessToken;
        private static string _collectionUri;

        public static string AccessToken =>
            _accessToken ?? (_accessToken = ConfigurationManager.AppSettings["AccessToken"]);

        public static string CollectionUri =>
            _collectionUri ?? (_collectionUri = ConfigurationManager.AppSettings["CollectionUri"]);
    }
}
