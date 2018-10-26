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

    [<Fact>]
    member __.``When country with full postal codes exists, should query correctly`` () =
        Main.updateCountry "us"

        let postalCodeResult = Main.queryDatabase "16629"
        let cityNameResult = Main.queryDatabase "Embarrass"

        test <@ postalCodeResult.IsOk @>
        test <@ postalCodeResult.OkValue.[0].PlaceName = "Coupon" @>

        test <@ cityNameResult.IsOk @>
        test <@ cityNameResult.OkValue.[0].PostalCode = "55732" @>

    [<Fact>]
    member __.``When country with partial postal codes exists, should query postal codes correctly`` () =
        Main.updateCountry "ca"

        let result = Main.queryDatabase "H0H0H0"

        test <@ result.IsOk @>
        test <@ result.OkValue.[0].PlaceName = "Reserved (Santa Claus)" @>

    [<Fact>]
    member __.``When country with partial postal codes exists, should query partial postal codes correctly`` () =
        Main.updateCountry "ca"

        let result = Main.queryDatabase "H0H"

        test <@ result.IsOk @>
        test <@ result.OkValue.[0].PlaceName = "Reserved (Santa Claus)" @>

    [<Fact>]
    member __.``When country with partial postal codes exists, should city name correctly`` () =
        Main.updateCountry "ca"

        let result = Main.queryDatabase "Asbestos"

        test <@ result.IsOk @>
        test <@ result.OkValue.[0].PlaceName = "Asbestos" @>

