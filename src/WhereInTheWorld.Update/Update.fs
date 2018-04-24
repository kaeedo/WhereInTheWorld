namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper

[<CLIMutable>]
type Country =
    { Id: int
      Code: string
      Name: string
      LocalizedName: string }


module Update =
    let private databaseFilename = "./world.db"
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = new SQLiteConnection(connectionString)

    connection.Open()

    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let getAllCountries () =
        let sql =
            supportedCountries
            |> Seq.map (fun (code, _, _) ->
                sprintf "'%s'" code
            )
            |> String.concat ","

        connection.Query<Country>(sprintf "SELECT * FROM Country WHERE Code IN (%s)" sql)

    let addCountries () =
        let allCountries = getAllCountries ()
        let sql =
            supportedCountries
            |> Seq.filter (fun (code, _, _) ->
                allCountries
                |> Seq.exists (fun ac ->
                    ac.Code = code
                )
                |> not
            )
            |> Seq.map (fun countryData ->
                let code, name, localizedName = countryData
                sprintf "('%s', '%s', '%s')" code name localizedName
                )
            |> String.concat ","

        if String.IsNullOrWhiteSpace(sql)
        then ()
        else
            connection.Execute("INSERT INTO 'Country' ('Code', 'Name', 'LocalizedName') VALUES " + sql) |> ignore
