using Octokit;
using Octokit.GraphQL;

namespace github_whitelist {
    public class OctokitClient {
        private const string productName = "github-whitelist";
        private const string productVersion = "0.1.0";

        private const string allowListDescription = "Managed by the GitHub Actions IP Whitelist";

        private readonly GitHubClient rest;
        private readonly Octokit.GraphQL.Connection graphQl;

        public OctokitClient(string authToken) {
            var productInfoRest = new Octokit.ProductHeaderValue(productName);
            rest = new GitHubClient(productInfoRest){
                Credentials = new Credentials(authToken)
            };

            var productInfoGraphQl = new Octokit.GraphQL.ProductHeaderValue(productName, productVersion);
            graphQl = new Octokit.GraphQL.Connection(productInfoGraphQl, authToken);
        }

        public async Task<(List<string> nodes, bool notFound)> GetGitHubActionNodes() {
            var metadata = await rest.Meta.GetMetadata();

            var response = metadata.Actions;
            if (response is null) {
                return (Enumerable.Empty<string>().ToList(), true);
            }

            return (response.ToList(), false);
        }

        public async Task<(List<AllowedIp> allowList, bool notFound)> GetIpAllowList(string orgSlug) {
            var allowListQuery = new Query()
                .Organization(orgSlug)
                .IpAllowListEntries(100)
                .Nodes
                .Select(entry => new AllowedIp {
                    Id = entry.Id,
                    Cidr = entry.AllowListValue,
                    Description = entry.Name,
                    IsActive = entry.IsActive
                });

            var response = await graphQl.Run(allowListQuery);
            if (response is null) {
                return (Enumerable.Empty<AllowedIp>().ToList(), true);
            }

            return (response.ToList(), false);
        }
    }

    public class AllowedIp {
        public required ID Id { get; set; }

        public required string Cidr { get; set; }

        public required string Description { get; set; }

        public required bool IsActive { get; set; }
    }
}