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

            var client = new OctokitClient(token);

            var (nodes, nodeErr) = await client.GetGitHubActionNodes();
            if (nodeErr is not null) {
                Console.WriteLine(nodeErr);
                return -1;
            }

            var (allowList, ipListErr) = await client.GetIpAllowList(orgSlug);
            if (ipListErr is not null) {
                Console.WriteLine(ipListErr);
                return -1;
            }

            Console.WriteLine($"found {nodes.Count} github action nodes");
            Console.WriteLine($"found {allowList.Count} entires in the allow list");
            return 0;
        }
    }
}