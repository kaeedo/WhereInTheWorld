namespace WhereInTheWorld.Utilities.Tests

open System.IO
open Xunit
open Swensen.Unquote
open Hedgehog
open WhereInTheWorld.Utilities.IoUtilities

module IoUtilitiesTests =
    [<Fact>]
    let ``Path combine infix should have same total length`` () =
        property {
            let! (firstPath, secondPath) = 
                Gen.char 'a' 'z'
                |> (Range.constant 1 9
                    |> Gen.string) 
                |> Gen.tuple

            let actual = firstPath @@ secondPath
            let totalLength = (firstPath + secondPath + Path.PathSeparator.ToString()).Length

            test <@ actual.Length = totalLength @>
        }
        |> Property.check

    [<Fact>]
    let ``Path combine infix should contain each part`` () =
        property {
            let! (firstPath, secondPath) = 
                Gen.char 'a' 'z'
                |> (Range.constant 0 9
                    |> Gen.string) 
                |> Gen.tuple

            let actual = firstPath @@ secondPath

            test <@ actual.Contains(firstPath) @>
            test <@ actual.Contains(secondPath) @>
        }
        |> Property.check