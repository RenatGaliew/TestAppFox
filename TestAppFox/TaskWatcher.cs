using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TestAppFox
{
    public static class TaskWatcher
    {
        public static void Watch(this Task task, string failMessage)
        {
            WatchImpl(task, failMessage);
        }

        [Conditional("DEBUG")]
        private static void WatchImpl(Task task, string failMessage)
        {
            task.ContinueWith(t => Debug.Assert(!t.IsFaulted, failMessage, t.Exception?.ToString() ?? "No exception!"),
                TaskContinuationOptions.ExecuteSynchronously);
        }

        public static void BreakIfFailed(this Task task)
        {
            BreakIfFailedImpl(task);
        }

        [Conditional("DEBUG")]
        private static void BreakIfFailedImpl(Task task)
        {
            if (!Debugger.IsAttached)
                return;

            task.ContinueWith(t =>
            {
                if (t.IsFaulted && Debugger.IsAttached)
                {
                    string message = t.Exception?.InnerExceptions.FirstOrDefault()?.Message;
                    Debug.WriteLine(message);
                    Debugger.Break();
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}