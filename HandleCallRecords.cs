using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Contoso.Function
{
    public class HandleCallRecords
    {
        private readonly ILogger<CreateWebHook> _logger;
        private GraphServiceClient _graphClient;

        public HandleCallRecords(ILogger<CreateWebHook> logger, GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
        }

        // create an additional function to handle the post from graph to the notification url
        [Function("ProcessNotification")]
        public async Task<IActionResult> HandleNotification([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // get validation token
            var validationToken = req.Query["validationToken"];

            if(!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation("Validation token received: "+ validationToken + " .Returning token.");
                return new ContentResult { Content = validationToken, ContentType = "text/plain", StatusCode = 200 };
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<GraphNotification>(requestBody);

            foreach (var notification in data.value)
            {
                // convert notification object to json string
                var notificationJson = JsonConvert.SerializeObject(notification);
                _logger.LogInformation("Notification received: { notification }", notificationJson);
            }
            
            return new OkResult();
        }
    }

    public class GraphNotification
    {
        public List<ChangeNotification> value { get; set; } = new List<ChangeNotification>();
    }
}