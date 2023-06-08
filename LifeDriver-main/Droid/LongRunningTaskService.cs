using System;
using Android.App;
using Android.Content;
using Android.OS;
using StyexFleetManagement.Messages;
using StyexFleetManagement.Services;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using OperationCanceledException = Android.OS.OperationCanceledException;

namespace StyexFleetManagement.Droid
{
    [Service]
    public class LongRunningTaskService : Service
    {
        CancellationTokenSource _cts;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _cts = new CancellationTokenSource();

            Task.Run(() => {
                try
                {
                    //INVOKE THE SHARED CODE

                    Device.StartTimer(TimeSpan.FromSeconds(30), () => {
                        // If you want to update UI, make sure its on the on the main thread.
                        // Otherwise, you can remove the BeginInvokeOnMainThread
                        var message = new TickedMessage
                        {
                            Message = DateTime.Now.Millisecond.ToString()
                        };
                        MessagingCenter.Send<TickedMessage>(message, "TickedMessage");
                        return !_cts.IsCancellationRequested;
                    });
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (_cts.IsCancellationRequested)
                    {
                        var message = new CancelledMessage();
                        //Device.BeginInvokeOnMainThread(
                        //    () => 
                        MessagingCenter.Send(message, "CancelledMessage");
                        //);
                    }
                }

            }, _cts.Token);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Token.ThrowIfCancellationRequested();

                _cts.Cancel();
            }
            base.OnDestroy();
        }
    }
}