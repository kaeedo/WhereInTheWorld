open System
open WhereInTheWorld.Update

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    printfn "%O" <| Update.addCountries
    0 // return an integer exit code
