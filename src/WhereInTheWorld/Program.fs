open System
open WhereInTheWorld.Update

[<EntryPoint>]
let main argv =
    let allImports = DataImport.supportedCountries
    allImports
    |> List.map (fun ai ->
        let code, _, _ = ai
        DataImport.fileImport code
    )
    |> List.collect id
    |> List.mapi (fun idx line ->
        if idx % 100 = 0
        then 
            printfn "%A" line
            1
        else 1
    )
    |> ignore
    System.Console.ReadLine() |> ignore
    0
