FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-alpine3.18-arm64v8 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BurnIn.ControlService/BurnIn.ControlService.csproj", "BurnIn.ControlService/"]
COPY ["BurnIn.Shared/BurnIn.Shared.csproj", "BurnIn.Shared/"]
RUN dotnet restore "BurnIn.ControlService/BurnIn.ControlService.csproj"
COPY . .
WORKDIR "/src/BurnIn.ControlService"
RUN dotnet build "BurnIn.ControlService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BurnIn.ControlService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StationController.dll"]
