using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Shared;

namespace ExternalClient;

public class ConfidentialClientApplicationAuthHandler<TClient> 
    : DelegatingHandler
{
    private readonly IConfidentialClientApplication _app;
    private readonly ILogger<ConfidentialClientApplicationAuthHandler<TClient>> _logger;
    private readonly AzureAdServerApiOptions<TClient> _options;

    public ConfidentialClientApplicationAuthHandler(IConfidentialClientApplication app, 
        ILogger<ConfidentialClientApplicationAuthHandler<TClient>> logger,
        IOptions<AzureAdServerApiOptions<TClient>> options)
    {
        _app = app;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var scopes = new[] { _options.ApplicationId + "/.default" };
        var result = await _app.AcquireTokenForClient(scopes)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation(result.AccessToken);

        request.Headers.Authorization 
            = new AuthenticationHeaderValue("Bearer", result.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}