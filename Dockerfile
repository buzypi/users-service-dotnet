FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UsersService.csproj", "./"]
RUN dotnet restore "UsersService.csproj"
COPY . .
RUN dotnet build "UsersService.csproj" -c Release -o /app/build
RUN dotnet publish "UsersService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersService.dll"]
