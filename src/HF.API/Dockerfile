FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt update && apt install inetutils-ping -y
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/HF.API/HF.API.csproj", "src/HF.API/"]
COPY ["src/HF.Composition/HF.Composition.csproj", "src/HF.Composition/"]
COPY ["src/HF.Application/HF.Application.csproj", "src/HF.Application/"]
COPY ["src/HF.Domain/HF.Domain.csproj", "src/HF.Domain/"]
COPY ["src/HF.Infrastructure/HF.Infrastructure.csproj", "src/HF.Infrastructure/"]
RUN dotnet restore "./src/HF.API/HF.API.csproj"
COPY ./src/ .
WORKDIR "/src/HF.API"
RUN dotnet build "./HF.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HF.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "HF.API.dll"]
