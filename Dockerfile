FROM mcr.microsoft.com/dotnet/sdk:10.0.101-alpine3.23 AS builder
COPY . /source
WORKDIR source
RUN dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o /app --no-self-contained -r linux-musl-x64 /p:DefineConstants=IsInContainer /p:IsInContainer=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0.1-alpine3.23 AS final
LABEL org.opencontainers.image.source="https://github.com/TanteiBot/Tantei"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --upgrade --no-cache icu-data-full icu-libs tzdata

COPY --from=builder /app /home/app
WORKDIR /home/app
ENTRYPOINT ["dotnet", "PaperMalKing.dll"]
