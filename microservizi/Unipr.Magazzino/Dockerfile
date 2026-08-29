FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TOOOOMM_NUGET_USERNAME
ARG TOOOOMM_NUGET_PASSWORD

WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Unipr.Magazzino.Api/Unipr.Magazzino.Api.csproj", "Unipr.Magazzino.Api/"]
COPY ["Unipr.Magazzino.Business/Unipr.Magazzino.Business.csproj", "Unipr.Magazzino.Business/"]
COPY ["Unipr.Magazzino.Repository/Unipr.Magazzino.Repository.csproj", "Unipr.Magazzino.Repository/"]
COPY ["Unipr.Magazzino.Shared/Unipr.Magazzino.Shared.csproj", "Unipr.Magazzino.Shared/"]
COPY ["Unipr.Magazzino.ClientHttp/Unipr.Magazzino.ClientHttp.csproj", "Unipr.Magazzino.ClientHttp/"]

RUN dotnet restore "Unipr.Magazzino.Api/Unipr.Magazzino.Api.csproj"
COPY . .
WORKDIR "/src/Unipr.Magazzino.Api"
RUN dotnet build "Unipr.Magazzino.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Unipr.Magazzino.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unipr.Magazzino.Api.dll"]