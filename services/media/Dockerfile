FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/media/DndApp.Media.csproj", "services/media/"]
COPY ["shared/contracts/DndApp.Contracts.csproj", "shared/contracts/"]
RUN dotnet restore "services/media/DndApp.Media.csproj"

COPY . .
WORKDIR "/src/services/media"
RUN dotnet publish "DndApp.Media.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DndApp.Media.dll"]
