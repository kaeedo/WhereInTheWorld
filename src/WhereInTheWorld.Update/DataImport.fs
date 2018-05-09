namespace WhereInTheWorld.Update

open System
open System.IO
open Models
open Utilities
open Hopac

module DataImport =
    let private baseSaveDirectory = "./temp"
    let private parse ctor str =
        if String.IsNullOrWhiteSpace(str)
        then None
        else
            try
                Some (ctor str)
            with
            | _ -> None

    let private readFile countryCode =
        job {
            try
                return Result.Ok <| File.ReadAllLines(sprintf "./%s/%s.txt" baseSaveDirectory countryCode)
            with
            | e -> return Result.Error e
        }

    let private splitLines (input: string[]) =
        job {
            let splitLines =
                input
                |> Seq.map (fun (line: string) ->
                    line.Split('\t')
                )

            return splitLines |> Result.Ok
        }

    let private mapFileImport (lines: seq<string []>) =
        job {
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
                return fileImport |> Result.Ok
            with
            | e -> return Result.Error e
        }

    let fileImport countryCode =
        let workflow = readFile >=> splitLines >=> mapFileImport
        workflow countryCode
