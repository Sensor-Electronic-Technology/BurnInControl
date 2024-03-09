FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-alpine3.18-arm64v8 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 5581

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BurnInControl.UI/BurnInControl.UI.csproj", "BurnInControl.UI/"]
COPY ["BurnInControl.Application/BurnInControl.Application.csproj", "BurnInControl.Application/"]
COPY ["BurnInControl.Data/BurnInControl.Data.csproj", "BurnInControl.Data/"]
COPY ["BurnInControl.Shared/BurnInControl.Shared.csproj", "BurnInControl.Shared/"]
COPY ["BurnInControl.Infrastructure/BurnInControl.Infrastructure.csproj", "BurnInControl.Infrastructure/"]
COPY ["BurnInControl.UI.Shared/BurnInControl.UI.Shared.csproj", "BurnInControl.UI.Shared/"]
RUN dotnet restore "BurnInControl.UI/BurnInControl.UI.csproj"
COPY . .
WORKDIR "/src/BurnInControl.UI"
RUN dotnet build "BurnInControl.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BurnInControl.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StationUI.dll"]
