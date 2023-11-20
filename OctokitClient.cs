using System.Collections.Immutable;
using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace github_whitelist {
    public class OctokitClient {
        private const string PRODUCT_NAME = "github-whitelist";
        private const string PRODUCT_VERSION = "0.1.0";

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

        public async Task<(ImmutableArray<string> nodes, Error? error)> GetGitHubActionNodes() {
            var metadata = await rest.Meta.GetMetadata();

            var response = metadata.Actions;
            if (response is null) {
                return (Enumerable.Empty<string>().ToImmutableArray(), new NotFoundError("github action nodes"));
            }

            return ([.. response], null);
        }

        public async Task<(ImmutableArray<AllowedIp> allowList, Error? error)> GetIpAllowList(string orgSlug) {
            var query = new Query()
                .Organization(orgSlug)
                .IpAllowListEntries()
                .AllPages()
                .Select(entry => new {
                    entry.Id,
                    entry.Name,
                    entry.AllowListValue,
                    entry.IsActive
                });

            var res = await graphQl.Run(query);
            if (res is null) {
                return (Enumerable.Empty<AllowedIp>().ToImmutableArray(), new NotFoundError("ip allow list"));
            }

            var response = res.Select(r => new AllowedIp {
                Id = r.Id,
                Description = r.Name,
                Cidr = r.AllowListValue,
                IsActive = r.IsActive
            }).ToImmutableArray();
            return (response, null);
        }

        public async Task<ImmutableArray<Error>> RemoveIpAllowList(IImmutableList<AllowedIp> allowList) {
            var errors = new List<Error>();
            foreach(var allowListItem in allowList) {
                var mutation = new Mutation().DeleteIpAllowListEntry(new DeleteIpAllowListEntryInput {
                    IpAllowListEntryId = allowListItem.Id
                });

                var response = await graphQl.Run(mutation);
                if (response is null) {
                    errors.Add(new CoundNotDeleteError("ip allow list", allowListItem.Id.ToString()));
                }
            }

            return [.. errors];
        }

        public async Task<ImmutableArray<Error>> AddIpAllowList(ID orgId, IImmutableList<AllowedIp> allowList) {
            var errors = new List<Error>();
            foreach(var allowListItem in allowList) {
                var mutation = new Mutation().CreateIpAllowListEntry(new CreateIpAllowListEntryInput {
                    OwnerId = orgId,
                    Name = allowListItem.Description,
                    AllowListValue = allowListItem.Cidr,
                    IsActive = allowListItem.IsActive,
                });

                var response = await graphQl.Run(mutation);
                if (response is null) {
                    errors.Add(new CoundNotDeleteError("ip allow list", allowListItem.Id.ToString()));
                }
            }

            return [.. errors];
        }

        public async Task<ID> GetOrgId(string orgSlug) {
            var query = new Query()
                .Organization(orgSlug)
                .Select(org => org.Id);

            return await graphQl.Run(query);
        }
    }

    public class AllowedIp {
        public required ID Id { get; set; }

        public required string Cidr { get; set; }

        public required string Description { get; set; }

        public required bool IsActive { get; set; }
    }
}