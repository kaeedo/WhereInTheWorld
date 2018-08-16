namespace WhereInTheWorld.Data

open Dapper
open System
open System.IO
open Hopac
open System.Data.SQLite
open WhereInTheWorld.Utilities
open WhereInTheWorld.Utilities.Models

type OptionHandler<'T>() =
    inherit SqlMapper.TypeHandler<option<'T>>()
    override __.SetValue(param, value) =
        let valueOrNull =
            match value with
            | Some x -> box x
            | None -> null
        param.Value <- valueOrNull
    override __.Parse value =
        if isNull value || value = box DBNull.Value
        then None
        else Some (value :?> 'T)

module Query =
    let a = 1

type DataAccess() =
    let connectionString = sprintf "Data Source=%s;Version=3" databaseFile

    let safeSqlConnection (connectionString: string) =
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        new SQLiteConnection(connectionString)

    member __.ClearDatabase () =
        if File.Exists(databaseFile)
        then File.Delete(databaseFile)

    member __.EnsureDatabase () =
        if not (File.Exists(databaseFile))
        then
            SQLiteConnection.CreateFile(databaseFile)
            let connection = safeSqlConnection connectionString
            connection.Open()
            let sql = IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.sqlScripts.createTables.sql"
            connection.Execute(sql) |> ignore
            connection.Close()
    member __.InsertPostalCodes (postalCodes: PostalCodeInformation list) =
        job {
            let connection = safeSqlConnection connectionString
            let transaction = connection.BeginTransaction()

            let sql = """
            INSERT OR IGNORE INTO Country(Code, Name)
    VALUES(@countryCode, @countryName);

    INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
    VALUES ((SELECT Id FROM Country WHERE Code = @countryCode), @subdivisionName, @subdivisionCode);

    INSERT OR IGNORE INTO PostalCode(
        PostalCode,
        PlaceName,
        SubdivisionId,
        CountyName,
        CountyCode,
        CommunityName,
        CommunityCode,
        Latitude,
        Longitude,
        Accuracy)
    VALUES (
        @postalCode,
        @placeName,
        (SELECT Id FROM Subdivision WHERE Code = @subdivisionCode),
        @countyName,
        @countyCode,
        @communityName,
        @communityCode,
        @latitude,
        @longitude,
    @accuracy)
            """
            connection.ExecuteAsync(sql, postalCodes, transaction) |> ignore

            transaction.Commit()
        }
