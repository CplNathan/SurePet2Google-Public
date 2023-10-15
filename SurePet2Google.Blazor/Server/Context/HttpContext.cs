using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Http;
using Polly;
using System.Net;

namespace SurePet2Google.Blazor.Server.Context
{
    public class GlobalHttpContext : FlurlClientFactoryBase
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var policy = new PolicyHttpMessageHandler(BuildRetryPolicy());
            policy.InnerHandler = base.CreateMessageHandler();

            return policy;
        }

        public static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy()
        {
            var retryPolicy =
                Policy
                .Handle<Exception>((exception) =>
                {
                    int? statusCode = -1;

                    if (exception is FlurlHttpException flurlException)
                    {
                        statusCode = flurlException.StatusCode;
                    }
                    else if (exception is HttpRequestException httpException)
                    {
                        statusCode = (int?)httpException.StatusCode;
                    }

                    return new List<int>() { (int)HttpStatusCode.TooManyRequests, (int)HttpStatusCode.BadRequest, (int)HttpStatusCode.GatewayTimeout }.Contains(statusCode ?? (int)HttpStatusCode.BadRequest);
                })
                .WaitAndRetryAsync(5, retryAttempt =>
                {
                    var nextAttemptIn = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    Console.WriteLine($"Retry attempt {retryAttempt} to make request. Next try on {nextAttemptIn.TotalSeconds} seconds.");
                    return nextAttemptIn;
                }).AsAsyncPolicy<HttpResponseMessage>();

            return retryPolicy;
        }

        protected override IFlurlClient Create(Url url)
        {
            var client = base.Create(url);
            return client;
        }

        protected override string GetCacheKey(Url url)
        {
            return "StaticCache";
        }
    }
}
