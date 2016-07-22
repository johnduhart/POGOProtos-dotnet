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

Task("Protos")
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
    .IsDependentOn("Protos")
    .Does(() =>
{
    var projectPath = "./src/POGOProtos";

    DotNetCoreRestore(projectPath);

    var buildSettings = new DotNetCorePackSettings();

    if (AppVeyor.IsRunningOnAppVeyor && AppVeyor.Environment.Repository.Branch.Contains("develop"))
    {
        var versionNumber = AppVeyor.Environment.Build.Number;
        buildSettings.VersionSuffix = string.Format("beta{0:0000}", versionNumber);
    }
    
    DotNetCorePack(projectPath, buildSettings);
});


RunTarget(target);