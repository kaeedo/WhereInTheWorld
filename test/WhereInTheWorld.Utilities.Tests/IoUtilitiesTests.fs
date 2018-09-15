namespace WhereInTheWorld.Utilities.Tests

open System
open System.IO
open Xunit
open Swensen.Unquote
open Hedgehog
open WhereInTheWorld.Utilities.IoUtilities

type IoUtilitiesTests() =
    [<Fact>]
    member __.``Path combine infix should have same total length`` () =
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
    member __.``Path combine infix should contain each part`` () =
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

    [<Fact>]
    member __.``Parse tsv should have list length equal to number of lines`` () =
        property {
            let! listOfParts =
                Gen.char 'a' 'z'
                |> (Range.constant 1 9
                    |> Gen.string)
                |> Gen.tuple
                |> Gen.list (Range.constant 10 20)

            let fileContens =
                listOfParts
                |> Seq.map (fun p -> sprintf "%s\t%s" (fst p) (snd p))
                |> String.concat (Environment.NewLine)

            test <@ listOfParts |> Seq.length = (parseTsv fileContens |> Seq.length) @>
        }
        |> Property.check
