namespace WhereInTheWorld.Utilities

open System
open Hopac
open Hopac.Infixes

type Ticker (milliseconds: int) =
    let mutable symbol = "-"
    let tickChannel = Ch<string>()

    let cancelled = IVar()

    let tick () =
        symbol <- if symbol = "|" then "-" else "|"
        Ch.give tickChannel symbol

    let rec loop () =
        let tickerLoop =
            timeOutMillis milliseconds
            |> Alt.afterJob tick
            |> Alt.afterJob loop
        tickerLoop <|> IVar.read cancelled

    member __.Stop() =
        Console.WriteLine()
        IVar.tryFill cancelled () |> start

    member __.Start() =
        do start (loop())

    member __.Channel
        with get() = tickChannel
