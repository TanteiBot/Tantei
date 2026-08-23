FROM node:24-alpine AS frontend
WORKDIR /src/Tantei.Client
COPY src/Tantei.Client/package.json src/Tantei.Client/package-lock.json ./
RUN npm ci
COPY src/Tantei.Client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.23 AS builder
COPY . /source
COPY --from=frontend /src/PaperMalKing/wwwroot /source/src/PaperMalKing/wwwroot
WORKDIR source
RUN dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o /app --no-self-contained -r linux-musl-x64 /p:DefineConstants=IsInContainer /p:IsInContainer=true /p:SkipSpaBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23-extra AS final
LABEL org.opencontainers.image.source="https://github.com/TanteiBot/Tantei"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=builder /app /home/app
WORKDIR /home/app
ENTRYPOINT ["dotnet", "PaperMalKing.dll"]
