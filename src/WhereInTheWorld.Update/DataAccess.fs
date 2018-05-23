namespace WhereInTheWorld.Update

open Dapper
open Models
open Hopac
open System
open System.IO
open System.Data.SQLite
open System.Reflection
open System.Text

module DataAccess =
    type Codes = { Codes: seq<string> }

    let private baseDirectory =
        match Environment.OSVersion.Platform with
        | PlatformID.Unix -> Path.Combine(Environment.GetEnvironmentVariable("HOME"), "WhereInTheWorld")
        | PlatformID.MacOSX -> Path.Combine(Environment.GetEnvironmentVariable("HOME"), "WhereInTheWorld")
        | _ -> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "WhereInTheWorld")

    let private databaseFilename = sprintf "%s/world.db" baseDirectory
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = Utilities.safeSqlConnection connectionString

    let openConnection () = connection.Open()
    let closeConnection () = connection.Close()

    let ensureDatabase () =
        if not (Directory.Exists(baseDirectory))
        then Directory.CreateDirectory(baseDirectory) |> ignore
        if not (File.Exists(databaseFilename))
        // also check if DB exists, but not tables
        then
            SQLiteConnection.CreateFile(databaseFilename)

            let assembly = Assembly.GetExecutingAssembly()

            let resourceStream = assembly.GetManifestResourceStream("WhereInTheWorld.Update.sqlScripts.create.sql")

            use reader = new StreamReader(resourceStream, Encoding.UTF8)

            let sql = reader.ReadToEnd()

            connection.Execute(sql) |> ignore

    let getAllCountries =
        Job.fromTask <|
            fun () ->
                let codes =
                    DataDownload.supportedCountries
                    |> Seq.map (fun (code, _, _) ->
                        code
                    )

                connection.QueryAsync<Country>("SELECT * FROM Country WHERE Code IN @codes", { Codes = codes })

    let insertCountryGetId (country: Country) =
        let sql = "
            INSERT OR IGNORE INTO Country(Code, Name, LocalizedName)
            VALUES(@code, @name, @localizedName);
            SELECT Id FROM Country WHERE Code = @code;"

        Job.fromTask <| fun () -> connection.QueryFirstAsync<int>(sql, country)

    let insertPostalCodes (postalCodes: seq<PostalCode>) =
        job {
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

            do! Job.awaitUnitTask <| connection.ExecuteAsync(sql, postalCodes, transaction)

            transaction.Commit()
        }

    let insertSubdivisions (countryId: int) (subdivisions: seq<Subdivision>) =
        job {
            let transaction = connection.BeginTransaction()

            let values =
                subdivisions
                |> Seq.map (fun sd ->
                    { Id = 1; CountryId = countryId; Name = sd.Name; Code = sd.Code }
                )

            let sql = "
                INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
                VALUES (@countryId, @name, @code)"

            do! Job.awaitUnitTask <| connection.ExecuteAsync(sql, values, transaction)

            transaction.Commit()
        }

    let getSubdivisions (subdivisions: seq<Subdivision>) =
        Job.fromTask <|
            fun () ->
                let subdivisionCodes =
                    subdivisions
                    |> Seq.map (fun sd ->
                        sd.Code
                    )

                let sql = "
                    SELECT Id, CountryId, Code, Name
                    FROM Subdivision
                    WHERE Code IN @codes"

                connection.QueryAsync<Subdivision>(sql, { Codes = subdivisionCodes })
