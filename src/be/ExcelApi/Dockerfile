# Dockerfile for ExcelApi service
# Using multi-stage build pattern for optimized container size

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory for the build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["BE/ExcelApi/ExcelApi.csproj", "BE/ExcelApi/"]

# Restore dependencies
RUN dotnet restore "BE/ExcelApi/ExcelApi.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/BE/ExcelApi"
RUN dotnet build "ExcelApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
# Publish the application
RUN dotnet publish "ExcelApi.csproj" -c Release -o /app/publish/excel_api /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app/excel_api

# Copy published files from publish stage
COPY --from=publish /app/publish/excel_api .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set the entry point
ENTRYPOINT ["dotnet", "ExcelApi.dll"]

# Image metadata
LABEL maintainer="VinhLTT <vinhltt.dev@gmail.com>"
LABEL description="Excel API service for processing Excel and CSV files"