migrate:
	cd ./src/PaperMalKing/ && dotnet tool run dotnet-ef migrations add $(Name) --project ../PaperMalKing.Database.Migrations/

optimize:
	cd ./src/PaperMalKing/ && dotnet tool run dotnet-ef dbcontext optimize --project ../PaperMalKing.Database.CompiledModels/ --output-dir ../PaperMalKing.Database.CompiledModels/ --namespace PaperMalKing.Database.CompiledModels	

bundle-migrations:
	cd ./src/PaperMalKing/ && dotnet tool run dotnet-ef migrations bundle --configuration Release --force -r linux-x64 --project ../PaperMalKing.Database.Migrations/ --output ../../output/efbundle
