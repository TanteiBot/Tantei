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