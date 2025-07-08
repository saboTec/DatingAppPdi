

# Stage 1: Build Angular and ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# --------- BACKEND ---------
# Copy and restore backend
COPY API/*.csproj ./API/
RUN dotnet restore ./API/API.csproj
COPY API ./API

# --------- FRONTEND ---------
# Install Node.js + npm
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

# Copy Angular project into container
WORKDIR /app/client
COPY client/package*.json ./
RUN npm install

COPY client ./
RUN npm run build:subpath

# Output goes to ../API/wwwroot
# Already handled by outputPath in angular.json

# --------- PUBLISH .NET ---------
WORKDIR /app/API
RUN dotnet publish -c Release -o /out

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose HTTP
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "API.dll"]