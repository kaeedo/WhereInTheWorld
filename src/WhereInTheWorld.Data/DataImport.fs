namespace WhereInTheWorld.Data

open System
open System.IO
open WhereInTheWorld.Utilities.ResultUtilities
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
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
        let file = baseDirectory @@ sprintf "%s.txt" countryCode
        let fileContents = File.ReadAllLines(file)

        File.Delete(file)

        Result.Ok fileContents

    let private splitLines (input: string[]) =
        let splitLines =
            input
            |> Seq.map (fun (line: string) ->
                line.Split('\t')
            )

        splitLines |> Result.Ok

    let private mapFileImport (lines: seq<string []>) =
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
              CommunityCode = parse string line.[8]
              Latitude = parse float line.[9]
              Longitude = parse float line.[10]
              Accuracy = parse int64 line.[11] }
        )
        |> Result.Ok

    let readPostalCodesFile filePath =
        let workflow = Job.lift readFile >=> Job.lift splitLines >=> Job.lift mapFileImport
        workflow filePath
