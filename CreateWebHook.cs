using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Contoso.Function
{
    public class CreateWebHook
    {
        private readonly ILogger<CreateWebHook> _logger;

        private readonly GraphServiceClient _graphClient;

        private readonly string _webHookEndpointName;

        public CreateWebHook(ILogger<CreateWebHook> logger, GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
            _webHookEndpointName = "https://"+ Environment.GetEnvironmentVariable("WEBHOOK_ENDPOINT_NAME") +"/api/ProcessNotification";
        }

        [Function("CreateWebHook")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var subscription = new Subscription
            {
                ChangeType = "created",
                NotificationUrl = _webHookEndpointName,
                Resource = "/communications/callRecords",
                ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(4230), // Set expiration to 3 days from now
                ClientState = "SecretClientState"
            };

            try
            {
                var newSubscription = await _graphClient.Subscriptions.PostAsync(subscription);
                return new OkObjectResult(newSubscription);
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error creating subscription: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}