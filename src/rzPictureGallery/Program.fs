open System
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Migrations
open Rezoom.SQL.Synchronous
open Rezoom.SQL.Plans

type GalleriesModel = SQLModel<".">

let migrate () =
    let config =
        { MigrationConfig.Default with
            LogMigrationRan = fun m -> printfn "Ran migration: %s" m.MigrationName }
    GalleriesModel.Migrate(config, "pictures")  //, "Data Source=c:/temp/pictures.db")

type InsertGallery = SQL<"""
    insert into galleries(name, url) values (@name, @url)
""">

type GetGalleries = SQL<"""
    select g.name, g.url, count(*) count, max(p.created) created from galleries g
    left join pictures p on g.name = p.gallery
    group by g.name, g.url
""">

type InsertPicture = SQL<"""
    insert into pictures(filename, gallery, width, height, created) values (@filename, @gallery, @width, @height, @created)
""">

type GetPictures = SQL<"""
    select * from pictures
    where gallery = @gallery
""">

let insertPictures () = plan {
    do! InsertGallery.Command(name = "example", url = Some "http://www.example.com").Plan()
    for x in 1..1000 do
        do! InsertPicture.Command(
                filename = (sprintf "hello-%i" x),
                gallery = (Some "example"),
                width = None,
                height = None,
                created = None
            ).Plan()
}

[<EntryPoint>]
let main argv =
    migrate ()

    use context = new ConnectionContext()

    // (Execution.execute Execution.ExecutionConfig.Default (insertPictures ())).Wait()

    let res = GetGalleries.Command().Execute(context)
    for row in res do
        printfn "%A" (row.name, row.url, row.count, row.created)
        // for picture in GetPictures.Command(gallery = row.name).Execute(context) do
        //     printfn "> %s" picture.filename

    0
