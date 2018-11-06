public class RepositoryPaths
{
    public RepositoryDirectories Directories { get; private set; }

    public static RepositoryPaths GetPaths(ICakeContext context, BuildParameters parameters)
    {
        if (context == null) throw new ArgumentNullException("context");

        var srcDir = (DirectoryPath)context.Directory("../src");
        var csProjPath = srcDir.Combine(parameters.Project);
        var deploymentPath = csProjPath.Combine("deployment");
        var csProjFile = csProjPath.Combine(parameters.Project + ".csproj");

        string globPattern = parameters.NugetPackageName + "@" + "*[0-9].*[0-9].*[0-9]*";
        var lastGitTag = Cmd.ExecuteCommand(context, "git describe --tags --match " + globPattern);

        var lastGitTagVersion = lastGitTag.Replace(parameters.NugetPackageName + "@", string.Empty);
        var lastReleasedVersion = context.ParseSemVer(lastGitTagVersion, true);

        return new RepositoryPaths
        {
            Directories = new RepositoryDirectories(csProjPath, csProjFile, deploymentPath, lastGitTag, lastReleasedVersion)
        };
    }
}

public class RepositoryDirectories
{
    public RepositoryDirectories(DirectoryPath csProjPath, DirectoryPath csProjFile, DirectoryPath deploymentPath, string lastGitTag, Semver.SemVersion lastReleasedVersion)
    {
        CsProjPath = csProjPath;
        CsProjFile = csProjFile;
        DeploymentPath = deploymentPath;
        LastGitTag = lastGitTag;
        LastReleasedVersion = lastReleasedVersion;
    }

    public DirectoryPath CsProjPath { get; private set; }
    public DirectoryPath CsProjFile { get; private set; }
    public DirectoryPath DeploymentPath { get; private set; }
    public string LastGitTag { get; private set; }
    public Semver.SemVersion LastReleasedVersion { get; private set; }
}
