namespace WhereInTheWorld.Update

open System
open System.IO
open Models
open Utilities
open Hopac

module DataImport =
    let private parse ctor str =
        if String.IsNullOrWhiteSpace(str)
        then None
        else
            try
                Some (ctor str)
            with
            | _ -> None

    let private readFile countryCode =
        try
            Result.Ok <| File.ReadAllLines(baseDirectory @@ sprintf "%s.txt" countryCode)
        with
        | e -> Result.Error e

    let private splitLines (input: string[]) =
        let splitLines =
            input
            |> Seq.map (fun (line: string) ->
                line.Split('\t')
            )

        splitLines |> Result.Ok

    let private mapFileImport (lines: seq<string []>) =
            try
                let fileImport =
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
                    )
                fileImport |> Result.Ok
            with
            | e -> Result.Error e

    let readPostalCodesFile countryCode =
        let bind fn value =
            match value with
            | Ok v -> fn v
            | Error err -> Error err

        let workflow = readFile >> bind splitLines >> bind mapFileImport
        workflow countryCode
