open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open Model

let execute () = task {
    use ctx = new DbContext("Data Source=c:/temp/pictures/efpictures.db")

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

[<EntryPoint>]
let main argv =
    (execute ()).Wait()
    0
