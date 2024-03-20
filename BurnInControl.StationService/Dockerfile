FROM mcr.microsoft.com/dotnet/aspnet:8.0.2-alpine3.18-arm64v8 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
# Add the GitHub NuGet package source
RUN --mount=type=secret,id=PASSWORD \
    dotnet nuget add source --username aelmendorf --password-file=/run/secrets/PASSWORD --store-password-in-clear-text --name github "https://nuget.pkg.github.com/Sensor-Electronic-Technology/index.json"
        
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BurnInControl.StationService/BurnInControl.StationService.csproj", "BurnInControl.StationService/"]
COPY ["BurnInControl.Data/BurnInControl.Data.csproj", "BurnInControl.Data/"]
COPY ["BurnInControl.Shared/BurnInControl.Shared.csproj", "BurnInControl.Shared/"]
COPY ["BurnInControl.Infrastructure/BurnInControl.Infrastructure.csproj", "BurnInControl.Infrastructure/"]
COPY ["StationService.Infrastructure/StationService.Infrastructure.csproj", "StationService.Infrastructure/"]
COPY ["BurnInControl.Application/BurnInControl.Application.csproj", "BurnInControl.Application/"]
RUN dotnet restore "BurnInControl.StationService/BurnInControl.StationService.csproj"
COPY . .
WORKDIR "/src/BurnInControl.StationService"
RUN dotnet build "BurnInControl.StationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BurnInControl.StationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#RUN chmod +x entry-script.sh
#ENTRYPOINT ["entry-script.sh"]
ENTRYPOINT ["dotnet", "StationService.dll"]
