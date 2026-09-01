FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Unipr.WebApp.csproj", "./"]
RUN dotnet restore "Unipr.WebApp.csproj"
COPY . .
RUN dotnet publish "Unipr.WebApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Unipr.WebApp.dll"]