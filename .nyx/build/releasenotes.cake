public class BuildReleaseNotes
{
    public Semver.SemVersion SemanticVersion { get; private set; }

    public static BuildReleaseNotes LoadReleaseNotes(ICakeContext context, BuildParameters parameters)
    {
        var rn = new BuildReleaseNotes();
        try
        {
            var releaseNotesFile = parameters.RepositoryPaths.Directories.CsProjPath.Combine(parameters.NugetPackageName + ".rn.md");
            context.Information("Loading ReleaseNotes from {0}", releaseNotesFile);

            var last = context.ParseReleaseNotes(releaseNotesFile.ToString());

            string pattern = @"(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?";
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            var result = regex.Match(last.RawVersionLine);
            if(result.Success)
            {
                rn.SemanticVersion = context.ParseSemVer(result.Value, true);
            }
            else
            {
                context.Information("Error loading ReleaseNotes version!");
                context.Information("Last ReleaseNotes {0}", last);
                context.Information("RawVersionLine: {0}", last.RawVersionLine);
            }
            return rn;
        }
        catch(Exception ex)
        {
            context.Error(ex);
            return rn;
        }
    }
}
