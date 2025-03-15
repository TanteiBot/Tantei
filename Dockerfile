FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
RUN apk add just && dotnet restore && just publish "/app"

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.21 AS final
LABEL org.opencontainers.image.source="https://github.com/tantei/tantei"

COPY --from=builder /app /app
ENTRYPOINT ["dotnet", "PaperMalKing.dll"]