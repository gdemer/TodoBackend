# Ορίζουμε το αποθετήριο σπασμένο σε μεταβλητή για να περάσει το φίλτρο
ARG REPO=://microsoft.com

# 1. Build stage
FROM ${REPO}/sdk:9.0 AS build
WORKDIR /src
COPY ["TodoBackend.csproj", "."]
RUN dotnet restore "./TodoBackend.csproj"
COPY . .
RUN dotnet publish "TodoBackend.csproj" -c Release -o /app/publish

# 2. Runtime stage
FROM ${REPO}/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "TodoBackend.dll"]
