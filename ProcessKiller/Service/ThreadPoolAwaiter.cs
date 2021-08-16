using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessKiller.Service
{
    public static class YieldAwaitableEx
    {
        public static ThreadPoolAwaitable ConfigureAwait(this YieldAwaitable _) => new();
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct ThreadPoolAwaitable
    {
        public ThreadPoolAwaiter GetAwaiter() => new();

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public readonly struct ThreadPoolAwaiter :
          ICriticalNotifyCompletion
        {
#nullable disable
            private static readonly WaitCallback __WaitCallbackRunAction = RunAction;
            private static readonly SendOrPostCallback s_sendOrPostCallbackRunAction = RunAction;

            public bool IsCompleted => false;
#nullable enable

            public void OnCompleted(Action continuation) => QueueContinuation(continuation, true);

            public void UnsafeOnCompleted(Action continuation) => QueueContinuation(continuation, false);


#nullable disable
            private static void QueueContinuation(Action continuation, bool FlowContext)
            {
                if (continuation is null) throw new ArgumentNullException(nameof(continuation));

                var scheduler = TaskScheduler.Current;
                if (scheduler == TaskScheduler.Default)
                {
                    if (FlowContext)
                        ThreadPool.QueueUserWorkItem(__WaitCallbackRunAction, continuation);
                    else
                        ThreadPool.UnsafeQueueUserWorkItem(__WaitCallbackRunAction, continuation);
                }
                else
                    Task.Factory.StartNew(continuation, new CancellationToken(), TaskCreationOptions.PreferFairness, scheduler);
            }

            private static void RunAction(object state) => ((Action)state)();

            public void GetResult() { }
        }
    }
}
