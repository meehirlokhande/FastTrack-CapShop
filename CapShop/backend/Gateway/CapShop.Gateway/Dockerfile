FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Gateway/CapShop.Gateway/ Gateway/CapShop.Gateway/
WORKDIR /src/Gateway/CapShop.Gateway
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# ocelot config files are not auto-published; copy them explicitly
COPY --from=build /src/Gateway/CapShop.Gateway/ocelot.json .
COPY --from=build /src/Gateway/CapShop.Gateway/ocelot.Docker.json .c
ENTRYPOINT ["dotnet", "CapShop.Gateway.dll"]
