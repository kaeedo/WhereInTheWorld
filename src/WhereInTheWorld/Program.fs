open Argu
open System.IO
open WhereInTheWorld
open WhereInTheWorld.ArgumentParser
open WhereInTheWorld.Update

let ensureDirectory () =
    if Directory.Exists(Models.baseDirectory)
    then Directory.Delete(Models.baseDirectory, true)
    Directory.CreateDirectory(Models.baseDirectory) |> ignore

let parser = ArgumentParser.Create<Arguments>(programName = "witw.exe")


[<EntryPoint>]
let main argv =
    if argv |> Seq.isEmpty
    then 
        printfn "%s" <| parser.PrintUsage()
        0
    else
        let arguments = parser.Parse argv
        let postalCode = arguments.Contains PostalCode
        let update = arguments.Contains Update
        
        0

        (* ensureDirectory()
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
     *)