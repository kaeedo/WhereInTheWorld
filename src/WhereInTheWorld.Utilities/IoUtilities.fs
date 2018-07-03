namespace WhereInTheWorld.Utilities

open System.IO

module IoUtilities =
    let (@@) a b = Path.Combine(a, b)
