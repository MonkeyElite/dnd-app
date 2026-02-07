FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/catalog/DndApp.Catalog.csproj", "services/catalog/"]
COPY ["shared/contracts/DndApp.Contracts.csproj", "shared/contracts/"]
RUN dotnet restore "services/catalog/DndApp.Catalog.csproj"

COPY . .
WORKDIR "/src/services/catalog"
RUN dotnet publish "DndApp.Catalog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DndApp.Catalog.dll"]
