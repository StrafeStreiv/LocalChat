FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RealtimeHub.csproj", "."]
RUN dotnet restore "RealtimeHub.csproj"
COPY . .
RUN dotnet build "RealtimeHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealtimeHub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/api/health || exit 1

ENTRYPOINT ["dotnet", "RealtimeHub.dll"]