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
    dotnet build ./src/PaperMalKing/PaperMalKing.csproj --tl:off --verbosity q
    $env:KUBB_DISABLE_TELEMETRY = '1'; npm --prefix ./src/Tantei.Client run --silent generate:api

check-client:
    npm --prefix ./src/Tantei.Client run format:check
    npm --prefix ./src/Tantei.Client run lint:check
    npm --prefix ./src/Tantei.Client run type-check

test:
    #!pwsh -NoLogo
    $PSNativeCommandUseErrorActionPreference = $false
    foreach ($configuration in @('Debug', 'Release')) {
        $log = Join-Path $PWD "TestResults/dotnet-test-$configuration.log"
        New-Item -ItemType Directory -Force -Path (Split-Path -Path $log) | Out-Null
        dotnet test -c $configuration --verbosity q --no-ansi --no-progress 2>&1 | Set-Content -LiteralPath $log -Encoding utf8
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            Get-Content -LiteralPath $log
            Write-Host "dotnet test -c $configuration failed with exit code $exitCode."
            exit $exitCode
        }
        $counts = @(Select-String -LiteralPath $log -Pattern '^\s+(total|failed|succeeded|skipped): \d+$' | ForEach-Object { $_.Line.Trim() })
        Write-Host "$configuration tests: $(if ($counts) { $counts -join ', ' } else { 'passed' }) (log: $log)"
    }

build-client:
    npm --prefix ./src/Tantei.Client run build
    npm --prefix ./src/Tantei.Client run build-storybook

generate-tenrai:
    pwsh -NoLogo -NoProfile -File ./src/PaperMalKing.MyAnimeList.Wrapper/Tenrai/Generate.ps1

update-tenrai-openapi:
    pwsh -NoLogo -NoProfile -File ./src/PaperMalKing.MyAnimeList.Wrapper/Tenrai/UpdateOpenApi.ps1

check-generated-tenrai: generate-tenrai
    #!pwsh -NoLogo
    git diff --exit-code -- src/PaperMalKing.MyAnimeList.Wrapper/Tenrai .config/dotnet-tools.json
    if ($LASTEXITCODE -ne 0) {
        Write-Host "The checked-in Tenrai contract or client is stale. Run 'just generate-tenrai' and commit the regenerated assets."
        exit 1
    }

# Fail if the committed OpenAPI document or generated API client is stale
check-generated-api:
    #!pwsh -NoLogo
    $ErrorActionPreference = 'Stop'
    $env:KUBB_DISABLE_TELEMETRY = '1'
    dotnet build ./src/PaperMalKing/PaperMalKing.csproj --tl:off --verbosity q
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    npm --prefix ./src/Tantei.Client run --silent generate:api
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    git diff --exit-code -- src/Tantei.Client/openapi.json src/Tantei.Client/src/api/gen
    if ($LASTEXITCODE -ne 0) {
        Write-Host "openapi.json or src/api/gen is stale. Commit the regenerated files alongside your API change."
        exit 1
    }

# Refuse to build while the bot holds its own Debug output open
_check-not-running:
    #!pwsh -NoLogo
    $buildOutputPath = [IO.Path]::GetFullPath((Join-Path $PWD 'src/PaperMalKing/bin')) + [IO.Path]::DirectorySeparatorChar
    $running = @(Get-Process -Name PaperMalKing -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -and [IO.Path]::GetFullPath($_.Path).StartsWith($buildOutputPath, [StringComparison]::OrdinalIgnoreCase)
    })
    if ($running.Count -gt 0) {
        Write-Host "Tantei is running from src/PaperMalKing/bin (PID $($running.Id -join ', ')), so builds fail with MSB3021. Stop it and re-run."
        exit 1
    }

# Everything CI runs: builds, tests, generated-code drift, client checks
verify: _check-not-running _ci_release _ci_release_container _ci_debug _ci_debug_container check-generated-api check-generated-tenrai check-client build-client test
    echo Success

ci: _ci_release _ci_release_container _ci_debug _ci_debug_container check-client
    echo Success
    
_ci_debug:
    dotnet build -c Debug --tl:off --verbosity q
    
_ci_release:
    dotnet build -c Release --tl:off --verbosity q
    
_ci_debug_container:
    dotnet build -c Debug /p:DefineConstants=IsInContainer /p:IsInContainer=true --tl:off --verbosity q
    
_ci_release_container:
    dotnet build -c Release /p:DefineConstants=IsInContainer /p:IsInContainer=true --tl:off --verbosity q