FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PROG7311_P2/PROG7311_P2.csproj", "PROG7311_P2/"]
RUN dotnet restore "PROG7311_P2/PROG7311_P2.csproj"
COPY . .
WORKDIR "/src/PROG7311_P2"
RUN dotnet build "PROG7311_P2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PROG7311_P2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PROG7311_P2.dll"] 