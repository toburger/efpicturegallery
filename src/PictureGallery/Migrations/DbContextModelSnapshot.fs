namespace efpicturegallery.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

[<DbContext(typeof<Model.DbContext>)>]
type DbContextModelSnapshot() =
    inherit ModelSnapshot()

    override this.BuildModel(modelBuilder: ModelBuilder) =
        modelBuilder.HasAnnotation("ProductVersion", "3.1.5")
             |> ignore

        modelBuilder.Entity("Model+Gallery", (fun b ->

            b.Property<string>("Name")
                .HasColumnName("name")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("Url")
                .HasColumnName("url")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Name")
                .HasName("pk_galleries") |> ignore

            b.ToTable("galleries") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+GalleryTag", (fun b ->

            b.Property<string>("Gallery")
                .HasColumnName("gallery")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("Tag")
                .HasColumnName("tag")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Gallery", "Tag")
                .HasName("pk_gallery_tags") |> ignore


            b.HasIndex("Tag")
                .HasName("ix_gallery_tags_tag") |> ignore

            b.ToTable("gallery_tags") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+Picture", (fun b ->

            b.Property<string>("Filename")
                .HasColumnName("filename")
                .HasColumnType("TEXT") |> ignore
            b.Property<string>("Gallery")
                .HasColumnName("gallery")
                .HasColumnType("TEXT") |> ignore
            b.Property<Int64>("Created")
                .IsRequired()
                .HasColumnName("created")
                .HasColumnType("INTEGER") |> ignore
            b.Property<int>("Height")
                .IsRequired()
                .HasColumnName("height")
                .HasColumnType("INTEGER") |> ignore
            b.Property<int>("Width")
                .IsRequired()
                .HasColumnName("width")
                .HasColumnType("INTEGER") |> ignore

            b.HasKey("Filename", "Gallery")
                .HasName("pk_pictures") |> ignore


            b.HasIndex("Gallery")
                .HasName("ix_pictures_gallery") |> ignore

            b.ToTable("pictures") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+Tag", (fun b ->

            b.Property<string>("Name")
                .HasColumnName("name")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Name")
                .HasName("pk_tags") |> ignore

            b.ToTable("tags") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+GalleryTag", (fun b ->
            b.HasOne("Model+Gallery","GalleryNav")
                .WithMany("TagsSet")
                .HasForeignKey("Gallery")
                .HasConstraintName("fk_tags_gallery")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired() |> ignore
            b.HasOne("Model+Tag","TagNav")
                .WithMany("GalleriesSet")
                .HasForeignKey("Tag")
                .HasConstraintName("fk_tags_tag")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired() |> ignore
        )) |> ignore

        modelBuilder.Entity("Model+Picture", (fun b ->
            b.HasOne("Model+Gallery","GalleryNav")
                .WithMany("PicturesSet")
                .HasForeignKey("Gallery")
                .HasConstraintName("fk_gallery_pictures") |> ignore
        )) |> ignore

