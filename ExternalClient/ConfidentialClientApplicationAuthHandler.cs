using System.Net.Http.Headers;
using Microsoft.Identity.Client;

namespace ExternalClient;

public class ConfidentialClientApplicationAuthHandler : DelegatingHandler
{
    private readonly IConfidentialClientApplication _app;
    private readonly ILogger<ConfidentialClientApplicationAuthHandler> _logger;

    public ConfidentialClientApplicationAuthHandler(IConfidentialClientApplication app, ILogger<ConfidentialClientApplicationAuthHandler> logger)
    {
        _app = app;
        _logger = logger;
    }

    public string ServerApplicationId { get; set; } = null!;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scopes = new[] { ServerApplicationId + "/.default" };
        var result = await _app.AcquireTokenForClient(scopes)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation(result.AccessToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}