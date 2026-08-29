FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TOOOOMM_NUGET_USERNAME
ARG TOOOOMM_NUGET_PASSWORD

WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Unipr.Pagamenti.Api/Unipr.Pagamenti.Api.csproj", "Unipr.Pagamenti.Api/"]
COPY ["Unipr.Pagamenti.Business/Unipr.Pagamenti.Business.csproj", "Unipr.Pagamenti.Business/"]
COPY ["Unipr.Pagamenti.Repository/Unipr.Pagamenti.Repository.csproj", "Unipr.Pagamenti.Repository/"]
COPY ["Unipr.Pagamenti.Shared/Unipr.Pagamenti.Shared.csproj", "Unipr.Pagamenti.Shared/"]
COPY ["Unipr.Pagamenti.ClientHttp/Unipr.Pagamenti.ClientHttp.csproj", "Unipr.Pagamenti.ClientHttp/"]

RUN dotnet restore "Unipr.Pagamenti.Api/Unipr.Pagamenti.Api.csproj"
COPY . .
WORKDIR "/src/Unipr.Pagamenti.Api"
RUN dotnet build "Unipr.Pagamenti.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Unipr.Pagamenti.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unipr.Pagamenti.Api.dll"]