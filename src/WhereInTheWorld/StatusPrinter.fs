namespace WhereInTheWorld

open WhereInTheWorld.Update.Models
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
            | Inserted cc ->
                printfn "%s inserted" cc
        }
