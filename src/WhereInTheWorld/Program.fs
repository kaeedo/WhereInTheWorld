open System
open WhereInTheWorld.Update

[<EntryPoint>]
let main argv =
    let germanImport = DataImport.fileImport "DE"

    System.Console.ReadLine() |> ignore
    0
