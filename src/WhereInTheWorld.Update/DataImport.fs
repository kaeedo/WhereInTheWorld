namespace WhereInTheWorld.Update

open System
open System.IO
open Models
open Utilities

module DataImport =
    let private parse ctor str =
        if String.IsNullOrWhiteSpace(str)
        then None
        else
            try
                Some (ctor str)
            with
            | _ -> None

    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let fileImport countryCode =
        let readFile countryCode =
            try
                Result.Ok <| File.ReadAllLines(sprintf "./rawInput/%s.txt" countryCode)
            with
            | e -> Result.Error e

        let splitLines =
            fun (input: string[]) ->
                input
                |> Seq.map (fun (line: string) ->
                    line.Split('\t')
                )
            |> Result.map

        let mapFileImport (lines: seq<string []>) =
            try
                lines
                |> Seq.map (fun line ->
                    { CountryCode = line.[0]
                      PostalCode = line.[1]
                      PlaceName = line.[2]
                      SubdivisionName = line.[3]
                      SubdivisionCode = line.[4]
                      CountyName = parse string line.[5]
                      CountyCode = parse string line.[6]
                      CommunityName = parse string line.[7]
                      CommunityCode = parse int line.[8]
                      Latitude = parse float line.[9]
                      Longitude = parse float line.[10]
                      Accuracy = parse int line.[11] }
                ) |> Result.Ok
            with
            | e -> Result.Error e

        (readFile
        >> splitLines) countryCode
        >=> mapFileImport
