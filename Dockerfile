FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY Game2048Web/*.csproj ./Game2048Web/
RUN dotnet restore Game2048Web/Game2048Web.csproj

# Copy the rest of the code and build
COPY . ./
RUN dotnet publish Game2048Web/Game2048Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port and set entry point
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

# Make sure the app listens on the correct port
CMD ["dotnet", "Game2048Web.dll", "--urls", "http://0.0.0.0:8080"]
