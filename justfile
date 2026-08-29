set windows-shell := ["pwsh.exe", "-NoLogo", "-Command"]

publish-dir := "output"

# Create migration
migrate Name:
	cd ./src/PaperMalKing/ && dotnet ef migrations add {{Name}} --project ../PaperMalKing.Database.Migrations/

# Adjust compiled models
optimize:
	cd ./src/PaperMalKing/ && dotnet ef dbcontext optimize --project ../PaperMalKing.Database/ --output-dir ../PaperMalKing.Database.CompiledModels/ --namespace PaperMalKing.Database.CompiledModels

# Generate SQL Script ot migrate
script LastMigration:
    cd ./src/PaperMalKing/ && dotnet ef migrations script {{LastMigration}}

publish dir=publish-dir:
    rm -r -fo {{dir}} | dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o {{dir}} --no-self-contained

benchmark Filter:
    cd ./benchmarks/Tantei.Benchmarks && dotnet run -c Release -- --filter *{{Filter}}*

run-server:
    dotnet clean && dotnet run --project ./src/PaperMalKing/PaperMalKing.csproj

run-client:
    cd ./src/Tantei.Client && npm run dev

run:
    #!pwsh -NoLogo
    $ErrorActionPreference = 'Stop'
    $backend = Start-Process dotnet -ArgumentList 'run','--project','./src/PaperMalKing/PaperMalKing.csproj' -NoNewWindow -PassThru
    try {
        npm --prefix ./src/Tantei.Client run dev
    } finally {
        if ($backend -and -not $backend.HasExited) {
            taskkill /PID $backend.Id /T /F | Out-Null
        }
    }

format-client:
    npm --prefix ./src/Tantei.Client run format

generate-api:
    dotnet build ./src/PaperMalKing/PaperMalKing.csproj
    npm --prefix ./src/Tantei.Client run generate:api

check-client:
    npm --prefix ./src/Tantei.Client run format:check
    npm --prefix ./src/Tantei.Client run lint:check
    npm --prefix ./src/Tantei.Client run type-check

test:
    dotnet test -c Debug
    dotnet test -c Release

build-client:
    npm --prefix ./src/Tantei.Client run build
    npm --prefix ./src/Tantei.Client run build-storybook

# Fail if the committed OpenAPI document or generated API client is stale
check-generated-api:
    #!pwsh -NoLogo
    $ErrorActionPreference = 'Stop'
    dotnet build ./src/PaperMalKing/PaperMalKing.csproj
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    npm --prefix ./src/Tantei.Client run generate:api
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    git diff --exit-code -- src/Tantei.Client/openapi.json src/Tantei.Client/src/api/gen
    if ($LASTEXITCODE -ne 0) {
        Write-Host "openapi.json or src/api/gen is stale. Commit the regenerated files alongside your API change."
        exit 1
    }

# Refuse to build while the bot holds its own Debug output open
_check-not-running:
    #!pwsh -NoLogo
    $running = @(Get-Process -Name PaperMalKing -ErrorAction SilentlyContinue)
    if ($running.Count -gt 0) {
        Write-Host "Tantei is running (PID $($running.Id -join ', ')). It locks src/PaperMalKing/bin/Debug, so builds fail with MSB3021. Stop it and re-run."
        exit 1
    }

# Everything CI runs: builds, tests, generated-code drift, client checks
verify: _check-not-running _ci_release _ci_release_container _ci_debug _ci_debug_container check-generated-api check-client build-client test
    echo Success

ci: _ci_release _ci_release_container _ci_debug _ci_debug_container check-client
    echo Success
    
_ci_debug:
    dotnet build -c Debug
    
_ci_release:
    dotnet build -c Release
    
_ci_debug_container:
    dotnet build -c Debug /p:DefineConstants=IsInContainer /p:IsInContainer=true
    
_ci_release_container:
    dotnet build -c Release /p:DefineConstants=IsInContainer /p:IsInContainer=true