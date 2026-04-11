FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Services/CatalogService/CapShop.CatalogService/ Services/CatalogService/CapShop.CatalogService/
WORKDIR /src/Services/CatalogService/CapShop.CatalogService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapShop.CatalogService.dll"]
