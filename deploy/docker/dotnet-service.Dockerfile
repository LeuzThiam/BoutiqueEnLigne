FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT_PATH
ARG APP_DLL
WORKDIR /src

COPY . .
RUN dotnet restore "$PROJECT_PATH"
RUN dotnet publish "$PROJECT_PATH" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
ARG APP_DLL
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet \"$APP_DLL\""]
