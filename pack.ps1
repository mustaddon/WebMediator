dotnet build -c Release 
dotnet pack .\WebMediator\ -c Release -o ..\_publish
dotnet pack .\WebMediator.Client\ -c Release -o ..\_publish
