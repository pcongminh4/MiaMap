dotnet ef migrations add InitialDb -p Infrastructure -s Api -o Database/Migrations
dotnet ef database update -p Infrastructure -s Api