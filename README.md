# Receive change notifications from Microsoft Graph

This project implements two scenarios to process change notifications from Microsoft Graph:

- WebHook: https://learn.microsoft.com/en-us/graph/change-notifications-delivery-webhooks?tabs=http
- EventHubs: https://learn.microsoft.com/en-us/graph/change-notifications-delivery-event-hubs?tabs=change-notifications-eventhubs-azure-portal-rbac%2Cchange-notifications-eventhubs-rbac%2Chttp

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Azure Functions Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- Azure Subscription
- Terraform

## Setup

1. **Clone the repository:**

    ```sh
    git clone <repository-url>
    cd receive-change-notifications-graph
    ```

2. **Open the project in Visual Studio Code:**

    ```sh
    code .
    ```

3. **Create an Application Registration in Entra**

    The application registration should have the Microsoft Graph Application permission OnlineMeetings.Read.All. Take a note of the following values:

    - Tenant ID
    - Client ID
    - Create a client secret
    - Add Microsoft Graph API Permission accordingly to the resource you would like to subscribe, for instance

4. **Deploy the infrastructure**

    Adjust file azure/terraform.tfvars with the desired region if needed and run the following commands from azure directory:

    ```
    cd azure
    terraform init
    terraform plan --var-file=terraform.tfvars
    terraform apply -var-file=terraform.tfvars

5. **Set environment variables in the app service:**

    1. Go to Azure Portal, open the resource group with name graphchangetracking-<uniqueid>-rg
    2. Open the Event Hub namespace and copy the hostname value in the Overview page
    3. Open the function app and acess Environment Variables page to set the following variables:

        "AzureWebJobsStorage": "<storage account connection string>",
        "CLIENT_ID": "<application registration client id>",
        "TENANT_ID": "<application registration tenant id>",
        "CLIENT_SECRET": "<application registration client secret>",
        "WEBHOOK_ENDPOINT_NAME": "https://qsr0nd8n-7071.brs.devtunnels.ms/api/ProcessNotification",
        "AzureWebJobsEventHubConnectionString": "<event hubs connection string>"

## Running the Project

1. **Build the project:**

    Open the terminal in Visual Studio Code and run:

    ```sh
    dotnet build
    ```

2. **Run the Azure Functions locally:**

    Press `F5`

    This will start the Azure Functions runtime and your functions will be available locally.

3. **Forward a public endpoint**

    In VSCode, use PORTS menu to forward port 7071. Change the forwarded address to public visibility (right click on the address in the list) and copy the address

## Testing the Functions

### CreateSubscription Function

1. **Trigger the function:**

    You can trigger the `CreateSubscription` function by sending an HTTP POST request to `https://<forwarded address>/api/processNotification`.

    Example of the json payload:

    **ATTENTION! You can get the domain name from Entra ID's Overview page**.

    ```json
    {
    "changeType": "created",
    "notificationUrl": "EventHub:https://<eventhub hostname>/eventhubname/<eventhub name>?tenantId=<domain name>",
    "lifecycleNotificationUrl": "https://<forwarded local address>/api/lifecycleNotifications",
    "resource": "communications/onlineMeetings/getAllRecordings",
    "includeResourceData": true,
    "expirationDateTime": "4230"
    }
    ```

2. **Check the logs:**

    The function logs will be displayed in the terminal where you started the Azure Functions runtime.

### Handle the subscribed notification

1. **Trigger the function:**

    Considering the previous step used to create the web hook, you have subscribed for any new Teams Meeting record. You can fire notifications using the following step:

    - Open Teams using any user in the tenant you subscribed to (same tenant of the application registration)
    - Schedule a meeting
    - Join the meeting
    - Start meeting recording
    - Stop meeting recording
    - End the meeting (MAKE SURE YOU REALLY ENDED THE MEETING, DO NOT JUST LEAVE IT, YOU MUST END IT)
    - To fire more events, join again, start and stop recording and end the meeting. Every time you do it, even using the same meeting, it fires a new notification.

2. **Check the logs:**

    The function logs will be displayed in the terminal when this function receives the notification that a new meeting recording is available.

## Deployment

To deploy the Azure Functions to Azure, Open Azure extension > Workspace > right click on Local Project folder > Deploy to Azure.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
