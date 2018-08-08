namespace WhereInTheWorld.Data.Tests

open System
open System.IO
open Xunit
open Swensen.Unquote
open Hopac
open WhereInTheWorld.Data
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities

type DataImportTests() =
    let createTestImportFile fileName =
        Directory.CreateDirectory(baseDirectory) |> ignore
        File.Copy(Directory.GetCurrentDirectory() @@ fileName, baseDirectory @@ fileName)
        File.Move(baseDirectory @@ fileName, baseDirectory @@ "AD.txt")

    interface IDisposable with
        member __.Dispose() = File.Delete(baseDirectory @@ "AD.txt")

    [<Fact>]
    member __.``Reading from valid file should give Ok result`` () =
        createTestImportFile "AD.txt"

        let workflowResult = DataImport.readPostalCodeFile "AD" |> run

        test <@ workflowResult.IsOk @>

    [<Fact>]
    member __.``Reading from invalid file should give error result`` () =
        createTestImportFile "ADbad.txt"

        let workflowResult = DataImport.readPostalCodeFile "AD" |> run

        test <@ workflowResult.IsError @>
