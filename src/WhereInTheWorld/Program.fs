open System.IO
open WhereInTheWorld
open WhereInTheWorld.Update

let ensureDirectory () =
    if Directory.Exists(Models.baseDirectory)
    then Directory.Delete(Models.baseDirectory, true)
    Directory.CreateDirectory(Models.baseDirectory) |> ignore

[<EntryPoint>]
let main argv =
    ensureDirectory()
    DataAccess.ensureDatabase()
    DataAccess.openConnection()

    let successfulUpdates, failedUpdates =
        UpdateProcess.updateAll StatusPrinter.downloadStatusPrinter StatusPrinter.insertStatusPrinter

    printfn "Succesfully updated %i countries" (successfulUpdates |> Seq.length)

    if failedUpdates |> Seq.isEmpty
    then ()
    else
        printfn "Problem updating the following"
        failedUpdates
        |> Seq.iter (function
            | Ok _ -> ()
            | Error (countryCode, e) -> printfn "%s failed with message %A" countryCode e
        )

    DataAccess.closeConnection()
    0
