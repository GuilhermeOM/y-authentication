# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
ARG BUILD_CONFIGURATION=Release

ENV NUGET_USERNAME=${NUGET_USERNAME}
ENV NUGET_PASSWORD=${NUGET_PASSWORD}

RUN dotnet nuget add source --username ${NUGET_USERNAME} --password ${NUGET_PASSWORD} --store-password-in-clear-text --name github "https://nuget.pkg.github.com/GuilhermeOM/index.json"

WORKDIR /src
COPY ["./src/Y.Authentication.Api/Y.Authentication.Api.csproj", "src/Y.Authentication.Api/"]
COPY ["./src/Y.Authentication.Application/Y.Authentication.Application.csproj", "src/Y.Authentication.Application/"]
COPY ["./src/Y.Authentication.Domain/Y.Authentication.Domain.csproj", "src/Y.Authentication.Domain/"]
COPY ["./src/Y.Authentication.Infrastructure/Y.Authentication.Infrastructure.csproj", "src/Y.Authentication.Infrastructure/"]
COPY ["./src/Y.Authentication.Presentation/Y.Authentication.Presentation.csproj", "src/Y.Authentication.Presentation/"]
RUN dotnet restore "./src/Y.Authentication.Api/Y.Authentication.Api.csproj"
COPY . .
WORKDIR "/src/src/Y.Authentication.Api"
RUN dotnet build "./Y.Authentication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Y.Authentication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Y.Authentication.Api.dll"]
