namespace WhereInTheWorld

open WhereInTheWorld.Utilities.Models
open Hopac

module StatusPrinter =
    let private printSpinner =
        printfn "wefwefdsfdsf"
        job {
            let mutable current = "|"
            while true do
                printfn "\r%s" current
                current <- if current = "|" then "-" else "|"
                do! timeOutMillis 100
        }

    let downloadStatusPrinter channel =
        job {
            let! status = Ch.take channel

            match status with
            | Completed cc ->
                printfn "%s downloaded" cc
        }

    let insertStatusPrinter channel =
        let isFinished status =
            match status with
                | Started _ ->
                    false
                | Inserted _ ->
                    true

        job {
            let! status = Ch.take channel
            do! Job.whileDo (fun () -> not <| isFinished status) printSpinner
        }
