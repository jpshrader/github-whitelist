namespace github_whitelist {
    class Program {
        static int Main(string[] args) {
            if (!args.Any()) {
                Console.WriteLine("received no arguments");
                return -1;
            }

            foreach (var arg in args) {
                Console.WriteLine($"arg: {arg}");
            }

            return 0;
        }
    }
}