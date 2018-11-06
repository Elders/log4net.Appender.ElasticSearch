public static class Cmd
{
    public static string ExecuteCommand(ICakeContext context, string command)
    {
        string output = "";
        context.Information(command);

        System.Diagnostics.ProcessStartInfo processInfo;
        System.Diagnostics.Process process;

        processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        // *** Redirect the output ***
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        process = System.Diagnostics.Process.Start(processInfo);
        process.OutputDataReceived += ((object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            if (!String.IsNullOrEmpty(e.Data))
                output = e.Data;
        });

        process.BeginOutputReadLine();

        process.ErrorDataReceived += ((object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            if (!String.IsNullOrEmpty(e.Data))
                context.Information("[error]" + e.Data);
        });
        process.BeginErrorReadLine();
        process.WaitForExit();

        context.Information("ExitCode: " + process.ExitCode);
        process.Close();

        context.Information("Output is: " + output);
        return output;
    }
}

