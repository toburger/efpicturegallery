[<AutoOpen>]
module Interop

open System
open System.Linq
open System.Collections
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading.Tasks
open System.Threading

type private AsyncEnumerator<'T>(enumerator: IEnumerator<'T>) =
  interface IAsyncEnumerator<'T> with
      member _.Current = 
        enumerator.Current
      member _.DisposeAsync() = 
        enumerator.Dispose()
        ValueTask()
      member _.MoveNextAsync() = 
        ValueTask<_>(enumerator.MoveNext())

type NotInDbSet<'T>(input: seq<'T>) =
  interface IQueryable<'T> with
      member this.ElementType: Type = 
          typeof<'T>
      member this.Expression: Expressions.Expression = 
          upcast Expressions.Expression.Empty()
      member this.Provider: IQueryProvider = 
          upcast EnumerableQuery<'T>((this :> IQueryable<'T>).Expression)

  interface IAsyncEnumerable<'T> with
      member this.GetAsyncEnumerator(cancellationToken: CancellationToken): IAsyncEnumerator<'T> = 
          let enumerator = (this :> seq<'T>).GetEnumerator()
          upcast AsyncEnumerator<'T>(enumerator)

  interface IEnumerable<'T> with
      member this.GetEnumerator(): IEnumerator<'T> = 
        input.GetEnumerator()

  interface IEnumerable with
      member this.GetEnumerator(): IEnumerator = 
        upcast input.GetEnumerator()

[<Extension>]
type QueryableExtensions =
  [<Extension >]
    static member AsAsyncQueryable(input: seq<'T>) =
        NotInDbSet<'T>(input);