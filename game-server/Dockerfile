#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
#WORKDIR /App

# Copy everything
#COPY . ./
## Restore as distinct layers
#RUN dotnet restore
# Build and publish a release
#RUN dotnet publish -c Release -o out

# Build runtime image
#FROM mcr.microsoft.com/dotnet/aspnet:6.0
#WORKDIR /App
#COPY --from=build-env /App/out .
#EXPOSE 9090
#ENTRYPOINT ["dotnet", "GameServerTTT.dll"]

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./GameServerTTT.csproj" --disable-parallel
RUN dotnet publish "./GameServerTTT.csproj" -c release -o /app --no-restore

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "GameServerTTT.dll"]