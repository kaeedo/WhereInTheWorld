namespace WhereInTheWorld

open WhereInTheWorld.Utilities.Models
open System
open Hopac
open Hopac.Infixes

module StatusPrinter =
    type Ticker (milliseconds: int) =
        let tickChannel = Ch<string>()

        let cancelled = IVar()

        let tick () =
            let mutable current = "|"
            Ch.give tickChannel <|
                if current = "|"
                then
                    current <- "-"
                    "-"
                else
                    current <- "|"
                    "|"

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

    let downloadStatusPrinter channel =
        job {
            let! status = Ch.take channel

            match status with
            | Completed cc ->
                printfn "%s downloaded" cc
        }

    let insertStatusPrinter message =
        job {
            let! status = Ch.take message

            printf "%A" status
        }
