FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["StudentPlanner.Api/StudentPlanner.Api.csproj", "StudentPlanner.Api/"]
RUN dotnet restore "StudentPlanner.Api/StudentPlanner.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/StudentPlanner.Api"
RUN dotnet build "StudentPlanner.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentPlanner.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentPlanner.Api.dll"]
