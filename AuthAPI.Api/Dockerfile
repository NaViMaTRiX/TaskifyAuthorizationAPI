FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthAPI.Api/AuthAPI.Api.csproj", "AuthAPI.Api/"]
COPY ["AuthAPI.Application/AuthAPI.Application.csproj", "AuthAPI.Application/"]
COPY ["AuthAPI.Shared/AuthAPI.Shared.csproj", "AuthAPI.Shared/"]
COPY ["AuthAPI.Domain/AuthAPI.Domain.csproj", "AuthAPI.Domain/"]
COPY ["AuthAPI.DAL/AuthAPI.DAL.csproj", "AuthAPI.DAL/"]
RUN dotnet restore "AuthAPI.Api/AuthAPI.Api.csproj"
COPY . .
WORKDIR "/src/AuthAPI.Api"
RUN dotnet build "AuthAPI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthAPI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthAPI.Api.dll"]
