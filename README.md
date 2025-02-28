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

4. **Deploy the infrastructure**

    Adjust file azure/terraform.tfvars with the desired region if needed and run the following commands from azure directory:

    ```
    terraform init
    terraform plan --var-file=terraform.tfvars
    terraform apply -var-file=terraform.tfvars

5. **Set environment variables in the app service:**

    1. Go to Azure Portal, open the resource group with name graphchangetracking-<uniqueid>-rg
    2. Open the Event Hub namespace and copy the hostname value in the Overview page

## Running the Project

1. **Build the project:**

    Open the terminal in Visual Studio Code and run:

    ```sh
    dotnet build
    ```

2. **Run the Azure Functions locally:**

    Press `F5` or run the following command in the terminal:

    ```sh
    func start
    ```

    This will start the Azure Functions runtime and your functions will be available locally.

## Testing the Functions

### CreateWebHook Function

1. **Trigger the function:**

    You can trigger the `CreateWebHook` function by sending an HTTP POST request to `<forwarded address>/api/CreateWebHook`.

    Example of the json payload:

    ```json
    {
        "changeType": "created",
        "webHookEndpointName": "https://qsr0nd8n-7071.brs.devtunnels.ms/",
        "resource" = "communications/onlineMeetings/getAllRecordings",
        "expirationDateTime": "4230"
    }
    ```

2. **Check the logs:**

    The function logs will be displayed in the terminal where you started the Azure Functions runtime.

### HandleCallRecords Function

1. **Trigger the function:**

    Considering the previous step used the CreateWebHook function to subscribe to get all meeting recordings, create a teams meeting the same tenant the application registration was created, start meeting recording, await for 5 ou 10 seconds and stop the recording.

2. **Check the logs:**

    The function logs will be displayed in the terminal when this function receives the notification that a new meeting recording is available.

## Deployment

To deploy the Azure Functions to Azure, follow the instructions in the [Azure Functions documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code?tabs=csharp).

## License

This project is licensed under the MIT License. See the LICENSE file for details.
