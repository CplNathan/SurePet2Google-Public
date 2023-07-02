using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Contrib.DuplicateRequestCollapser;
using System.Net;

namespace SurePet2Google.Blazor.Server.Context
{
    public class GlobalHttpContext : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var policy = new PolicyHttpMessageHandler(BuildRetryPolicy());
            policy.InnerHandler = base.CreateMessageHandler();

            return policy;
        }

        private static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy()
        {
            var retryPolicy =
                AsyncRequestCollapserPolicy.Create().WrapAsync(
                    Policy
                   .Handle((FlurlHttpException exception) => new List<int>() { (int)HttpStatusCode.TooManyRequests, (int)HttpStatusCode.BadRequest }.Contains(exception.StatusCode ?? (int)HttpStatusCode.BadRequest))
                   .WaitAndRetryAsync(5, retryAttempt =>
                   {
                       var nextAttemptIn = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                       Console.WriteLine($"Retry attempt {retryAttempt} to make request. Next try on {nextAttemptIn.TotalSeconds} seconds.");
                       return nextAttemptIn;
                   })
               ).AsAsyncPolicy<HttpResponseMessage>();

            return retryPolicy;
        }
    }
}
