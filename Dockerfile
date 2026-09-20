# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TodoBackend.csproj", "."]
RUN dotnet restore "./TodoBackend.csproj"
COPY . .
RUN dotnet publish "TodoBackend.csproj" -c Release -o /app/publish --no-restore

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet TodoBackend.dll --urls http://0.0.0.0:${PORT:-8080}"]
