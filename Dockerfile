FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY src/mywebapp/mywebapp.csproj src/mywebapp/
RUN dotnet restore src/mywebapp/mywebapp.csproj

COPY src/mywebapp/ src/mywebapp/
RUN dotnet publish src/mywebapp/mywebapp.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "mywebapp.dll"]