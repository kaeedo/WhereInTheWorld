namespace WhereInTheWorld.Update

open System.Data.SQLite
open Dapper
open Models

module DataAccess =
    type Codes = { Codes: seq<string> }

    let private databaseFilename = "./world.db"
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = Utilities.safeSqlConnection connectionString

    connection.Open()

    let getAllCountries () =
        let codes =
            DataImport.supportedCountries
            |> Seq.map (fun (code, _, _) ->
                code
            )

        connection.Query<Country>("SELECT * FROM Country WHERE Code IN @codes", { Codes = codes })

    let insertCountryGetId (country: Country) =
        let sql = "
            INSERT OR IGNORE INTO Country(Code, Name, LocalizedName)
            VALUES(@code, @name, @localizedName);
            SELECT Id FROM Country WHERE Code = @code;"

        connection.QueryFirst<int>(sql, country)

    let insertPostalCodes (postalCodes: seq<PostalCode>) =
        let transaction = connection.BeginTransaction()

        let sql = "
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
                @subdivisionId,
                @countyName,
                @countyCode,
                @communityName,
                @communityCode,
                @latitude,
                @longitude,
                @accuracy)"

        connection.Execute(sql, postalCodes, transaction) |> ignore

        transaction.Commit()

    let insertSubdivisions (countryId: int) (subdivisions: seq<Subdivision>) =
        let transaction = connection.BeginTransaction()

        let values =
            subdivisions
            |> Seq.map (fun sd ->
                { Id = 1; CountryId = countryId; Name = sd.Name; Code = sd.Code }
            )

        let sql = "
            INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
            VALUES (@countryId, @name, @code)"

        connection.Execute(sql, values, transaction) |> ignore

        transaction.Commit()

    let getSubdivisions (subdivisions: seq<Subdivision>) =
        let subdivisionCodes =
            subdivisions
            |> Seq.map (fun sd ->
                sd.Code
            )

        let sql = "
            SELECT Id, CountryId, Code, Name
            FROM Subdivision
            WHERE Code IN @codes"

        connection.Query<Subdivision>(sql, { Codes = subdivisionCodes })
