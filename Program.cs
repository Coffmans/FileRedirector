using FileRedirector.Services;
using FileRedirector.UI;

namespace FileRedirector;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Last-chance handlers: log everything; keep running after UI-thread errors
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowError("An unexpected error occurred.", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLog.Error($"Unhandled exception (terminating: {e.IsTerminating})", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppLog.Error("Unobserved task exception", e.Exception);
            e.SetObserved();
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        AppLog.PruneOldLogs();
        AppLog.Info("FileRedirector starting.");

        MainForm form;
        try
        {
            form = new MainForm();
        }
        catch (Exception ex)
        {
            ShowError("FileRedirector could not start.", ex);
            return;
        }

        Application.Run(form);
        AppLog.Info("FileRedirector exited.");
    }

    private static void ShowError(string message, Exception ex)
    {
        AppLog.Error(message, ex);
        MessageBox.Show($"{message}\n\n{ex.Message}\n\nDetails were written to the log in:\n{AppLog.LogDirectory}",
            "FileRedirector", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
