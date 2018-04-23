namespace WhereInTheWorld.Update

open System
open System.Data.SQLite
open Dapper

[<CLIMutable>]
type Test =
    { Id: int
      Name: string }


module Update =
    let private databaseFilename = ".\world.db"
    let private connectionString = sprintf "Data Source=%s;Version=3" databaseFilename
    let private connection = new SQLiteConnection(connectionString)

    connection.Open()

    let supportedCountries =
        [ "CA", ("Canada", "Canada")
          "DE", ("Germany", "Deutschland")
          "US", ("United States of America", "United States of America") ]
        |> dict

    let addCountries =
        let sql =
            supportedCountries
            |> Seq.map (fun kvp ->
                let name, localizedName = kvp.Value
                sprintf "('%s', '%s', '%s')" kvp.Key name localizedName
                )
            |> String.concat ","

        connection.Execute("INSERT INTO 'Country' ('Code', 'Name', 'LocalizedName') VALUES " + sql) |> ignore
