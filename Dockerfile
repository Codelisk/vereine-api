# syntax=docker/dockerfile:1.7

# Versionen folgen global.json; Digests beim Heben der Vorschau mitziehen.
ARG DOTNET_SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:11.0.100-rc.3@sha256:3a9175b6c54b2f076817af1360f691c9994447b192faa48a1a1f112a516844c7
# Chiseled: keine Shell, kein Paketmanager, Nicht-root-Benutzer ab Werk.
# -extra bringt ICU und tzdata fuer de-AT und Europe/Vienna (InvariantGlobalization=false).
ARG DOTNET_ASPNET_IMAGE=mcr.microsoft.com/dotnet/aspnet:11.0.0-rc.3-resolute-chiseled-extra@sha256:4a02e6f650d442ef3acf66932e74a2f63f3585b44e1990c3c8edd2c010298ff0

FROM ${DOTNET_SDK_IMAGE} AS build
WORKDIR /src
COPY . .
RUN --mount=type=cache,id=vereine-api-nuget,target=/root/.nuget/packages \
    dotnet restore src/Vereine.Api/Vereine.Api.csproj
# JIT-Host (ADR-0002): Orleans laesst NativeAOT fuer den Silo noch nicht zu.
RUN --mount=type=cache,id=vereine-api-nuget,target=/root/.nuget/packages \
    dotnet publish src/Vereine.Api/Vereine.Api.csproj \
      --configuration Release \
      --no-restore \
      --output /out/api \
      /p:UseAppHost=false

FROM ${DOTNET_ASPNET_IMAGE} AS api
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0 \
    TZ=Europe/Vienna
USER $APP_UID
COPY --from=build --chown=$APP_UID:$APP_UID /out/api/ ./
# 8080 HTTP, 11111 Silo, 30000 Gateway
EXPOSE 8080 11111 30000
ENTRYPOINT ["dotnet", "Vereine.Api.dll"]
