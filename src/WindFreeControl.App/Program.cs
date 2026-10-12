using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace WindFreeControl.App;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        if (RedirectToRunningInstance(args))
        {
            return 0;
        }

        Application.Start(callback =>
        {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            _ = new App();
        });
        return 0;
    }

    private static bool RedirectToRunningInstance(string[] args)
    {
        var key = args.Contains("--demo") ? "WindFreeControl.Demo" : "WindFreeControl";
        var instance = AppInstance.FindOrRegisterForKey(key);
        if (instance.IsCurrent)
        {
            instance.Activated += (_, _) => App.ShowRunningWindow();
            return false;
        }

        // RedirectActivationToAsync must not block the STA thread, so it runs on a worker thread.
        var activation = AppInstance.GetCurrent().GetActivatedEventArgs();
        var done = new ManualResetEventSlim();
        _ = Task.Run(async () =>
        {
            await instance.RedirectActivationToAsync(activation);
            done.Set();
        });
        done.Wait(TimeSpan.FromSeconds(5));
        return true;
    }
}
