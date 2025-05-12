FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy everything
COPY . .

# Restore and publish
RUN dotnet restore Game2048Web/Game2048Web.csproj
RUN dotnet publish Game2048Web/Game2048Web.csproj -c Release -o /app --no-restore

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Configure for Fly.io
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "Game2048Web.dll"]
