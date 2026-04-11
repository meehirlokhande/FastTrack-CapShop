FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Services/AdminService/CapShop.AdminService/ Services/AdminService/CapShop.AdminService/
WORKDIR /src/Services/AdminService/CapShop.AdminService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapShop.AdminService.dll"]
