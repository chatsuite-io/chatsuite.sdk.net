# ChatSuite SDK

ChatSuite SDK is a .NET library designed to facilitate the development of chat-based applications. This SDK provides various utilities and integrations to streamline the development process.

## Target Framework

This project targets **.NET 9.0**.

## Features

- Implicit Usings enabled
- Nullable reference types enabled
- Latest C# language version

## Dependencies

The project relies on several NuGet packages to provide additional functionality:

- **DBreeze**: A fast and reliable embedded database.
- **FluentValidation**: A popular .NET library for building strongly-typed validation rules.
- **Microsoft.AspNetCore.SignalR.Client**: A client library for ASP.NET Core SignalR.
- **Microsoft.AspNetCore.SignalR.Client.Core**: Core components for SignalR client.
- **Microsoft.Extensions.Http**: Extensions for HTTP client.
- **Microsoft.Identity.Client**: Microsoft Authentication Library (MSAL) for .NET.
- **Microsoft.Azure.Functions.Extensions**: Extensions for Azure Functions.
- **Serilog.Extensions.Hosting**: Serilog integration with .NET Generic Host.
- **Microsoft.Extensions.Logging.Abstractions**: Abstractions for logging.
- **Newtonsoft.Json**: Popular high-performance JSON framework for .NET.
- **Serilog**: Simple .NET logging with fully-structured events.
- **Serilog.Settings.Configuration**: Serilog settings from configuration sources.
- **Serilog.Extensions.Logging**: Serilog logging extensions.
- **Serilog.Sinks.Console**: Serilog sink for console output.
- **Serilog.Sinks.ApplicationInsights**: Serilog sink for Azure Application Insights.
- **Polly**: A resilience and transient-fault-handling library.
- **Microsoft.Extensions.Http.Polly**: Polly extensions for HTTP client.
- **System.Text.Json**: High-performance JSON APIs for .NET.

## Client.cs Overview

The `Client.cs` file is responsible for managing the connection to the SignalR hub and handling various chat-related functionalities. It includes methods for connecting, sending messages, and managing secure groups.

### Key Methods

- `Build()`: Initializes the SignalR hub connection.
- `ConnectAsync()`: Connects to the SignalR hub.
- `SendMessageToUserAsync()`: Sends a message to a specific user.
- `SendEncryptedMessageToUserAsync()`: Sends an encrypted message to a specific user.
- `SendMessageToGroupAsync()`: Sends a message to a group.
- `AddUserToGroupAsync()`: Adds a user to a group.
- `RemoveUserFromGroupAsync()`: Removes a user from a group.
- `ReportStatusToUserAsync()`: Reports status to a specific user.
- `ReportStatusToGroupAsync()`: Reports status to a group.
- `Dispose()`: Disposes the client and its resources.

### Events

- `Closed`: Triggered when the connection is closed.
- `Reconnected`: Triggered when the connection is re-established.
- `Reconnecting`: Triggered when the connection is in the process of reconnecting.

### Internal Methods

- `RequestPublicKeyAsync()`: Requests the public key of another user.
- `IsUserOnlineAsync()`: Checks if another user is online.
- `ConcludePublicKeyAsync()`: Concludes the public key of another user.
- `PreserveGroup()`: Preserves the group information in a chat message.
- `SendPublicKeyRequestAsync()`: Sends a request for the public key of another user.
- `SendUserOnlineStatusQueryRequestAsync()`: Sends a request to check if another user is online.
- `DisengageHandlers()`: Disengages event handlers.

## Getting Started

To get started with the ChatSuite SDK, clone the repository and open the solution in your preferred IDE. Restore the NuGet packages and build the project to ensure all dependencies are correctly resolved.

```sh
git clone https://github.com/your-repo/chatsuite.sdk.net.git
cd chatsuite.sdk.net
dotnet restore
dotnet build
```

## Contributing

Contributions are welcome! Please fork the repository and submit pull requests for any enhancements or bug fixes.
