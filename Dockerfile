FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DotnetAzureStarter.slnx ./
COPY DotnetAzureStarter.Api/DotnetAzureStarter.Api.csproj DotnetAzureStarter.Api/
COPY DotnetAzureStarter.Core/DotnetAzureStarter.Core.csproj DotnetAzureStarter.Core/
COPY DotnetAzureStarter.Infrastructure/DotnetAzureStarter.Infrastructure.csproj DotnetAzureStarter.Infrastructure/

RUN dotnet restore DotnetAzureStarter.Api/DotnetAzureStarter.Api.csproj

COPY DotnetAzureStarter.Api/ DotnetAzureStarter.Api/
COPY DotnetAzureStarter.Core/ DotnetAzureStarter.Core/
COPY DotnetAzureStarter.Infrastructure/ DotnetAzureStarter.Infrastructure/

RUN dotnet publish DotnetAzureStarter.Api/DotnetAzureStarter.Api.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetAzureStarter.Api.dll"]
