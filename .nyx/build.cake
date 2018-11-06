#load "./build/parameters.cake";
#load "./build/cmd.cake";

#addin "nuget:https://www.nuget.org/api/v2?package=Cake.Git&version=0.19.0"
#addin "nuget:https://www.nuget.org/api/v2?package=Cake.SemVer&version=3.0.0"
#addin "nuget:https://www.nuget.org/api/v2?package=semver&version=2.0.4"

#tool "nuget:https://www.nuget.org/api/v2?package=GitVersion.CommandLine&version=4.0.0"

BuildParameters parameters = BuildParameters.GetParameters(Context);

Setup(context =>
{
    parameters.Initialize(context);

    Information("================================================================================================");
    Information("Building version {1} of {0} ({2}, {3}) using version {4} of Cake. (IsWeb: {5})",
        parameters.Project,
        parameters.Version.SemVersion,
        parameters.Configuration,
        parameters.Target,
        parameters.Version.CakeVersion,
        parameters.IsWeb,
        parameters.IsTagged);

    Information("------------------------------------------------------------------------------------------------");
    Information("   GitVersion version:\t{0}", parameters.Version.SemVersion);
    Information(" ReleaseNotes version:\t{0}", parameters.ReleaseNotes.SemanticVersion);
    Information("Last released version:\t{0}", parameters.RepositoryPaths.Directories.LastReleasedVersion);
    Information("================================================================================================");
});

var target = Argument("target", "Default");

Task("Clean").Does(() => CleanDirectories(parameters.Paths.Directories.ToClean));

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var projects = GetFiles("./src/**/*.csproj");
        foreach(var project in projects)
        {
            DotNetCoreRestore(project.FullPath, new DotNetCoreRestoreSettings());
        }
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .WithCriteria(() => parameters.IsNetFull == false)
    .Does(() =>
    {
        var path = MakeAbsolute(parameters.RepositoryPaths.Directories.CsProjFile);
        DotNetCoreBuild(path.FullPath, new DotNetCoreBuildSettings()
        {
            Configuration = parameters.Configuration,
            OutputDirectory = parameters.Paths.Directories.ArtifactsBinNetCoreApp,
            ArgumentCustomization = args => args
                .Append("/p:Version={0}", parameters.Version.SemVersion)
                .Append("/p:AssemblyVersion={0}", parameters.Version.Version)
                .Append("/p:FileVersion={0}", parameters.Version.Version)
                .Append("/p:SourceLinkCreate=true")
        });
    });

Task("Publish")
    .IsDependentOn("Build")
    .WithCriteria(() => parameters.IsWeb)
    .Does(() =>
    {
        var path = MakeAbsolute(parameters.RepositoryPaths.Directories.CsProjFile);
        DotNetCorePublish(path.FullPath, new DotNetCorePublishSettings()
        {
            Configuration = parameters.Configuration,
            OutputDirectory = parameters.Paths.Directories.ArtifactsBinNetCoreAppPublish,
            ArgumentCustomization = args => args
                .Append("/p:Version={0}", parameters.Version.SemVersion)
                .Append("/p:AssemblyVersion={0}", parameters.Version.Version)
                .Append("/p:FileVersion={0}", parameters.Version.Version)
                .Append("/p:SourceLinkCreate=true")
        });
    });

Task("Create-Lib-NuGet-Packages")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCorePack(parameters.RepositoryPaths.Directories.CsProjFile.ToString(), new DotNetCorePackSettings {
            Configuration = parameters.Configuration,
            OutputDirectory = parameters.Paths.Directories.NugetRoot,
            NoBuild = false,
            ArgumentCustomization = args => args
                .Append("/p:Version={0}", parameters.Version.SemVersion)
                .Append("/p:AssemblyVersion={0}", parameters.Version.Version)
                .Append("/p:FileVersion={0}", parameters.Version.Version)
                .Append("/p:SourceLinkCreate=true")
        });
    });

Task("Create-Web-NuGet-Packages")
    .IsDependentOn("Publish")
    .WithCriteria(() => parameters.IsWeb)
    .Does(() =>
    {
        var deployment = MakeAbsolute(parameters.RepositoryPaths.Directories.DeploymentPath).FullPath;
        var files = new DirectoryInfo(deployment).GetFiles().Select(f=> new NuSpecContent {Source = f.FullName, Target = "tools"}).ToList();
        files.Add(new NuSpecContent {Source = "**", Target = "content"});

        NuGetPack(new NuGetPackSettings {
                                     Id                      = parameters.NugetPackageName,
                                     Version                 = parameters.Version.SemVersion.ToString(),
                                     Title                   = parameters.NugetPackageName,
                                     Description             = "The description of the package",
                                     Authors                 = new[] {"John Doe"},
                                     RequireLicenseAcceptance= false,
                                     Symbols                 = false,
                                     NoPackageAnalysis       = true,
                                     Files                   = files.ToArray(),
                                     BasePath                = parameters.Paths.Directories.ArtifactsBinNetCoreAppPublish,
                                     OutputDirectory         = parameters.Paths.Directories.NugetRoot
                                 });
    });

Task("Pack")
    .IsDependentOn("Create-Lib-NuGet-Packages")
    .IsDependentOn("Create-Web-NuGet-Packages")
    .Does(() =>
    {
        Information("Packing...");
    });

Task("Release")
    .IsDependentOn("Pack")
    .WithCriteria(context => parameters.CanRelease)
    .WithCriteria(context => parameters.CanPublishNuGet)
    .Does(context =>
    {
        var apiKey = EnvironmentVariable("RELEASE_NUGETKEY");
        if(string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Could not resolve NuGet API key from EnvVar=RELEASE_NUGETKEY.");

        var apiUrl = EnvironmentVariable("nugetserver");
        if(string.IsNullOrEmpty(apiUrl))
            apiUrl = "https://www.nuget.org/api/v2/package";

        var pkg = parameters.Packages.All.First();
        NuGetPush(pkg.PackagePath, new NuGetPushSettings {
            ApiKey = apiKey,
            Source = apiUrl
        });

        string tag = parameters.NugetPackageName + "@" + parameters.Version.SemVersion;
        GitTag("../.", tag);
        Cmd.ExecuteCommand(context, "git push --tags");
    });

Task("Default").IsDependentOn("Release");

RunTarget(target);
