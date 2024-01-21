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

            var purge = false;
            if (args.Length > 2) {
                var parsed = bool.TryParse(args[2], out purge);
                if (!parsed) {
                    Console.WriteLine("purge must be a boolean");
                    return -1;
                }
            }

            var client = new OctokitClient(token);

            var (allowList, ipListErr) = await client.GetIpAllowList(orgSlug);
            if (ipListErr is not null) {
                Console.WriteLine(ipListErr);
                return -1;
            }
            var managedAllowListItems = allowList.Where(i => i.Description == ALLLOWLIST_DESC).ToImmutableArray();

            var (nodes, nodeErr) = await client.GetGitHubActionNodes();
            if (nodeErr is not null) {
                Console.WriteLine(nodeErr);
                return -1;
            }

            if (purge) {
                Console.WriteLine("purging allow list");
                await PurgeAllowList(client, allowList, managedAllowListItems, nodes);
            } else {
                Console.WriteLine("adding missing nodes to allow list");
                await AddMissingNodes(orgSlug, client, allowList, managedAllowListItems, nodes);
            }

            Console.WriteLine("done");
            return 0;
        }

        private static async Task AddMissingNodes(string orgSlug, OctokitClient client, ImmutableArray<AllowedIp> allowList, ImmutableArray<AllowedIp> managedAllowListItems, ImmutableArray<string> nodes){
            var managedItemsToDelete = managedAllowListItems
                .Where(i => !nodes.Contains(i.Cidr))
                .ToImmutableArray();

            Console.WriteLine($"found {nodes.Length} git hub action nodes");
            Console.WriteLine($"found {managedAllowListItems.Length} managed allow list entires");
            Console.WriteLine($"found {allowList.Length - managedAllowListItems.Length} other entires in the allow list");
            Console.WriteLine($"attempting to delete {managedItemsToDelete.Length} managed allow list entires");

            var deletedItems = await client.DeleteIpAllowListItems(managedItemsToDelete);
            var numFailedToDelete = managedItemsToDelete.Length - deletedItems.Length;
            if (numFailedToDelete > 0) {
                Console.WriteLine($"failed to delete {numFailedToDelete} managed entries");
                return;
            }

            Console.WriteLine($"deleted {managedItemsToDelete.Length} managed allow list entires");

            var orgId = await client.GetOrgId(orgSlug);
            var nodesToAdd = nodes
                .Where(n => !managedAllowListItems
                    .Select(i => i.Cidr)
                    .Contains(n)
                ).ToImmutableArray();
            var newAllowListItems = nodesToAdd.Select(n => new AllowedIp {
                Cidr = n,
                Description = ALLLOWLIST_DESC,
                IsActive = true,
            }).ToImmutableArray();

            Console.WriteLine($"attempting to add {newAllowListItems.Length} managed allow list entries");

            var addedIps = await client.AddIpAllowListItems(orgId, newAllowListItems);
            var numFailedToCreate = newAllowListItems.Length - addedIps.Length;
            if (numFailedToCreate > 0) {
                Console.WriteLine($"failed to add {numFailedToCreate} managed entries");
                return;
            }

            Console.WriteLine($"added {addedIps.Length} managed allow list entries");
            Console.WriteLine($"kept {nodes.Length - nodesToAdd.Length} managed allow list entries");
        }

        private static async Task PurgeAllowList(OctokitClient client, ImmutableArray<AllowedIp> allowList, ImmutableArray<AllowedIp> managedAllowListItems, ImmutableArray<string> nodes){
            Console.WriteLine($"found {nodes.Length} git hub action nodes");
            Console.WriteLine($"found {managedAllowListItems.Length} managed allow list entires");
            Console.WriteLine($"found {allowList.Length - managedAllowListItems.Length} other entires in the allow list");
            Console.WriteLine($"attempting to purge {managedAllowListItems.Length} managed allow list entires");

            var deletedItems = await client.DeleteIpAllowListItems(managedAllowListItems);
            var numFailedToDelete = managedAllowListItems.Length - deletedItems.Length;
            if (numFailedToDelete > 0) {
                Console.WriteLine($"failed to delete {numFailedToDelete} managed entries");
                return;
            }

            Console.WriteLine($"deleted {managedAllowListItems.Length} managed allow list entires");
        }
    }
}