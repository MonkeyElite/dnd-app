FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/bff/DndApp.Bff.csproj", "services/bff/"]
COPY ["shared/contracts/DndApp.Contracts.csproj", "shared/contracts/"]
RUN dotnet restore "services/bff/DndApp.Bff.csproj"

COPY . .
WORKDIR "/src/services/bff"
RUN dotnet publish "DndApp.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DndApp.Bff.dll"]
