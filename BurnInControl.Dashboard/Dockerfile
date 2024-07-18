FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./NuGet.config", "."]
COPY ["BurnInControl.Dashboard/BurnInControl.Dashboard.csproj", "BurnInControl.Dashboard/"]
COPY ["BurnInControl.Data/BurnInControl.Data.csproj", "BurnInControl.Data/"]
COPY ["BurnInControl.Shared/BurnInControl.Shared.csproj", "BurnInControl.Shared/"]
RUN dotnet restore "BurnInControl.Dashboard/BurnInControl.Dashboard.csproj"
COPY . .
WORKDIR "/src/BurnInControl.Dashboard"
RUN dotnet build "BurnInControl.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BurnInControl.Dashboard.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
ENTRYPOINT ["dotnet", "BurnInControl.Dashboard.dll"]
