open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.EntityFrameworkCore
open Model

let insertData connectionString = task {
    use ctx = new DbContext(connectionString)

    let! _ = ctx.Database.EnsureDeletedAsync()
    let! _ = ctx.Database.EnsureCreatedAsync()

    let picture filename =
        { Filename = filename
          Gallery = null
          GalleryNav = Unchecked.defaultof<_> }

    let tag name =
        { Name = name
          GalleriesSet = null }

    let postTag tag =
        { Gallery = null
          GalleryNav = Unchecked.defaultof<_>
          Tag = null
          TagNav = tag }

    let natureTag = tag "nature"

    do! ctx.Galleries.AddRangeAsync([
        { Name = "Foo"
          Url = "http://foo.com"
          PicturesSet = ResizeArray [
            picture "bla bla bla"
            picture "blo blo blo"
          ]
          TagsSet = ResizeArray [
            postTag (tag "green")
            postTag (natureTag)
          ] }
        { Name = "Bar"
          Url = "http://bar.com"
          PicturesSet = null
          TagsSet = ResizeArray [
            postTag (tag "blue")
            postTag natureTag
          ] }
    ])
    let! _ = ctx.SaveChangesAsync()
    ()
}

open System.Linq

let queryData connectionString = task {
  use ctx = new DbContext(connectionString)
  let q = query {
      for p in ctx.Pictures.Include(fun x -> x.GalleryNav).ThenInclude(fun x -> x.TagsSet) do
      let tags = query {
        for gt in p.GalleryNav.TagsSet do
        select gt.TagNav.Name
      }
      select {| Filename = p.Filename; Gallery = p.GalleryNav.Name; Tags = tags |}
  }
  // return! q.ToArrayAsync() // throws exception
  return q.ToArray()
}

[<EntryPoint>]
let main argv =
    let connectionString = "Data Source=c:/temp/pictures/efpictures.db"
    //(insertData connectionString).Wait()

    let pictures = (queryData connectionString).Result
    for picture in pictures do
      printfn "%A" picture

    0
