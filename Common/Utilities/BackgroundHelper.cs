﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Common.Utilities
{
    public static class BackgroundHelper
    {
        public static void DoInBackground(Action backgroundAction, Action mainThreadAction)
        {
            DoInBackground(backgroundAction, mainThreadAction, 200);
        }
        public static void DoInBackground(Action backgroundAction, Action mainThreadAction, int milliseconds)
        {
            try
            {
#if SL
			    Dispatcher dispatcher = Dispatcher;
#else
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
#endif
                Thread thread = new Thread(delegate ()
                {
                    Thread.Sleep(milliseconds);
                    backgroundAction?.Invoke();
                    if (mainThreadAction != null)
                        dispatcher.BeginInvoke(mainThreadAction);
                })
                {
                    IsBackground = true
                };
                thread.TrySetApartmentState(ApartmentState.STA);
#if !SL
                thread.Priority = ThreadPriority.Lowest;
#endif
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }
        public static void DoWithDispatcher(Dispatcher dispatcher, Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Background)
        {
            try
            {
                if (dispatcher.CheckAccess())
                    action();
                else
                {
                    AutoResetEvent done = new AutoResetEvent(false);
                    dispatcher.BeginInvoke((Action)delegate ()
                    {
                        action();
                        done.Set();
                    }, dispatcherPriority);
                    done.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
        }
#if SL
		public static Dispatcher Dispatcher {
			get {
				return Deployment.Current.Dispatcher;
			}
		}
#endif


        public static Task PumpInvokeAsync(this Dispatcher dispatcher, Delegate action, params object[] args)
        {
            var completer = new TaskCompletionSource<bool>();

            // exit if we don't have a valid dispatcher
            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            {
                completer.TrySetResult(true);
                return completer.Task;
            }

            var threadFinished = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(async (o) =>
            {
                await dispatcher?.InvokeAsync(() =>
                {
                    action.DynamicInvoke(o as object[]);
                });
                threadFinished.Set();
                completer.TrySetResult(true);
            }, args);

            // The pumping of queued operations begins here.
            do
            {
                // Error condition checking
                if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                    break;

                try
                {
                    // Force the processing of the queue by pumping a new message at lower priority
                    dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                }
                catch
                {
                    break;
                }
            }
            while (threadFinished.WaitOne(1) == false);

            threadFinished.Dispose();
            threadFinished = null;
            return completer.Task;
        }
    }
}
