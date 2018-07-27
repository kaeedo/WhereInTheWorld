namespace WhereInTheWorld.Utilities

open Hopac
open Hopac.Infixes
open Models

type Ticker (milliseconds: int) =
    let mutable symbol = "-"
    let tickChannel = Ch<InsertStatus>()

    let cancelled = IVar()

    let tick () =
        symbol <- if symbol = "|" then "-" else "|"
        tickChannel *<- Progress symbol

    let rec loop () =
        let tickerLoop =
            timeOutMillis milliseconds
            |> Alt.afterJob tick
            |> Alt.afterJob loop
        tickerLoop <|> IVar.read cancelled

    member this.Stop() =
        IVar.tryFill cancelled () |> start

    member this.Start() =
        do start (loop())

    member this.Channel
        with get() = tickChannel
