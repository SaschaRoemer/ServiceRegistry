# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
#FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG TARGETARCH
WORKDIR /App

# copy csproj and restore as distinct layers
COPY src/*.csproj .
RUN dotnet restore --ucr

# copy and publish app and libraries
COPY src/. .
RUN dotnet publish --ucr -o out


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://*:7089
EXPOSE 7089

WORKDIR /App
COPY --from=build /App/out .
USER $APP_UID
ENTRYPOINT ["dotnet", "ServiceRegistry.dll"]