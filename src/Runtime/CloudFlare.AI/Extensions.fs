namespace CloudFlare.AI

[<AutoOpen>]
module Extensions =
    open Fable.Core
    open Fable.Core.JsInterop

    // Helper to chain promises (equivalent to .then() that returns a promise)
    [<Emit("$0.then($1)")>]
    let private thenPromise (p: JS.Promise<'a>) (f: 'a -> JS.Promise<'b>) : JS.Promise<'b> = jsNative

    // Computation expression for working with promises
    type PromiseBuilder() =
        member _.Bind(m: JS.Promise<'a>, f: 'a -> JS.Promise<'b>) : JS.Promise<'b> =
            thenPromise m f
        member _.Return(x: 'a) : JS.Promise<'a> =
            JS.Constructors.Promise.resolve(x)
        member _.ReturnFrom(x: JS.Promise<'a>) : JS.Promise<'a> = x
        member _.Zero() : JS.Promise<unit> = JS.Constructors.Promise.resolve()

    let promise = PromiseBuilder()

    // Async extensions
    type JS.Promise<'T> with
        member this.ToAsync() =
            Async.AwaitPromise this
