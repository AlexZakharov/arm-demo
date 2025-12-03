# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/CryptoPriceAnalyzer/

# Copy project file and restore first
COPY CryptoPriceAnalyzer.csproj .
RUN dotnet restore

# Copy everything else
COPY . .

# Publish
RUN dotnet publish -c Release -o /out


# Runtime stage (Azure Functions isolated .NET 8)
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4 AS final

WORKDIR /home/site/wwwroot

RUN mkdir -p /home/site/wwwroot/data

COPY --from=build /out .

EXPOSE 80