var commandLineArgs = Environment.GetCommandLineArgs();

foreach (var arg in commandLineArgs) {
    Console.WriteLine($"Arg: {arg}");
}
