FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/campaign/DndApp.Campaign.csproj", "services/campaign/"]
COPY ["shared/contracts/DndApp.Contracts.csproj", "shared/contracts/"]
RUN dotnet restore "services/campaign/DndApp.Campaign.csproj"

COPY . .
WORKDIR "/src/services/campaign"
RUN dotnet publish "DndApp.Campaign.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DndApp.Campaign.dll"]
