set windows-shell := ["pwsh.exe", "-NoLogo", "-Command"]

# Create migration
migrate Name:
	cd ./src/PaperMalKing/ && dotnet ef migrations add {{Name}} --project ../PaperMalKing.Database.Migrations/

# Adjust compiled models
optimize:
	cd ./src/PaperMalKing/ && dotnet ef dbcontext optimize --project ../PaperMalKing.Database.CompiledModels/ --output-dir ../PaperMalKing.Database.CompiledModels/ --namespace PaperMalKing.Database.CompiledModels

# Generate SQL Script ot migrate
script LastMigration:
    cd ./src/PaperMalKing/ && dotnet ef script {{LastMigration}}
    
publish:
    rm -r -fo output && dotnet publish ./src/PaperMalKing/PaperMalKing.csproj -c Release -o output && Com