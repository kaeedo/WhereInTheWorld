namespace WhereInTheWorld.Tests

open System
open System.IO

open Xunit
open Swensen.Unquote

open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities
open WhereInTheWorld.Data
open WhereInTheWorld

[<Collection("IntegrationTest")>]
type EndToEndTests() =
    do if File.Exists(databaseFile) then File.Delete(databaseFile)

    interface IDisposable with
        member __.Dispose() =
            if Directory.Exists(baseDirectory)
            then Directory.Delete(baseDirectory, true)

    [<Fact>]
    member __.``When listing countries should return country`` () =
        Main.updateCountry "ad"

        let countries = Query.getAvailableCountries()

        test <@ countries.IsOk @>
        test <@ countries.OkValue.IsSome @>

    [<Fact>]
    member __.``When listing countries in empty database should return none`` () =
        Database.ensureDatabase()

        let countries = Query.getAvailableCountries()

        test <@ countries.IsOk @>
        test <@ countries.OkValue.IsNone @>

    [<Fact>]
    member __.``When listing countries before database exists should return error`` () =
        let countries = Query.getAvailableCountries()

        test <@ countries.IsError @>