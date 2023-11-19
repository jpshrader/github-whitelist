using System.Collections.Immutable;
using Octokit;
using Octokit.GraphQL;

namespace github_whitelist {
    public class OctokitClient {
        private const string PRODUCT_NAME = "github-whitelist";
        private const string PRODUCT_VERSION = "0.1.0";
        private const string ALLOWLIST_DESC = "Managed by the GitHub Actions IP Whitelist";

        private readonly GitHubClient rest;
        private readonly Octokit.GraphQL.Connection graphQl;

        public OctokitClient(string authToken) {
            var productInfoRest = new Octokit.ProductHeaderValue(PRODUCT_NAME);
            rest = new GitHubClient(productInfoRest){
                Credentials = new Credentials(authToken)
            };

            var productInfoGraphQl = new Octokit.GraphQL.ProductHeaderValue(PRODUCT_NAME, PRODUCT_VERSION);
            graphQl = new Octokit.GraphQL.Connection(productInfoGraphQl, authToken);
        }

        public async Task<(IImmutableList<string> nodes, Error? error)> GetGitHubActionNodes() {
            var metadata = await rest.Meta.GetMetadata();

            var response = metadata.Actions;
            if (response is null) {
                return (Enumerable.Empty<string>().ToImmutableList(), new NotFoundError("github action nodes"));
            }

            return (response.ToImmutableList(), null);
        }

        public async Task<(IImmutableList<AllowedIp> allowList, Error? error)> GetIpAllowList(string orgSlug) {
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
                return (Enumerable.Empty<AllowedIp>().ToImmutableList(), new NotFoundError("ip allow list"));
            }

            return (response.ToImmutableList(), null);
        }
    }

    public class AllowedIp {
        public required ID Id { get; set; }

        public required string Cidr { get; set; }

        public required string Description { get; set; }

        public required bool IsActive { get; set; }
    }
}