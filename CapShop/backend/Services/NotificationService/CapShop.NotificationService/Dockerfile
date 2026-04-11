FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Shared/ Shared/
COPY Services/NotificationService/CapShop.NotificationService/ Services/NotificationService/CapShop.NotificationService/
WORKDIR /src/Services/NotificationService/CapShop.NotificationService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CapShop.NotificationService.dll"]
