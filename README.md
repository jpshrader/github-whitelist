# Github Whitelist

Using [octokit.net](ctokit.net), this tool will call the [GitHub Meta Api](https://docs.github.com/en/free-pro-team@latest/rest/meta/meta?apiVersion=2022-11-28#get-github-meta-information) to determine the current pool of GitBub Actions nodes and their IPs (GitHub updates this pool weekly). The tool will then update your organization's [IP allow list](https://docs.github.com/en/enterprise-cloud@latest/organizations/keeping-your-organization-secure/managing-security-settings-for-your-organization/managing-allowed-ip-addresses-for-your-organization) with those IPs.

### Use Case

If you have an organization with an IP Whitelist gating your GitHub organization and/or its repos, you are unable to use GitHub Actions as the running node won't have access to clone the repo. Your two options are:

1. Create your own self-hosted runner that has a static IP, whitelist that IP, and route all GHA traffic to that runner.
2. Have an automated way to identify and whitelist all IPs belonging to GitHub Action runner nodes, allowing these nodes to clone/interact with your repos.

While `Option 1` is likely the _optimal_ solution, it incurs a lot of maintanence costs as you now have to maintain and pay for that node. `Option 2` will enable you to shield your code while granting access to verified GitHub resources, allowing your team to continue to lean on GitHub Actions for CI/CD builds.


### Observations

After completing the initial work of adding the node IPs (which was ~3800 as of December 2023), the week-to-week work was very minimal. Most of the time there are no changes, other times there are a maybe a dozen in total (nodes added/removed).

From what I can see, this approach would absolutely serve as an adequate solution, especially if you're short on time.
