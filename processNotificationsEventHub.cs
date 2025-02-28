using System;
using Azure.Messaging.EventHubs;
using Contoso.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class processNotifications
    {
        private readonly ILogger<processNotifications> _logger;

        public processNotifications(ILogger<processNotifications> logger)
        {
            _logger = logger;
        }

        [Function(nameof(processNotifications))]
        public void Run([EventHubTrigger("graph-change-tracking-eh", Connection = "AzureWebJobsEventHubConnectionString")] string[] events)
        {
            foreach (string @event in events)
            {
                var data = JsonConvert.DeserializeObject<GraphNotification>(@event);
                if (data != null && data.value !=  null)
                {
                    foreach (var notification in data.value)
                    {
                        // convert notification object to json string
                        var notificationJson = JsonConvert.SerializeObject(notification);
                        _logger.LogInformation("Notification received: { notification }", notificationJson);
                    }
                }
            }
        }
    }
}
