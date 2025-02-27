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

        private string _webHookEndpointName;
        private string _changeType;
        private string _resource;
        private DateTimeOffset _expirationDateTime;

        public CreateWebHook(ILogger<CreateWebHook> logger, GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
        }

        [Function("CreateWebHook")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = requestBody != null ? JsonConvert.DeserializeObject(requestBody) : null;

            if (data?.webHookEndpointName != null)
            {
                _changeType = data.changeType;
                _webHookEndpointName = data.webHookEndpointName;
                _resource = data.resource;
                _expirationDateTime = DateTimeOffset.UtcNow.AddMinutes(data.expirationDateTime);
            }

            var subscription = new Subscription
            {
                ChangeType = _changeType,
                NotificationUrl = _webHookEndpointName,
                Resource = _resource,
                ExpirationDateTime = _expirationDateTime,
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