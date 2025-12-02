# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution file
COPY ["utcert-service.sln", "./"]

# Copy project files
COPY ["utcert-service/utcert-service.csproj", "utcert-service/"]
COPY ["UTCert.Data.Repository/UTCert.Data.Repository.csproj", "UTCert.Data.Repository/"]
COPY ["UTCert.Model.Database/UTCert.Model.Database.csproj", "UTCert.Model.Database/"]
COPY ["UTCert.Model.Shared/UTCert.Model.Shared.csproj", "UTCert.Model.Shared/"]
COPY ["UTCert.Model.Web/UTCert.Model.Web.csproj", "UTCert.Model.Web/"]
COPY ["UTCert.Service.BusinessLogic/UTCert.Service.BusinessLogic.csproj", "UTCert.Service.BusinessLogic/"]
COPY ["UTCert.Service.Helper/UTCert.Service.Helper.csproj", "UTCert.Service.Helper/"]

# Restore dependencies
RUN dotnet restore "utcert-service.sln"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/utcert-service"
RUN dotnet build "utcert-service.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "utcert-service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "utcert-service.dll"]

