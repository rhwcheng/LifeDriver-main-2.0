using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StyexFleetManagement.MZone.Client.Models;

namespace StyexFleetManagement.MZone.Client
{
    public class BaseClient : IBaseClient
    {
        public const string BaseUrl = "https://live.mzoneweb.net/mzone61.api";

        public Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, string>
            {
                {"grant_type", "password"},
                {"username", "rhw"},
                {"password", "P45t1234!"},
                {"client_id", "mz-styex"},
                {"client_secret", "Syk-3m*P6-tz7ttd3efYTPPh"}
            };

            var request = new HttpRequestMessage();
            request.Headers.Clear();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsImtpZCI6IkVGMUUxMkVFOTQ1NTdBNDg5MzlCMUJBNjJFQUUxQzFBN0ZDNTY2MkQiLCJ0eXAiOiJKV1QiLCJ4NXQiOiI3eDRTN3BSVmVraVRteHVtTHE0Y0duX0ZaaTAifQ.eyJuYmYiOjE2MDQyMTQ3NjYsImV4cCI6MTYwNDIxODM2NiwiaXNzIjoiaHR0cHM6Ly9sb2dpbi5tem9uZXdlYi5uZXQiLCJhdWQiOlsiaHR0cHM6Ly9sb2dpbi5tem9uZXdlYi5uZXQvcmVzb3VyY2VzIiwibXo2LWFwaSJdLCJjbGllbnRfaWQiOiJtei1zdHlleCIsInN1YiI6IjBjYmZhNmRkLTQ3YTItNDU4OC04MDIyLWE4Y2EzYjU1N2U3OCIsImF1dGhfdGltZSI6MTYwNDIxNDc2NiwiaWRwIjoibG9jYWwiLCJtel91c2VybmFtZSI6InJodyIsIm16X3VzZXJncm91cF9pZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsIm16X3NoYXJkX2NvZGUiOiJFTUVBIiwic2NvcGUiOlsibXpfdXNlcm5hbWUiLCJvcGVuaWQiLCJtejYtYXBpLmFsbCJdLCJhbXIiOlsicHdkIl19.aIep8GHdL8iOwzjsWl4OFf_92OOVtTcK-8uJ-CpMOWV5gZ5Ne6nXjaHZaaenBy1_05Jv4EDj19LDMYt1B4GDVg704q6cup7KnUpJZHMnSuLxD7rg_opaAqIgI80Un6cmcFPssFITm0opsa5n_O9TojyDCwJYJdXOqFJjmqp3bv9QDw1yaE6phJbuD3dXKbZBowykluEW20scXTrQbWpXuEbOm_T7Ewgi7msCIar6NTCM-zcOjtIRU03lwX4xWs_--LKJHdTlKyC3t9jip5QM5hLtvZq9S38j0oQGaSk7v8lil64ccJs95eYCml_BZ3g4Irf3kqZtLcdIMMwpX3flDWVMU3cnKYVersG9J3R1ThjAGAuesB2q_ibVU6ViNgd6uR7yjqUWEYUKZJpt4wdwfDLHHmJc41oF0shZxCNjLhjbD73kA8Bqd_pQbMMH1lf8HqAR5ZcKRPq7hl6ZF38bPBxjdYDPxP78MyFpKiBtAoESkiW7R5UZPATVTDviysW86Ou0A_7cSPl1k07JqEspzbgi81Z7phykGqythzmzO5PIQEiaIxquwOm8wqkOupiEIyVOvsEykO8BqceLaYjYHSOqpxCzNn7Aa8zmIlJNVWHFdBwC9YH2qM3Vl-YKr1lsMOQ5LUfSBp99Zj9o1sSZ09YYf6JKe_Awa8FolR3Mt-A");

            return Task.FromResult(request);
        }
    }
}