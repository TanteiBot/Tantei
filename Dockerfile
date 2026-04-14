FROM mcr.microsoft.com/dotnet/nightly/sdk:11.0-preview-alpine3.24 AS builder
COPY . /source
WORKDIR source
RUN dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o /app --no-self-contained -r linux-musl-x64 /p:DefineConstants=IsInContainer /p:IsInContainer=true

FROM mcr.microsoft.com/dotnet/nightly/aspnet:11.0-preview-alpine3.24-extra AS final
LABEL org.opencontainers.image.source="https://github.com/TanteiBot/Tantei"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=builder /app /home/app
WORKDIR /home/app
ENTRYPOINT ["dotnet", "PaperMalKing.dll"]
