using Azure.Identity;
using Contoso.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

var _tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
var _clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
var _clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

string[] _scopes = new string[] { "https://graph.microsoft.com/.default" };

var options = new ClientSecretCredentialOptions
{
    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
};

var clientSecretCredential = new ClientSecretCredential(_tenantId, _clientId, _clientSecret, options);
var graphClient = new GraphServiceClient(clientSecretCredential, _scopes);

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(graphClient);
        services.AddSingleton<CreateWebHook>();
        services.AddSingleton<HandleResourceNotifications>();
        services.AddSingleton<HandleLifecycleNotifications>();
    })
    .Build();

host.Run();