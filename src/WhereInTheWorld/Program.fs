open System
open WhereInTheWorld.Update

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    printfn "%O" <| Update.get
    0 // return an integer exit code
