using System.Collections.Immutable;

namespace github_whitelist {
    class Program {
        private const string ALLLOWLIST_DESC = "Managed by the GitHub Actions IP Whitelist";

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

            var (allowList, ipListErr) = await client.GetIpAllowList(orgSlug);
            if (ipListErr is not null) {
                Console.WriteLine(ipListErr);
                return -1;
            }

            var managedAllowListItems = allowList.Where(i => i.Description == ALLLOWLIST_DESC).ToImmutableArray();
            var deletedItems = await client.DeleteIpAllowListItems(managedAllowListItems);
            var numFailedToDelete = managedAllowListItems.Length - deletedItems.Length;
            if (numFailedToDelete > 0) {
                Console.WriteLine($"failed to delete {numFailedToDelete} managed entries");
                return -1;
            }

            var orgId = await client.GetOrgId(orgSlug);

            var (nodes, nodeErr) = await client.GetGitHubActionNodes();
            if (nodeErr is not null) {
                Console.WriteLine(nodeErr);
                return -1;
            }

            var newAllowListItems = nodes.Select(n => new AllowedIp {
                Cidr = n,
                Description = ALLLOWLIST_DESC,
                IsActive = true,
            }).ToImmutableArray();
            // var addErrors = await client.AddIpAllowListItems(orgId, newAllowListItems);
            // if (addErrors.Any()) {
            //     foreach (var error in addErrors) {
            //         var id = "N/A";
            //         if (error is CoundNotCreateError err) {
            //             id = err.Id;
            //         }
            //         Console.WriteLine($"unable to remove ip {id}: {error.Message}");
            //     }
            //     return -1;
            // }

            var summaryTag = new string('=', 10);
            Console.WriteLine($"{summaryTag} SUMMARY {summaryTag}");
            Console.WriteLine($"found {allowList.Length} entires in the allow list");
            Console.WriteLine($"found {managedAllowListItems.Length} managed allow list entires");
            Console.WriteLine($"deleted {deletedItems.Length} managed allow list entires");
            Console.WriteLine($"found {nodes.Length} github action nodes");
            Console.WriteLine($"added {newAllowListItems.Length} managed allow list entries");
            return 0;
        }
    }
}