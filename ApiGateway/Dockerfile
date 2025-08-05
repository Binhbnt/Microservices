FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ApiGateway.csproj", "./"]
RUN dotnet restore "./ApiGateway.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

# Giai đoạn 2: Publish
FROM build AS publish
WORKDIR "/src/."
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 3: Final Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
# ======================================

EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiGateway.dll"]