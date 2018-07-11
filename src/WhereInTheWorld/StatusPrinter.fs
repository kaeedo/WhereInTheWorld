namespace WhereInTheWorld

open WhereInTheWorld.Utilities.Models
open Hopac

module StatusPrinter =

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
