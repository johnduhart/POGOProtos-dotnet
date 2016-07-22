string target = Argument("target", "Default");

Task("Nuget-Restore")
    .Does(() =>
{
    NuGetInstall("Google.Protobuf.Tools", new NuGetInstallSettings {
        ExcludeVersion = true,
        OutputDirectory = "./tools",
        Version = "3.0.0-beta3",
        Prerelease = true
    });
});

Task("Proto")
    .IsDependentOn("Nuget-Restore")
    .Does(() =>
{
    var protos = GetFiles("./POGOProtos/src/**/*.proto");

    var protoRoot = MakeAbsolute(Directory("./POGOProtos/src"));

    foreach (var protoFile in protos)
    {
        Verbose("Compiling proto {0}", protoRoot.GetRelativePath(protoFile));

        StartProcess("./tools/Google.Protobuf.Tools/tools/windows_x64/protoc.exe", 
            new ProcessSettings()
                .WithArguments(args => 
                    args.Append("--csharp_out")
                        .AppendQuoted("src\\POGOProtos\\Protos")
                        .Append("--proto_path")
                        .AppendQuoted(protoRoot.ToString())
                        .AppendQuoted(protoFile.ToString())));
    }
});

Task("Build")
    //.IsDependentOn("Protos")
    .Does(() =>
{
    DotNetCorePack("./src/POGOProtos");
});


RunTarget(target);