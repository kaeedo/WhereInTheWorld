namespace WhereInTheWorld.Data

open System.IO
open System.IO.Compression
open System.Net.Http
open Hopac
open Hopac.Infixes
open WhereInTheWorld.Utilities.ResultUtilities
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models
open WhereInTheWorld.Utilities
open System


module DataDownload =
    let private baseUrl = downloadUrl
    let downloadZip countryCode =
        job {
            let httpClient = new HttpClient()
            let! response = httpClient.GetByteArrayAsync(sprintf "%s%s.zip" baseUrl countryCode) |> Job.awaitTask
            return Result.Ok response
        }

    let saveZip countryCode file =
        Directory.CreateDirectory(baseDirectory) |> ignore
        let filePath = baseDirectory @@ countryCode
        File.WriteAllBytes(sprintf "%s.zip" filePath, file)
        filePath |> Result.Ok

    let saveCountryFile filePath =
        let zipFileName = sprintf "%s.zip" filePath
        let countryCode = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last

        let archive = ZipFile.OpenRead(zipFileName)

        archive.Entries
        |> Seq.find (fun zae ->
            zae.Name <> "readme.txt"
        )
        |> fun entry -> entry.ExtractToFile(sprintf "%s.txt" (baseDirectory @@ countryCode))
        archive.Dispose()

        File.Delete(zipFileName)

        Result.Ok filePath

    let supportedCountries =
        [ "AD", "Andorra"
          "AR", "Argentina"
          "AS", "American Samoa"
          "AT", "Austria"
          "AU", "Australia"
          "AX", "Åland Islands"
          "BD", "Bangladesh"
          "BE", "Belgium"
          "BG", "Bulgaria"
          "BM", "Bermuda"
          "BR", "Brazil"
          "BY", "Belarus"
          "CA", "Canada"
          "CH", "Switzerland"
          "CO", "Colombia"
          "CR", "Costa Rica"
          "CZ", "Czech Republic"
          "DE", "Germany"
          "DK", "Denmark"
          "DO", "Dominican Republic"
          "DZ", "Algeria"
          "ES", "Spain"
          "FI", "Finland"
          "FM", "Federated States of Micronesia"
          "FO", "Faroe Islands"
          "FR", "France"
          "GB", "Great Britain"
          "GF", "French Guiana"
          "GG", "Guernsey"
          "GL", "Greenland"
          "GP", "Guadeloupe"
          "GT", "Guatemala"
          "GU", "Guam"
          "HR", "Croatia"
          "HU", "Hungary"
          "IE", "Ireland"
          "IM", "Isle of Man"
          "IN", "India"
          "IS", "Iceland"
          "IT", "Italy"
          "JE", "Jersey"
          "JP", "Japan"
          "LI", "Liechtenstein"
          "LK", "Sri Lanka"
          "LT", "Lithuania"
          "LU", "Luxembourg"
          "LV", "Latvia"
          "MC", "Monaco"
          "MD", "Moldova"
          "MH", "Marshall Islands"
          "MK", "Macedonia"
          "MP", "Northern Mariana Islands"
          "MQ", "Martinique"
          "MT", "Malta"
          "MX", "Mexico"
          "MY", "Malaysia"
          "NC", "New Caledonia"
          "NL", "Netherlands"
          "NO", "Norway"
          "NZ", "New Zealand"
          "PH", "Philippines"
          "PK", "Pakistan"
          "PL", "Poland"
          "PM", "Saint Pierre and Miquelon"
          "PR", "Puerto Rico"
          "PT", "Portugal"
          "PW", "Palau"
          "RE", "Réunion"
          "RO", "Romania"
          "RU", "Russia"
          "SE", "Sweden"
          "SI", "Slovenia"
          "SJ", "Svalbard and Jan Mayen"
          "SK", "Slovakia"
          "SM", "San Marino"
          "TH", "Thailand"
          "TR", "Turkey"
          "UA", "Ukraine"
          "US", "United States of America"
          "UY", "Uruguay"
          "VA", "Holy See"
          "VI", "U.S. Virgin Islands"
          "WF", "Wallis and Futuna"
          "YT", "Mayotte"
          "ZA", "South Africa" ]
        |> Map.ofSeq

    let postalCodeFormats =
        IoUtilities.getEmbeddedResource "WhereInTheWorld.Data.countryInformationPostalCodes.txt"
        |> fun t -> t.Split([|Environment.NewLine|], StringSplitOptions.None)
        |> Seq.filter (fun pc -> not (String.IsNullOrWhiteSpace(pc)))
        |> Seq.map (fun pc ->
            let line = pc.Split('\t')
            line.[0], line.[2]
        )
        |> Map.ofSeq


    let downloadPostalCodesForCountry statusChannel countryCode =
        let workflow = downloadZip >=> Job.lift (saveZip countryCode) >=> Job.lift saveCountryFile
        let workflowResult =
            job {
                do! statusChannel *<- (DownloadStatus.Started <| supportedCountries.[countryCode])

                let! result = workflow countryCode

                if result.IsOk
                then do! statusChannel *<- (Completed <| supportedCountries.[countryCode])

                return result
            }
        Job.tryWith workflowResult (Job.lift Result.Error)
