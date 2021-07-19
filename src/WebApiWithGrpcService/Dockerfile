FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
WORKDIR /app

COPY /*.csproj ./
RUN dotnet restore "./Server.WebApi.csproj" -s "https://api.nuget.org/v3/index.json"

COPY . ./
RUN dotnet publish *.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "Server.WebApi.dll"]