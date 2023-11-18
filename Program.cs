using Octokit;

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

            var client = new GitHubClient(new ProductHeaderValue("github-whitelist")){
                Credentials = new Credentials(token)
            };

            Console.WriteLine("fetching github metadata...");
            var metadata = await client.Meta.GetMetadata();
            foreach (var node in metadata.Actions) {
                Console.WriteLine(node);
            }

            Console.WriteLine("fetching organization...");
            var org = await client.Organization.Get(orgSlug);
            if (org is null) {
                Console.WriteLine($"org not found: {orgSlug}");
                return -1;
            }

            Console.WriteLine($"org found: {org.Url}");

            return 0;
        }
    }
}