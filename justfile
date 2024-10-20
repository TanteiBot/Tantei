﻿set windows-shell := ["pwsh.exe", "-NoLogo", "-Command"]

# Create migration
migrate Name:
	cd ./src/PaperMalKing/ && dotnet ef migrations add {{Name}} --project ../PaperMalKing.Database.Migrations/

# Adjust compiled models
optimize:
	cd ./src/PaperMalKing/ && dotnet ef dbcontext optimize --project ../PaperMalKing.Database/ --output-dir ../PaperMalKing.Database.CompiledModels/ --namespace PaperMalKing.Database.CompiledModels --nativeaot

# Generate SQL Script ot migrate
script LastMigration:
    cd ./src/PaperMalKing/ && dotnet ef migrations script {{LastMigration}}
    
publish:
    rm -r -fo output | dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o output
    
benchmark Filter:
    cd ./benchmarks/Tantei.Benchmarks && dotnet run -c Release -- --filter *{{Filter}}*