using StyexFleetManagement.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public class TaskCounter
    {
        public async Task RunCounter(CancellationToken token)
        {
            await Task.Run(async () => {
                var iterations = TimeSpan.FromMinutes(5).TotalMilliseconds / 10000;
                for (long i = 0; i < iterations; i++)
                {
                    token.ThrowIfCancellationRequested();

                    await Task.Delay(10000);
                    var message = new TickedMessage
                    {
                        Message = i.ToString()
                    };

                    //Device.BeginInvokeOnMainThread(() => {
                        MessagingCenter.Send<TickedMessage>(message, "TickedMessage");
                    //});
                }
            }, token);
        }
    }
}
