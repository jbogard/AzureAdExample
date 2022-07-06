using System.Net.Http.Headers;
using Azure.Core;

namespace AzureClient;

public class AzureIdentityAuthHandler : DelegatingHandler
{
    private readonly TokenCredential _credential;
    private readonly ILogger<AzureIdentityAuthHandler> _logger;

    public AzureIdentityAuthHandler(TokenCredential credential, 
        ILogger<AzureIdentityAuthHandler> logger)
    {
        _credential = credential;
        _logger = logger;
    }

    public string ServerApplicationId { get; set; } = null!;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var scopes = new[] { ServerApplicationId + "/.default" };
        var tokenRequestContext = new TokenRequestContext(scopes);
        var result = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        _logger.LogInformation(result.Token);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}