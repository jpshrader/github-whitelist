var arguments = Environment.GetCommandLineArgs();

if (arguments is null || arguments.Length < 1) {
    Console.WriteLine("received no arguments");
    return -1;
}

foreach (var arg in arguments.Skip(1)) {
    Console.WriteLine($"arg: {arg}");
}

return 0;