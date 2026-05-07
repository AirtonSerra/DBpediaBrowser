# Dockerfile multi-stage para aplicação ASP.NET Core targeting .NET 5
# Construção em imagem SDK e execução em imagem ASP.NET Runtime

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Copia apenas o csproj primeiro para aproveitar cache do docker
COPY DBpediaBrowser/DBpediaBrowser.csproj DBpediaBrowser/
RUN dotnet restore DBpediaBrowser/DBpediaBrowser.csproj

# Copia todo o código e publica em Release
COPY . .
WORKDIR /src/DBpediaBrowser
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:10000"

# Copia os artefatos publicados da fase de build
COPY --from=build /app/publish .

EXPOSE 10000
ENTRYPOINT ["dotnet", "DBpediaBrowser.dll"]
