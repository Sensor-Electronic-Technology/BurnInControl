FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./NuGet.config", "."]
COPY ["BurnInControl.Api/BurnInControl.Api.csproj", "BurnInControl.Api/"]
COPY ["BurnInControl.Data/BurnInControl.Data.csproj", "BurnInControl.Data/"]
COPY ["BurnInControl.Shared/BurnInControl.Shared.csproj", "BurnInControl.Shared/"]
COPY ["BurnInControl.Infrastructure/BurnInControl.Infrastructure.csproj", "BurnInControl.Infrastructure/"]
COPY ["BurnInControl.Api.Data/BurnInControl.Api.Data.csproj", "BurnInControl.Api.Data/"]
RUN dotnet restore "BurnInControl.Api/BurnInControl.Api.csproj"
COPY . .
WORKDIR "/src/BurnInControl.Api"
RUN dotnet build "BurnInControl.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BurnInControl.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
ENTRYPOINT ["dotnet", "BurnInControl.Api.dll"]
