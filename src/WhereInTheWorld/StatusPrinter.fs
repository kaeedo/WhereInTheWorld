namespace WhereInTheWorld

open WhereInTheWorld.Utilities.Models
open Hopac
open Hopac.Infixes

module StatusPrinter =
    type Message =
    | Started of string
    | Tick
    | Finished of string

    type CounterActor =
    | CA of Ch<Message>

    let create : Job<CounterActor> =
        job {
            let inChannel = Ch ()
            let state = ref (false, "|")
            do! Job.foreverServer
                 (inChannel >>= function
                   | Started cc ->
                        state := true, "-"
                        Job.unit ()
                   | Tick ->
                        let symbol = if snd !state = "|" then "-" else "|"
                        state := true, symbol
                        Job.unit ()
                   | Finished cc ->
                        state := false, ""
                        Job.unit ())
            return CA inChannel
        }
// https://vasily-kirichenko.github.io/fsharpblog/actors
    let downloadStatusPrinter channel =
        job {
            let! status = Ch.take channel

            match status with
            | Completed cc ->
                printfn "%s downloaded" cc
        }

    let insertStatusPrinter channel =
        job {
            let! status = Ch.take channel

            match status with
                | Started cc ->
                    printfn "Started %s" cc
                | Inserted cc ->
                    printfn "Inserted %s" cc
        }
