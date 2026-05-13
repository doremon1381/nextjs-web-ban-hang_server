FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NhanViet.Api/NhanViet.Api.csproj", "NhanViet.Api/"]
COPY ["src/NhanViet.Application/NhanViet.Application.csproj", "NhanViet.Application/"]
COPY ["src/NhanViet.Domain/NhanViet.Domain.csproj", "NhanViet.Domain/"]
COPY ["src/NhanViet.Infrastructure/NhanViet.Infrastructure.csproj", "NhanViet.Infrastructure/"]
RUN dotnet restore "NhanViet.Api/NhanViet.Api.csproj"
COPY src/ .
RUN dotnet publish "NhanViet.Api/NhanViet.Api.csproj" -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NhanViet.Api.dll"]
