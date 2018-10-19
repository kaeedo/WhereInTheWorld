namespace WhereInTheWorld.Data.Tests

open System
open System.IO

open Dapper
open Xunit
open Swensen.Unquote

open WhereInTheWorld.Data
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities.ResultUtilities
open Hopac

[<Collection("IntegrationTest")>]
type DatabaseTests() =
    do if File.Exists(databaseFile) then File.Delete(databaseFile)

    let executeScript sql data =
        let connection = Database.safeSqlConnection Database.connectionString
        connection.Open()
        let transaction = connection.BeginTransaction()

        connection.Execute(sql, data, transaction) |> ignore

        transaction.Commit()
        connection.Close()

    interface IDisposable with
        member __.Dispose() =
            if Directory.Exists(baseDirectory)
            then Directory.Delete(baseDirectory, true)

    [<Fact>]
    member __.``When no database should create database file`` () =
        Database.ensureDatabase()

        test <@ File.Exists(databaseFile) @>

    [<Fact>]
    member __.``When querying for countries when some exist should return countries`` () =
        Database.ensureDatabase()

        let insertCountriesSql = """
        INSERT OR IGNORE INTO Country(Code, Name)
        VALUES(@Code, @Name);
        """

        let expected =
            [ { Country.Code = "DE"; Name = "Germany" }
              { Code = "US"; Name = "United States of America" } ]

        executeScript insertCountriesSql expected

        let result = Query.getAvailableCountries()

        test <@ result.IsOk @>
        test <@ result.OkValue.IsSome @>

        let actual = result.OkValue.Value

        test <@ actual.Count = expected.Length @>
        test <@ actual.Item "DE" = "Germany" @>


    [<Fact>]
    member __.``When querying for countries in empty database should return none`` () =
        Database.ensureDatabase()

        let countries = Query.getAvailableCountries()

        test <@ countries.IsOk @>
        test <@ countries.OkValue.IsNone @>

    [<Fact>]
    member __.``When querying for countries when database doesn't exist should return error`` () =
        let countries = Query.getAvailableCountries()

        test <@ countries.IsError @>

    [<Fact>]
    member __.``When querying for city names when exist should return city`` () =
        Database.ensureDatabase()

        let sql = WhereInTheWorld.Utilities.IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.Tests.sqlScripts.insertPostalCodes.sql"

        let expected =
            [ { PostalCodeInformation.CountryCode = "DE"
                CountryName = "Deutschland"
                PostalCode = "10243"
                PlaceName = "Berlin"
                SubdivisionCode = ""
                SubdivisionName = ""
                CountyName = None
                CountyCode = None
                CommunityName = None
                CommunityCode = None } ]

        executeScript sql expected

        let result = Query.getCityNameInformation "berlin"

        test <@ result.IsOk @>
        test <@ result.OkValue.Length = 1 @>

        let actual = result.OkValue.Head

        test <@ actual.PlaceName = "Berlin" @>

    [<Fact>]
    member __.``When querying for partial postal codes when exist should return city`` () =
        Database.ensureDatabase()

        let sql = WhereInTheWorld.Utilities.IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.Tests.sqlScripts.insertPostalCodes.sql"

        let expected =
            [ { PostalCodeInformation.CountryCode = "DE"
                CountryName = "Deutschland"
                PostalCode = "10243"
                PlaceName = "Berlin"
                SubdivisionCode = ""
                SubdivisionName = ""
                CountyName = None
                CountyCode = None
                CommunityName = None
                CommunityCode = None } ]

        executeScript sql expected

        let result = Query.getPostalCodeInformation "102"

        test <@ result.IsOk @>
        test <@ result.OkValue.Length = 1 @>

        let actual = result.OkValue.Head

        test <@ actual.PlaceName = "Berlin" @>

    [<Fact>]
    member __.``When inserting postal codes should insert`` () =
        Database.ensureDatabase()

        let expected =
            [ { PostalCodeInformation.CountryCode = "DE"
                CountryName = "Deutschland"
                PostalCode = "10243"
                PlaceName = "Berlin"
                SubdivisionCode = ""
                SubdivisionName = ""
                CountyName = None
                CountyCode = None
                CommunityName = None
                CommunityCode = None } ]

        DataAccess.insertPostalCodes expected |> run |> ignore

        let inserted = Query.getPostalCodeInformation "10243"

        test <@ inserted.IsOk @>
        test <@ inserted.OkValue.Length = 1 @>

        let actual = inserted.OkValue.Head

        test <@ actual.PlaceName = "Berlin" @>
