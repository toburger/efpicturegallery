module Gallery

open System.Linq
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks.ContextInsensitive
open Model

type PictureViewModel =
    { Filename: string
      Gallery: string
      Tags: string[]
      Width: int
      Height: int
      Created: System.DateTimeOffset }

let getPictures connectionString = task {
  use ctx = new DbContext(connectionString)
  let q = query {
      for p in ctx.Pictures do
      let tags = query {
        for gt in p.GalleryNav.TagsSet do
        select gt.TagNav.Name
      }
      sortByDescending p.Created
      select { Filename = p.Filename
               Gallery = p.GalleryNav.Name
               Tags = Seq.toArray tags
               Width = p.Width
               Height = p.Height
               Created = p.Created }
  }
  return! q.AsAsyncQueryable().ToArrayAsync()
}