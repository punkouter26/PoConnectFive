FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/Po.ConnectFive.Api/Po.ConnectFive.Api.csproj", "src/Po.ConnectFive.Api/"]
COPY ["src/Po.ConnectFive.Client/Po.ConnectFive.Client.csproj", "src/Po.ConnectFive.Client/"]
COPY ["src/Po.ConnectFive.Shared/Po.ConnectFive.Shared.csproj", "src/Po.ConnectFive.Shared/"]
COPY ["src/PoConnectFive.ServiceDefaults/PoConnectFive.ServiceDefaults.csproj", "src/PoConnectFive.ServiceDefaults/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
RUN dotnet restore "src/Po.ConnectFive.Api/Po.ConnectFive.Api.csproj"
COPY . .
WORKDIR "/src/src/Po.ConnectFive.Api"
RUN dotnet build "Po.ConnectFive.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Po.ConnectFive.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Po.ConnectFive.Api.dll"]
