using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Contoso.Function
{
    public class processLifecyleNotificationsWebHook
    {
        private readonly ILogger<CreateSubscription> _logger;
        private GraphServiceClient _graphClient;
        private readonly HttpClient _httpClient;

        public processLifecyleNotificationsWebHook(ILogger<CreateSubscription> logger, GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
            _httpClient = new HttpClient();
        }

        // create an additional function to handle the post from graph to the notification url
        [Function("lifecycleNotifications")]
        public async Task<IActionResult> HandleLifecycleNotification([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // get validation token
            var validationToken = req.Query["validationToken"];

            if(!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation("Validation token received: "+ validationToken + " .Returning token.");
                return new ContentResult { Content = validationToken, ContentType = "text/plain" };
            } 

            // parse request body from req to a json
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<GraphNotification>(requestBody);

            if (data != null && data.value != null)
            {
                _logger.LogInformation(JsonConvert.SerializeObject(data.value));
            }

            var url = "https://graph.microsoft.com/v1.0/subscriptions/{subscriptionId}/reauthorize";
            var content = new StringContent("");
            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Reauthorization request sent successfully.");
            }
            else
            {
                _logger.LogError("Failed to send reauthorization request. Status code: {statusCode}", response.StatusCode);
            }
            return new OkResult();
        }
    }
}