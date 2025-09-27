namespace CloudFlare.AI

[<AutoOpen>]
module Extensions =
    open Fable.Core
    open Fable.Core.JsInterop
    open System

    // Computation expression for working with promises
    type PromiseBuilder() =
        member _.Bind(m: JS.Promise<'a>, f: 'a -> JS.Promise<'b>) =
            m?`then`(f) :?> JS.Promise<'b>
        member _.Return(x: 'a) =
            JS.Promise.resolve(x)
        member _.ReturnFrom(x: JS.Promise<'a>) = x
        member _.Zero() = JS.Promise.resolve()

    let promise = PromiseBuilder()

    // Async extensions
    type JS.Promise<'T> with
        member this.ToAsync() =
            Async.AwaitPromise this
