# 1. Build Stage
FROM library/dotnet-sdk:9.0 AS build-env
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Runtime Stage
FROM library/dotnet-aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "TodoBackend.dll"]
