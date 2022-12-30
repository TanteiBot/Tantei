migrate:
	cd src/PaperMalKing/ && dotnet ef migrations add Check --project ../PaperMalKing.Database.Migrations/

optimize:
	cd src/PaperMalKing/ && dotnet ef dbcontext optimize --project ../PaperMalKing.Database.CompiledModels/ --output-dir ../PaperMalKing.Database.CompiledModels/ --namespace PaperMalKing.Database.CompiledModels