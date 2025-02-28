// define azure provider
provider "azurerm" {
    features {}
    subscription_id = var.subscription_id
}

// define a unique string with lowercase letters and numbers, without any special characters
resource "random_string" "unique_string" {
  length  = 16
  upper   = false
  special = false
}

//define a local variable joining unique_string and resource_group_name
locals {
  unique_string = random_string.unique_string.result
  resource_group_name = "${var.resource_group_name}-${local.unique_string}-rg"
}

// define resource group
resource "azurerm_resource_group" "resource_group" {
  name     = local.resource_group_name
  location = var.location
}

// define the event hub
resource "azurerm_eventhub" "eventhub" {
  name                = var.event_hub_name
  namespace_id        = azurerm_eventhub_namespace.eventhub_namespace.id
  partition_count     = var.event_hub_partition_count
  message_retention   = var.event_hub_message_retention

  depends_on = [ azurerm_eventhub_namespace.eventhub_namespace ]
}


// define the event hubs resource
resource "azurerm_eventhub_namespace" "eventhub_namespace" {
  name                = var.event_hub_namespace_name
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  sku                 = var.event_hub_namespace_sku
  capacity            = var.event_hub_namespace_capacity

}

// add a role assingmnet to the event hub namespace with role Azure Event Hubs Data Sender for Microsoft Graph Change Tracking
resource "azurerm_role_assignment" "eventhub_role_assignment" {
  principal_id   = var.microsoft_garph_change_tracking_principal_id
  role_definition_name = "Azure Event Hubs Data Sender"
  scope          = azurerm_eventhub_namespace.eventhub_namespace.id

  depends_on = [ azurerm_eventhub_namespace.eventhub_namespace ]
}

// define a storage account
resource "azurerm_storage_account" "storage_account" {
  name                     = "${local.unique_string}storage"
  resource_group_name      = azurerm_resource_group.resource_group.name
  location                 = azurerm_resource_group.resource_group.location
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
}

// define the app service plan
resource "azurerm_service_plan" "service_plan" {
  name                = "${local.unique_string}-asp"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  os_type             = "Linux"
  sku_name            = "B1"
}

// define an azure function
resource "azurerm_linux_function_app" "function_app" {
  name                       = "${local.unique_string}-function-app"
  location                   = azurerm_resource_group.resource_group.location
  resource_group_name        = azurerm_resource_group.resource_group.name
  service_plan_id            = azurerm_service_plan.service_plan.id
  storage_account_name       = "${local.unique_string}storage"
  storage_account_access_key = azurerm_storage_account.storage_account.primary_access_key

  site_config { }

  app_settings = {
    "AzureWebJobsStorage" = azurerm_storage_account.storage_account.primary_connection_string
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet-isolated"
    "AzureWebJobsEventHubConnectionString" = azurerm_eventhub_namespace.eventhub_namespace.default_primary_connection_string
    "WEBHOOK_ENDPOINT_NAME" = "paste_function_app_endpoint_name_here/api/processNotificationsWebHook"
    "CLIENT_ID" = "paste_client_id_here"
    "TENANT_ID" = "paste_tenant_id_here"
    "CLIENT_SECRET" = "paste_client_secret_here"
  }
  
  identity {
    type = "SystemAssigned"
  }

  depends_on = [ azurerm_service_plan.service_plan, azurerm_storage_account.storage_account ]
}