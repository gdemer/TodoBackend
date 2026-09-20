# 1. Build stage (Χρήση επίσημης εικόνας .NET SDK από το Amazon Public Gallery)
FROM public.ecr.aws/sam/build-dotnet9:latest AS build
WORKDIR /src
COPY ["TodoBackend.csproj", "."]
RUN dotnet restore "./TodoBackend.csproj"
COPY . .
RUN dotnet publish "TodoBackend.csproj" -c Release -o /app/publish

# 2. Runtime stage (Χρήση ελαφριάς εικόνας για το τρέξιμο)
FROM public.ecr.aws/docker/library/buildpack-deps:bookworm-curl AS final
WORKDIR /app
COPY --from=build /app/publish .

# Εγκατάσταση του .NET Runtime χειροκίνητα για απόλυτη ασφάλεια
RUN apt-get update && apt-get install -y wget && \
    wget https://dot.net -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 --runtime dotnet && \
    ln -s /root/.dotnet/dotnet /usr/bin/dotnet

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "TodoBackend.dll"]
