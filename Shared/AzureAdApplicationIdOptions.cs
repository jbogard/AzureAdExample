namespace Shared;

public class AzureAdServerApiOptions<TClient>
{
    public string ApplicationId { get; set; } = null!;
    public string BaseAddress { get; set; } = null!;
}