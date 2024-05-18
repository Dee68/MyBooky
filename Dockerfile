FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MyBookyWeb/MyBookyWeb.csproj", "MyBookyWeb/"]
COPY ["MyBooky.DataAccess/MyBooky.DataAccess.csproj", "MyBooky.DataAccess/"]
COPY ["MyBooky.Models/MyBooky.Models.csproj", "MyBooky.Models/"]
COPY ["MyBooky.Utilities/MyBooky.Utilities.csproj", "MyBooky.Utilities/"]

RUN dotnet restore "MyBookyWeb/MyBookyWeb.csproj"

COPY . .
WORKDIR "/src/MyBookyWeb"
RUN dotnet build "MyBookyWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyBookyWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["MyBookyWeb/wwwroot/images", "/app/wwwroot/images"]

ENTRYPOINT ["dotnet", "MyBookyWeb.dll"]
