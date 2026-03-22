FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY RusAuth.Authorization.Example/RusAuth.Authorization.Example.csproj RusAuth.Authorization.Example/

RUN dotnet restore RusAuth.Authorization.Example/RusAuth.Authorization.Example.csproj --nologo

COPY . .

RUN dotnet publish RusAuth.Authorization.Example/RusAuth.Authorization.Example.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "RusAuth.Authorization.Example.dll"]
