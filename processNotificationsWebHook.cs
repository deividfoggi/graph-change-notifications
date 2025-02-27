
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using Newtonsoft.Json;

namespace Contoso.Function
{
    public class processNotificationsWebHook
    {
        private readonly ILogger<CreateSubscription> _logger;
        private GraphServiceClient _graphClient;

        public processNotificationsWebHook(ILogger<CreateSubscription> logger, GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
        }

        // create an additional function to handle the post from graph to the notification url
        [Function("resourceNotifications")]
        public async Task<IActionResult> HandleResourceNotification([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // get validation token
            var validationToken = req.Query["validationToken"];
            _logger.LogInformation("Validation token received: "+ validationToken);

            if(!string.IsNullOrEmpty(validationToken))
            {
                _logger.LogInformation("Validation token received: "+ validationToken + ". Returning token.");
                return new ContentResult { Content = validationToken, ContentType = "text/plain" };
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<GraphNotification>(requestBody);

            _logger.LogInformation(data.value.ToString());

            if (data != null && data.value != null)
            {
                foreach (var notification in data.value)
                {
                    // convert notification object to json string
                    var notificationJson = JsonConvert.SerializeObject(notification);
                    _logger.LogInformation("Notification received: { notification }", notificationJson);
                    // call GetCallRecord endpoint to get the call record
                    // var decodedResource = System.Web.HttpUtility.UrlDecode(notification.Resource);
                    // var resourceSegments = decodedResource?.Split('/');
                    // var callRecordId = resourceSegments?.Last();

                    // var callRecord = await _graphClient.Communications.CallRecords[callRecordId].GetAsync();

                    // if(callRecord != null)
                    // {
                    //     // get call record sessions
                    //     var callRecordJson = JsonConvert.SerializeObject(callRecord, Formatting.Indented);
                    //     _logger.LogInformation("Call record received: { callRecord }", callRecordJson);
                    //     SaveToFile.Save("{" + callRecord.Id + "}.json", callRecordJson);
                    // }

                    // var callRecordSessions = await _graphClient.Communications.CallRecords[callRecordId].GetAsync((RequestConfiguration) =>
                    // {
                    //     RequestConfiguration.QueryParameters.Expand = new string[] { "sessions($expand=segments)" };
                    // });

                    // if (callRecordSessions != null)
                    // {
                    //     var callRecordSessionsJson = JsonConvert.SerializeObject(callRecordSessions, Formatting.Indented);
                    //     _logger.LogInformation("Call record sessions received: { callRecordSessionsJson }", callRecordSessionsJson);
                    //     SaveToFile.Save("{" + callRecordSessions.Id + "}.json", callRecordSessionsJson);
                    // }
                }
            }
            
            return new OkResult();
        }
    }


    public class GraphNotification
    {
        public List<ChangeNotification> value { get; set; } = new List<ChangeNotification>();
    }

    // A class to save the content of a variable to a file in local disk
    public class SaveToFile
    {
        public static void Save(string fileName, string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(path, content);
        }
    }
}