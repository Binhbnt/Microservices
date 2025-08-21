FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NotificationsService.csproj", "./"]
RUN dotnet restore "./NotificationsService.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "NotificationsService.csproj" -c Release -o /app/build

# Giai đoạn 2: Publish
FROM build AS publish
WORKDIR "/src/."
RUN dotnet publish "NotificationsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 3: Final Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
# ======================================

EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationsService.dll"]