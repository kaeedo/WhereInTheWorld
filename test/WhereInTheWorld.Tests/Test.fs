namespace WhereInTheWorld.Tests

open Xunit
open Swensen.Unquote

type IoUtilitiesTests() =
    [<Fact>]
    member __.``Should do a thing`` () =
        test <@ 1 = 1 @>
