namespace WhereInTheWorld.Update

open System
open System.IO
open Models

module DataImport =
    let private parse ctor str =
        if String.IsNullOrWhiteSpace(str)
        then None
        else Some (ctor str)

    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let fileImport countryCode =
        File.ReadAllLines(sprintf "./rawInput/%s.txt" countryCode)
        |> Seq.map (fun line ->
            line.Split('\t')
        )
        |> Seq.map (fun line ->
            // TODO: RoPify
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
        |> List.ofSeq
