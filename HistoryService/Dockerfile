FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["HistoryService/HistoryService.csproj", "HistoryService/"]
RUN dotnet restore "HistoryService/HistoryService.csproj"
COPY . .
WORKDIR "/src/HistoryService"
RUN dotnet build "HistoryService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HistoryService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HistoryService.dll"]