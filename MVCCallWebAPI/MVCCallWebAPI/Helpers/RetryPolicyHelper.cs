using Polly;
using Polly.Extensions.Http;

public static class RetryPolicyHelper
{
    public static IAsyncPolicy<HttpResponseMessage>
        GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt =>
                    TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt)
                    )
            );
    }
}