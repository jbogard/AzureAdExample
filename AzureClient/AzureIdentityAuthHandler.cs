using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.Extensions.Options;
using Shared;

namespace AzureClient;

public class AzureIdentityAuthHandler<TClient> : DelegatingHandler
{
    private readonly TokenCredential _credential;
    private readonly ILogger<AzureIdentityAuthHandler<TClient>> _logger;
    private readonly AzureAdServerApiOptions<TClient> _options;

    public AzureIdentityAuthHandler(TokenCredential credential, 
        ILogger<AzureIdentityAuthHandler<TClient>> logger,
        IOptions<AzureAdServerApiOptions<TClient>> options)
    {
        _credential = credential;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var scopes = new[] { _options.ApplicationId + "/.default" };
        var tokenRequestContext = new TokenRequestContext(scopes);
        var result = await _credential.GetTokenAsync(
            tokenRequestContext, cancellationToken);

        _logger.LogInformation(result.Token);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}