# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

- **Build the project**: `dotnet build`
- **Run the application**: `dotnet run`
- **Run in development mode**: `dotnet watch run` (if available, otherwise `dotnet run`)

## Project Structure

This is an ASP.NET Core Web Application (MVC) designed to upload files to SharePoint with metadata.

- **Controllers/**: Contains `HomeController.cs`, which handles the main file upload logic and interacts with the SharePoint services.
- **Models/**: Defines view models such as `UploadViewModel.cs` for handling form data from the UI.
- **Services/**: Contains the core business logic for interacting with SharePoint via `SharePointUploaderService.cs`. It implements an interface `ISharePointUploaderService .cs`.
- **Settings/**: Holds configuration classes like `SharePointSettings.cs`, which are populated from `appsettings.json`.
- **Views/**: Contains Razor views for the user interface, including the upload form and error pages.
- **wwwroot/**: Static assets such as CSS, JavaScript, and images.

## Key Architectural Patterns

- **Dependency Injection**: Services like `SharePointSettings` are injected into controllers using the `IOptions<T>` pattern.
- **Service Layer**: Business logic for SharePoint operations (getting Site IDs, Drive IDs, and uploading with metadata) is encapsulated in the `Services/` directory to keep controllers thin.
- **Configuration-Driven**: SharePoint site details and authentication settings are managed via `appsettings.json` and mapped to strongly typed configuration objects.

## Development Notes

- The application has specific configurations for handling large file uploads in `Program.cs`, adjusting `MaxRequestBodySize` and `MultipartBodyLengthLimit`.
- Ensure that the SharePoint credentials and site information in `appsettings.Development.json` are correctly configured for local testing.
