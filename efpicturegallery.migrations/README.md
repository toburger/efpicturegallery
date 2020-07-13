dotnet tool restore

dotnet ef migrations add InitialCreate --startup-project efpicturegallery.migrations.cli --project efpicturegallery.migrations --verbose

dotnet ef database update --startup-project efpicturegallery.migrations.cli --project efpicturegallery.migrations --verbose

TODO: Get rid of the DbContextFactory in efpicturegallery.migrations.cli https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet