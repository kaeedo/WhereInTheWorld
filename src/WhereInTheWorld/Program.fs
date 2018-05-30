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

    UpdateProcess.updateAll StatusPrinter.downloadStatusPrinter StatusPrinter.insertStatusPrinter

    DataAccess.closeConnection()
    0
