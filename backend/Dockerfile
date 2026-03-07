FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore UserDirectory.sln
RUN dotnet publish src/UserDirectory.Api/UserDirectory.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/data/app.db"

EXPOSE 8080
VOLUME ["/data"]

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserDirectory.Api.dll"]
