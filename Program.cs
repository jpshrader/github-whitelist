using Octokit;
using Octokit.GraphQL;

namespace github_whitelist {
    class Program {
        static async Task<int> Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("must provide 2 arguments: <organization slug> <github api token>");
                return -1;
            }

            var orgSlug = args[0];
            if (string.IsNullOrWhiteSpace(orgSlug)) {
                Console.WriteLine("organization slug cannot be empty");
                return -1;
            }

            var token = args[1];
            if (string.IsNullOrWhiteSpace(token)) {
                Console.WriteLine("github api token cannot be empty");
                return -1;
            }

            var productInfoRest = new Octokit.ProductHeaderValue("github-whitelist");
            var rest = new GitHubClient(productInfoRest){
                Credentials = new Credentials(token)
            };

            Console.WriteLine("fetching github metadata...");
            var metadata = await rest.Meta.GetMetadata();
            Console.WriteLine($"found {metadata.Actions.Count} build nodes");

            var productInfoGraphQl = new Octokit.GraphQL.ProductHeaderValue("github-whitelist", "0.1.0");
            var graphQl = new Octokit.GraphQL.Connection(productInfoGraphQl, token);

            var allowListQuery = new Query()
                .Organization(orgSlug)
                .IpAllowListEntries(100)
                .Nodes
                .Select(entry => new {
                    entry.Id,
                    entry.Name,
                    entry.AllowListValue,
                    entry.IsActive
                });

            var response = await graphQl.Run(allowListQuery);

            Console.WriteLine($"found {response.Count()} ips in the allow list");
            return 0;
        }
    }
}