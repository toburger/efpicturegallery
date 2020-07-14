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

type GetGalleriesWithTag = SQL<"""
    select distinct *
    from galleries_with_tags g
    join gallery_tags gt on g.name = gt.gallery
    where case when @with_tags then tag in @tags else true end
    order by g.last_update desc
""">

type GetGalleriesWithoutTag = SQL<"""
    select * from galleries_with_tags
    except
        select distinct g.*
        from galleries_with_tags g
        join gallery_tags gt on g.name = gt.gallery
        where case when @with_tags then tag in @tags else true end
        
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
    let galleryNames = [ for x in 1..10 -> sprintf "gallery-%i" x ]
    for i, galleryName in List.indexed galleryNames do
        do! InsertGallery.Command(name = galleryName, url = Some "http://www.example.com").Plan()

        let tags = if i % 2 <> 0 then ['a'..'b'] else ['c'..'d']
        for tag in tags do
            do! InsertTag.Command(tag = sprintf "tag-%c" tag, gallery = galleryName).Plan()

    let galleryNamesOpt = List.toArray (None::List.map Some galleryNames)
    for x in 1..100_000 do
        let galleryName = galleryNamesOpt.[x % galleryNamesOpt.Length]
        do! InsertPicture.Command(
                filename = (sprintf "picture-%i" x),
                gallery = galleryName,
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

let queryData tags = plan {
    printfn "Executing query..."
    let! res =
        GetGalleriesWithTag.Command(
            with_tags = Option.isSome tags,
            tags = List.toArray (defaultArg tags [])
        ).Plan()
    for row in res do
        printfn "%A" {| Name = row.name
                        Url = row.url
                        Count = row.count
                        LastUpdate = row.last_update
                        Tags = [| for tag in row.tags -> tag.tag |] |}
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

    (Execution.execute config setupData).Wait()

    let tags = Some [ "tag-a" ]
    (Execution.execute config (queryData tags)).Wait()

    printfn "Finished"

    0
