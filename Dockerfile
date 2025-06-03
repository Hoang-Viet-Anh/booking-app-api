# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj and restore as distinct layers
COPY *.csproj ./ 
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

# Expose the default port (optional, for Docker networking)
EXPOSE 8080

ENTRYPOINT ["dotnet", "booking-api.dll"]
