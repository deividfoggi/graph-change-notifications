variable "subscription_id" {
  description = "The subscription ID"
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The location of the resource group"
  type        = string
}

variable "event_hub_namespace_name" {
  description = "The name of the Event Hub namespace"
  type        = string
}

variable "event_hub_namespace_sku" {
  description = "The SKU of the Event Hub namespace"
  type        = string
}

variable "event_hub_namespace_capacity" {
  description = "The capacity of the Event Hub namespace"
  type        = number
}

variable "event_hub_name" {
  description = "The name of the Event Hub"
  type        = string
}

variable "event_hub_partition_count" {
  description = "The number of partitions for the Event Hub"
  type        = number
}

variable "event_hub_message_retention" {
  description = "The message retention period for the Event Hub"
  type        = number
}

variable "event_hub_authorization_rule_name" {
  description = "The name of the Event Hub authorization rule"
  type        = string
}

variable "microsoft_garph_change_tracking_principal_id" {
  description = "The principal ID for Microsoft Graph Change Tracking"
  type        = string
  default = "5ef35de7-93ff-4cbd-86bd-27b5d045f859"
}

variable "storage_account_tier" {
  description = "The tier of the storage account"
  type        = string
}

variable "storage_account_replication_type" {
  description = "The replication type of the storage account"
  type        = string
}