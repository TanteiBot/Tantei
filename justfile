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
    dotnet run --project ./src/PaperMalKing/PaperMalKing.csproj

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