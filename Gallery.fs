module Gallery

open System.Linq
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks.ContextInsensitive
open Model

let getPictures connectionString = task {
  use ctx = new DbContext(connectionString)
  let q = query {
      for p in ctx.Pictures do
      let tags = query {
        for gt in p.GalleryNav.TagsSet do
        select gt.TagNav.Name
      }
      sortByDescending p.Created
      select {| Filename = p.Filename
                Gallery = p.GalleryNav.Name
                Tags = tags |}
  }
  return! q.AsAsyncQueryable().ToArrayAsync()
}