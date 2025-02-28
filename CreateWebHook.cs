using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Contoso.Function
{
    public class CreateWebHook
    {
        private readonly ILogger<CreateWebHook> _logger;

        private readonly GraphServiceClient _graphClient;
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
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var subscription = new Subscription
            {
                ChangeType = data.changeType,
                NotificationUrl = data.notificationUrl,
                Resource = data.resource,
                ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes((double)data.expirationDateTime),
                LifecycleNotificationUrl = data.lifecycleNotificationUrl,
                ClientState = "SecretClientState"
            };

            try
            {
                var newSubscription = await _graphClient.Subscriptions.PostAsync(subscription);
                return new ObjectResult(newSubscription) { StatusCode = StatusCodes.Status200OK };
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"ServiceException creating subscription: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (ODataError ex)
            {
                _logger.LogError($"Error creating subscription: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}