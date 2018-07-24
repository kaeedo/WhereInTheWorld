namespace WhereInTheWorld.Utilities

open Hopac
open Hopac.Infixes

type Ticker (milliseconds: int) =
    let tickChannel = Ch<string>()

    let cancelled = IVar()

    let tick () =
        Ch.give tickChannel "----------------"
        // let mutable current = "|"
        // Ch.give tickChannel <|
        //     if current = "|"
        //     then
        //         current <- "-"
        //         "-"
        //     else
        //         current <- "|"
        //         "|"

    let rec loop () =
        let tickerLoop =
            timeOutMillis milliseconds
            |> Alt.afterJob tick
            |> Alt.afterJob loop
        tickerLoop <|> IVar.read cancelled

    member __.Stop() =
        IVar.tryFill cancelled () |> start

    member __.Start() =
        do start (loop())

    member __.Channel
        with get() = tickChannel
