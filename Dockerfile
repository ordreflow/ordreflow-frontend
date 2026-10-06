FROM mcr.microsoft.com/dotnet/sdk:8.0.424 AS build
WORKDIR /src

COPY global.json ./
RUN dotnet --version
COPY src/OrdreFlow.Frontend/OrdreFlow.Frontend.csproj src/OrdreFlow.Frontend/
RUN dotnet restore src/OrdreFlow.Frontend/OrdreFlow.Frontend.csproj

COPY src/OrdreFlow.Frontend/ src/OrdreFlow.Frontend/
RUN dotnet publish src/OrdreFlow.Frontend/OrdreFlow.Frontend.csproj -c Release --no-restore -o /app/publish

FROM nginxinc/nginx-unprivileged:stable-alpine
ENV NGINX_ENVSUBST_FILTER="^API_UPSTREAM$"
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/publish/wwwroot/ /usr/share/nginx/html/
EXPOSE 8080
