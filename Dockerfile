# -- STAGE 1: Build --
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar el archivo de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# -- STAGE 2: Runtime --
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar los binarios desde la etapa de build
COPY --from=build /app/out .

# Configuración para Render:
# Usamos 0.0.0.0 para que sea accesible externamente y permitimos que el puerto sea dinámico.
# IMPORTANTE: Render inyecta la variable $PORT automáticamente.
ENV ASPNETCORE_URLS="http://0.0.0.0:8080"
# Exponemos el puerto por defecto, aunque Render lo sobreescribirá internamente.
EXPOSE 8080

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "pyreApi.dll"]
