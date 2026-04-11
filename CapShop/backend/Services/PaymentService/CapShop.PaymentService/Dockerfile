FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared/ Shared/
COPY Services/PaymentService/CapShop.PaymentService/ Services/PaymentService/CapShop.PaymentService/
WORKDIR /src/Services/PaymentService/CapShop.PaymentService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapShop.PaymentService.dll"]
