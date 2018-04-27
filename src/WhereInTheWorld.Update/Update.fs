namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper
open Models

module DataAccess =
    let private databaseFilename = "C:/Users/kait/dev/WhereInTheWorld/world.db"
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = new SQLiteConnection(connectionString)

    connection.Open()

    let getAllCountries () =
        let sql =
            DataImport.supportedCountries
            |> Seq.map (fun (code, _, _) ->
                sprintf "'%s'" code
            )
            |> String.concat ","

        connection.Query<Country>(sprintf "SELECT * FROM Country WHERE Code IN (%s)" sql)

    let insertCountry (country: Country) =
        let sql = sprintf "
            INSERT INTO Country(%s, %s, %s)
            SELECT %s
            WHERE NOT EXISTS(SELECT 1 FROM Country WHERE Code = %s)"
                    country.Code country.Name country.LocalizedName country.Code country.Code

        connection.Execute(sql)

    let insertSubdivision (subdivision: Subdivision) =
        let sql = sprintf "
            INSERT INTO Subdivision(%s, %s)
            SELECT %s
            WHERE NOT EXISTS(SELECT 1 FROM Country WHERE Code = %s)"
                    subdivision.Code subdivision.Name subdivision.Code subdivision.Code

        connection.Execute(sql)
