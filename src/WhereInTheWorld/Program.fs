open System
open WhereInTheWorld.Update

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    printfn "%A" <| Update.getAllCountries()
    0 // return an integer exit code
