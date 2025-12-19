FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BenefitService/BenefitService.csproj", "BenefitService/"]
RUN dotnet restore "BenefitService/BenefitService.csproj"
COPY . .
WORKDIR "/src/BenefitService"
RUN dotnet build "./BenefitService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BenefitService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY BenefitService/wwwroot ./app/wwwroot
ENTRYPOINT ["dotnet", "BenefitService.dll"]
