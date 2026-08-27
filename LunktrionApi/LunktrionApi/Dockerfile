# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN mkdir -p /app/data \
    && chown -R $APP_UID /app/data
USER $APP_UID
EXPOSE 50000
VOLUME ["/app/data"]


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LunktrionApi/LunktrionApi.csproj", "LunktrionApi/"]
RUN dotnet restore "./LunktrionApi/LunktrionApi.csproj"
COPY . .
WORKDIR "/src/LunktrionApi"
RUN dotnet build "./LunktrionApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LunktrionApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LunktrionApi.dll"]