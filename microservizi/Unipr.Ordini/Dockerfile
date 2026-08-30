# Usa l'immagine ufficiale Microsoft per far girare l'app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Usa l'immagine SDK per compilare il codice
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Queste variabili ci servono per passare username e token di GitHub per scaricare i pacchetti NuGet privati
ARG TOOOOMM_NUGET_USERNAME
ARG TOOOOMM_NUGET_PASSWORD
ENV TOOOOMM_NUGET_USERNAME=$TOOOOMM_NUGET_USERNAME
ENV TOOOOMM_NUGET_PASSWORD=$TOOOOMM_NUGET_PASSWORD

WORKDIR /src

# Copia il file delle password di NuGet
COPY ["NuGet.config", "."]

# Copia i file di progetto (i 5 layer)
COPY ["Unipr.Ordini.Api/Unipr.Ordini.Api.csproj", "Unipr.Ordini.Api/"]
COPY ["Unipr.Ordini.Business/Unipr.Ordini.Business.csproj", "Unipr.Ordini.Business/"]
COPY ["Unipr.Ordini.Repository/Unipr.Ordini.Repository.csproj", "Unipr.Ordini.Repository/"]
COPY ["Unipr.Ordini.Shared/Unipr.Ordini.Shared.csproj", "Unipr.Ordini.Shared/"]
COPY ["Unipr.Ordini.ClientHttp/Unipr.Ordini.ClientHttp.csproj", "Unipr.Ordini.ClientHttp/"]

# Scarica le librerie (incluso il pacchetto Kafka da GitHub)
RUN dotnet restore "Unipr.Ordini.Api/Unipr.Ordini.Api.csproj"

# Copia tutto il resto del codice e compila
COPY . .
WORKDIR "/src/Unipr.Ordini.Api"
RUN dotnet build "Unipr.Ordini.Api.csproj" -c Release -o /app/build

# Prepara la versione finale da pubblicare
FROM build AS publish
RUN dotnet publish "Unipr.Ordini.Api.csproj" -c Release -o /app/publish

# Inserisci l'app compilata nell'immagine leggera finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unipr.Ordini.Api.dll"]