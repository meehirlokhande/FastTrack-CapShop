FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared/ Shared/
COPY Services/OrderService/CapShop.OrderService/ Services/OrderService/CapShop.OrderService/
WORKDIR /src/Services/OrderService/CapShop.OrderService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapShop.OrderService.dll"]
