module Model

open System
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore.Design
open System.Collections.Generic
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

type [<CLIMutable>] Gallery =
    { Name: string
      Url: string
      PicturesSet: ICollection<Picture>
      TagsSet: ICollection<GalleryTag> }

and [<CLIMutable>] Picture =
    { Filename: string
      Gallery: string
      GalleryNav: Gallery
      Width: int
      Height: int
      Created: DateTimeOffset }

and [<CLIMutable>] Tag =
    { Name: string
      GalleriesSet: ICollection<GalleryTag> }

and [<CLIMutable>] GalleryTag =
    { Gallery: string
      GalleryNav: Gallery
      Tag: string
      TagNav: Tag }

type DbContext(connectionString: string) =
    inherit Microsoft.EntityFrameworkCore.DbContext()
        override _.OnConfiguring(optionsBuilder) =
            optionsBuilder
                .UseSqlite(connectionString, fun sqliteOptionsBuilder -> 
                    sqliteOptionsBuilder.CommandTimeout(Nullable 5)
                                        .MigrationsAssembly("efpicturegallery.migrations")
                    |> ignore)
                .UseLoggerFactory(LoggerFactory.Create(fun builder ->
                    builder.AddConsole()
                    |> ignore))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging(true)
                .UseSnakeCaseNamingConvention()
                |> ignore
        override _.OnModelCreating(modelBuilder) =
            // Setup galleries table
            modelBuilder
                .Entity<Gallery>()
                .ToTable("galleries")
                .HasKey(fun g -> g.Name :> obj)
                |> ignore

            // Setup pictures table
            modelBuilder
                .Entity<Picture>()
                .ToTable("pictures")
                .HasKey(fun p -> {| Filename = p.Filename; Gallery = p.Gallery |} :> obj)
                |> ignore

            // Setup tags table
            modelBuilder
                .Entity<Tag>()
                .ToTable("tags")
                .HasKey(fun gt -> gt.Name :> obj)
                |> ignore

            // Setup gallery tags table
            modelBuilder
                .Entity<GalleryTag>()
                .ToTable("gallery_tags")
                .HasKey(fun gt -> {| Gallery = gt.Gallery; Tag = gt.Tag |} :> obj)
                |> ignore

            // Setup foreign key and navigation properties
            modelBuilder
                .Entity<GalleryTag>()
                .HasOne(fun gt -> gt.GalleryNav)
                .WithMany(fun g -> g.TagsSet :> seq<_>)
                .HasForeignKey(fun g -> g.Gallery :> obj)
                .HasConstraintName("fk_tags_gallery")
                |> ignore
            modelBuilder
                .Entity<GalleryTag>()
                .HasOne(fun gt -> gt.TagNav)
                .WithMany(fun g -> g.GalleriesSet :> seq<_>)
                .HasForeignKey(fun g -> g.Tag :> obj)
                .HasConstraintName("fk_tags_tag")
                |> ignore
            modelBuilder
                .Entity<Gallery>()
                .HasMany(fun g -> g.PicturesSet :> seq<_>)
                .WithOne(fun p -> p.GalleryNav)
                .HasForeignKey(fun p -> p.Gallery :> obj)
                .HasConstraintName("fk_gallery_pictures")
                .IsRequired(false)
                |> ignore

            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            // Source: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            for entityType in modelBuilder.Model.GetEntityTypes() do
                let properties =
                    entityType.ClrType.GetProperties()
                    |> Seq.filter (fun p -> p.PropertyType = typeof<DateTimeOffset>
                                         || p.PropertyType = typeof<Nullable<DateTimeOffset>>)
                for property in properties do
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(DateTimeOffsetToBinaryConverter())
                        |> ignore
    member self.Galleries = self.Set<Gallery>()
    member self.Pictures = self.Set<Picture>()

type DbContextFactory () =
    interface IDesignTimeDbContextFactory<DbContext> with
        member _.CreateDbContext(args) =
            new DbContext("Data Source=c:/temp/pictures/efpictures.db")
