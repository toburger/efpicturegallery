open System
open Rezoom
open Rezoom.SQL
open Rezoom.SQL.Migrations
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

type InsertTag = SQL<"""
    insert into gallery_tags(tag, gallery) values (@tag, @gallery)
""">

type GetGalleries = SQL<"""
    select coalesce(g.name, '<unnamed>') name, g.url, count(*) count, max(p.created) last_update from pictures p
    left join galleries g on p.gallery = g.name
    group by g.name, g.url
""">

type GetGalleriesWithTags = SQL<"""
    select * from galleries_with_tags
""">

type InsertPicture = SQL<"""
    insert into pictures(filename, gallery, width, height, created)
    values (@filename, @gallery, @width, @height, @created)
""">

type GetPictures = SQL<"""
    select * from pictures
    where gallery = @gallery
""">

let insertData () = plan {
    do! InsertGallery.Command(name = "example", url = Some "http://www.example.com").Plan()
    for tag in ['a'..'d'] do
        do! InsertTag.Command(tag = sprintf "tag-%c" tag, gallery = "example").Plan()
    for x in 1..100_000 do
        do! InsertPicture.Command(
                filename = (sprintf "hello-%i" x),
                gallery = (if x % 2 = 0 then Some "example" else None),
                width = None,
                height = None,
                created = Some (DateTime(2000, 01, 01).AddHours(float x))
            ).Plan()
}

type DeletePictures = SQL<"""
    delete from pictures
""">

type DeleteGalleries = SQL<"""
    delete from galleries
""">

let deleteAllData () = plan {
    do! DeletePictures.Command().Plan()
    do! DeleteGalleries.Command().Plan()
}

let setupData = plan {
    printfn "Delete existing data"
    do! deleteAllData ()
    printfn "Insert new data"
    do! insertData ()
    printfn "Finished"
}

let queryData = plan {
    printfn "Executing query..."
    let! res = GetGalleriesWithTags.Command().Plan()
    for row in res do
        printfn "%A" {| Name = row.name
                        Url = row.url
                        Count = row.count
                        LastUpdate = row.last_update
                        Tags = row.tags |> Seq.map (fun t -> t.tag) |> Seq.toArray |}
}

[<EntryPoint>]
let main argv =
    printfn "Migrate..."
    migrate ()

    let config =
        let log = Execution.ExecutionLog()
        // let log = Execution.ConsoleExecutionLog()
        let instance () = Execution.ExecutionInstance(log)
        { Execution.ExecutionConfig.Default with
            Instance = instance }

    // (Execution.execute config setupData).Wait()

    (Execution.execute config queryData).Wait()

    printfn "Finished"

    0
