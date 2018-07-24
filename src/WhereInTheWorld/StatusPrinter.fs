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

    let insertStatusPrinter message =
        job {
            let! status = Ch.take message

            printf "%A" status
        }
