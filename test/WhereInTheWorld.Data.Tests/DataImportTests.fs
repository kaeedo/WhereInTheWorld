namespace WhereInTheWorld.Data.Tests

open System.IO
open Xunit
open Swensen.Unquote
open Hopac
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities

type DataImportTests() =
    let validCountryFile = getEmbeddedResource "WhereInTheWorld.Data.Tests.AD.txt"
    let invalidCountryFile = getEmbeddedResource "WhereInTheWorld.Data.Tests.ADbad.txt"

    let createTestImportFile fileName contents =
        Directory.CreateDirectory(baseDirectory) |> ignore
        File.WriteAllLines(baseDirectory @@ fileName, contents)

    [<Fact>]
    member __.``Reading from valid file should give Ok result`` () =
        createTestImportFile "AD.txt" (validCountryFile |> String.split('\n') |> Seq.toArray)

        let workflowResult = DataImport.readPostalCodeFile "AD" |> run

        test <@ workflowResult.IsOk @>

    [<Fact>]
    member __.``Reading from invalid file should give error result`` () =
        createTestImportFile "ADbad.txt" (invalidCountryFile |> String.split('\n') |> Seq.toArray)

        let workflowResult = DataImport.readPostalCodeFile "AD" |> run

        test <@ workflowResult.IsError @>