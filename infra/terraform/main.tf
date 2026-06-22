terraform {
  required_version = ">= 1.8.0"
}

variable "project_name" {
  type    = string
  default = "platform-app"
}

variable "location" {
  type    = string
  default = "eastus"
}

resource "null_resource" "forward_engineering_baseline" {
  triggers = {
    project_name = var.project_name
    location     = var.location
  }
}
