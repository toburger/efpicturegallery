using Microsoft.EntityFrameworkCore.Migrations;

namespace efpicturegallery.migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "galleries",
                columns: table => new
                {
                    name = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_galleries", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "pictures",
                columns: table => new
                {
                    filename = table.Column<string>(nullable: false),
                    gallery = table.Column<string>(nullable: false),
                    width = table.Column<int>(nullable: false),
                    height = table.Column<int>(nullable: false),
                    created = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pictures", x => new { x.filename, x.gallery });
                    table.ForeignKey(
                        name: "fk_gallery_pictures",
                        column: x => x.gallery,
                        principalTable: "galleries",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gallery_tags",
                columns: table => new
                {
                    gallery = table.Column<string>(nullable: false),
                    tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gallery_tags", x => new { x.gallery, x.tag });
                    table.ForeignKey(
                        name: "fk_tags_gallery",
                        column: x => x.gallery,
                        principalTable: "galleries",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tags_tag",
                        column: x => x.tag,
                        principalTable: "tags",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gallery_tags_tag",
                table: "gallery_tags",
                column: "tag");

            migrationBuilder.CreateIndex(
                name: "ix_pictures_gallery",
                table: "pictures",
                column: "gallery");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gallery_tags");

            migrationBuilder.DropTable(
                name: "pictures");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "galleries");
        }
    }
}
