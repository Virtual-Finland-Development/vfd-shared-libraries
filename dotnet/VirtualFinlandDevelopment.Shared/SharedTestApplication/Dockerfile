FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SharedTestApplication/SharedTestApplication.csproj", "SharedTestApplication/"]
RUN dotnet restore "SharedTestApplication/SharedTestApplication.csproj"
COPY . .
WORKDIR "/src/SharedTestApplication"
RUN dotnet build "SharedTestApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharedTestApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharedTestApplication.dll"]
