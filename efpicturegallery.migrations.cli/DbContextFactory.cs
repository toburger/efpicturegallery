using Microsoft.EntityFrameworkCore.Design;

namespace efpicturegallery.migrations.cli
{
    public class DbContextFactory : IDesignTimeDbContextFactory<Model.DbContext>
    {
        public Model.DbContext CreateDbContext(string[] args)
        {
            return new Model.DbContext("Data Source=c:/temp/pictures/efpictures.db");
        }
    }
}
