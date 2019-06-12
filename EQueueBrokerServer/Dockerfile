FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY EQueueBrokerServer/EQueueBrokerServer.csproj EQueueBrokerServer/
RUN dotnet restore EQueueBrokerServer/EQueueBrokerServer.csproj
COPY . .
WORKDIR /src/EQueueBrokerServer
RUN dotnet build EQueueBrokerServer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish EQueueBrokerServer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EQueueBrokerServer.dll"]
