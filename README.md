# DBpediaBrowser

DBpediaBrowser is an ASP.NET Core MVC web application that helps explore DBpedia resources and their relationships using the DBpedia SPARQL endpoint. It builds an interactive network graph (vis.js) of resources and literals related to a chosen DBpedia resource.

## Features
- Search DBpedia resources and visualize relationships as a graph
- Expand and remove nodes dynamically
- Autocomplete search against DBpedia resources
- Optional MySQL persistence for caching nodes and tracking popularity
- Simple authentication (register/login) backed by MySQL when enabled

## Technologies
- ASP.NET Core MVC (target: .NET 5)
- C#
- MySqlConnector (optional, for persistence)
- dotNetRDF (SPARQL client usage)
- vis.js for graph visualization
- Bootstrap and jQuery for UI

## Prerequisites
- .NET 5 SDK
- (Optional) MySQL server if you want to enable database persistence
- Visual Studio 2019 / 2022 / 2026 or VS Code

## Configuration
1. The project reads configuration from `appsettings.json` (see `DBpediaBrowser/appsettings.json`).
2. To enable database functionality, update the `ConnectionStrings:Default` value with your MySQL connection string.
3. The application currently sets `use_db` to `false` inside `HomeController` by default. Change this flag if you want the application to read/write nodes from/to the database.

## Run locally
Using the command line (from the solution root):

```bash
dotnet restore
dotnet build
dotnet run --project DBpediaBrowser/DBpediaBrowser.csproj
```

Or open the `DBpediaBrowser` project in Visual Studio and run (F5).

## Live demo
The application is available online at: https://dbpediabrowser.onrender.com

## Important endpoints / routes
- GET / or /Home/Index - main page with search
- POST /Home/Search - perform a search and build network data
- POST /Home/ExpandChart - expand a node
- POST /Home/RemoveNode - remove a node from the graph
- GET /Home/AutoCompleteSearch?search=... - autocomplete suggestions
- Authentication: /Authentication (login), /Authentication/Register

## Project structure (selected)
- DBpediaBrowser/Controllers - MVC controllers (Home, Authentication)
- DBpediaBrowser/Services/DBPedia - DBpedia SPARQL helper
- DBpediaBrowser/Biz - business layer accessing database (optional)
- DBpediaBrowser/Views - Razor views and UI
- DBpediaBrowser/wwwroot - static assets (js, css, libs)

## Notes
- The application queries the public DBpedia SPARQL endpoint (https://dbpedia.org/sparql). Be mindful of rate limits and endpoint availability.
- The project stores some session state (colors, current network data) in ASP.NET Core session; ensure session is configured for your hosting environment.

## Troubleshooting
- If SPARQL queries fail, check network connectivity and DBpedia endpoint status.
- If using MySQL, ensure the connection string is correct and the database user has necessary permissions.

## Acknowledgements
- DBpedia (https://dbpedia.org) for the SPARQL endpoint
- vis.js, Bootstrap, jQuery
