#load "./repository.cake"
#load "./paths.cake"
#load "./packages.cake"
#load "./version.cake"
#load "./releasenotes.cake"

public class BuildParameters
{
    public string Project{ get; private set; }
    public string NugetPackageName { get; private set; }
    public string Target { get; private set; }
    public string Configuration { get; private set; }
    public bool IsLocalBuild { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsMainCakeRepo { get; private set; }
    public bool IsMainCakeBranch { get; private set; }
    public bool IsDevelopCakeBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool IsPublishBuild { get; private set; }
    public bool IsReleaseBuild { get; private set; }
    public bool SkipGitVersion { get; private set; }
    public bool SkipOpenCover { get; private set; }
    public bool SkipSigning { get; private set; }

    //public BuildCredentials GitHub { get; private set; }
    //public CoverallsCredentials Coveralls { get; private set; }
    //public TwitterCredentials Twitter { get; private set; }
    //public GitterCredentials Gitter { get; private set; }
    public RepositoryPaths RepositoryPaths { get; private set; }
    public BuildReleaseNotes ReleaseNotes { get; private set; }
    public BuildVersion Version { get; private set; }
    public BuildPaths Paths { get; private set; }
    public BuildPackages Packages { get; private set; }
    public bool CanPublishNuGet { get; private set; }
    public bool IsNetFull { get; private set; }
    public bool IsWeb { get; private set; }

    public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainCakeRepo
                && IsMainCakeBranch && IsTagged;
        }
    }

    public bool ShouldPublishToMyGet
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainCakeRepo
                && (IsTagged || IsDevelopCakeBranch);
        }
    }

    public bool CanRelease
    {
        get
        {
            return
                ReleaseNotes.SemanticVersion == Version.SemVersion &&
                RepositoryPaths.Directories.LastReleasedVersion < Version.SemVersion;
        }
    }

    // public bool CanPostToTwitter
    // {
    //     get
    //     {
    //         return !string.IsNullOrEmpty(Twitter.ConsumerKey) &&
    //             !string.IsNullOrEmpty(Twitter.ConsumerSecret) &&
    //             !string.IsNullOrEmpty(Twitter.AccessToken) &&
    //             !string.IsNullOrEmpty(Twitter.AccessTokenSecret);
    //     }
    // }

    // public bool CanPostToGitter
    // {
    //     get
    //     {
    //         return !string.IsNullOrEmpty(Gitter.Token) && !string.IsNullOrEmpty(Gitter.RoomId);
    //     }
    // }

    public void Initialize(ICakeContext context)
    {
        RepositoryPaths = RepositoryPaths.GetPaths(context, this);
        Version = BuildVersion.Calculate(context, RepositoryPaths);
        Paths = BuildPaths.GetPaths(context, Project, Configuration, Version.SemVersion.ToString());
        ReleaseNotes = BuildReleaseNotes.LoadReleaseNotes(context, this);

        Packages = BuildPackages.GetPackages(
            Paths.Directories.NugetRoot,
            Version.SemVersion.ToString(),
            new [] { NugetPackageName });
    }

    public static BuildParameters GetParameters(ICakeContext context)
    {
        if (context == null) throw new ArgumentNullException("context");

        var project = context.Argument("appname", "");
        var nugetPackageName = context.Argument("nugetPackageName", project);
        var target = context.Argument("target", "Default");
        var buildSystem = context.BuildSystem();
        var isNetFull = context.Argument("netfull", "");
        var isWeb = context.Argument("web", "");

        var res = new BuildParameters {
            Project = project,
            NugetPackageName = nugetPackageName,
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsRunningOnUnix = context.IsRunningOnUnix(),
            IsRunningOnWindows = context.IsRunningOnWindows(),
            IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor,
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsMainCakeRepo = StringComparer.OrdinalIgnoreCase.Equals("cake-build/cake", buildSystem.AppVeyor.Environment.Repository.Name),
            IsMainCakeBranch = StringComparer.OrdinalIgnoreCase.Equals("main", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsDevelopCakeBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsTagged = IsBuildTagged(buildSystem),
            CanPublishNuGet = CheckCanPublishNuGet(context),
            IsNetFull = string.IsNullOrEmpty(isNetFull) == false,
            IsWeb = string.IsNullOrEmpty(isWeb) == false
        };

        return res;
    }

    private static bool IsBuildTagged(BuildSystem buildSystem)
    {
        return buildSystem.AppVeyor.Environment.Repository.Tag.IsTag
            && !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name);
    }

    private static bool IsReleasing(string target)
    {
        var targets = new [] { "Publish", "Publish-NuGet", "Publish-Chocolatey", "Publish-HomeBrew", "Publish-GitHub-Release" };
        return targets.Any(t => StringComparer.OrdinalIgnoreCase.Equals(t, target));
    }

    private static bool IsPublishing(string target)
    {
        var targets = new [] { "ReleaseNotes", "Create-Release-Notes" };
        return targets.Any(t => StringComparer.OrdinalIgnoreCase.Equals(t, target));
    }

    private static bool CheckCanPublishNuGet(ICakeContext context)
    {
        var apiKey = context.EnvironmentVariable("RELEASE_NUGETKEY");
        return string.IsNullOrEmpty(apiKey) == false;
    }
}
