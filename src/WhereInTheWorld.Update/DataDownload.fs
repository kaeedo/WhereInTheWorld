namespace WhereInTheWorld.Update

open System.IO
open System.IO.Compression
open Hopac
open HttpFs.Client
open Models
open Utilities
open System

module DataDownload =
    let private baseUrl = "http://download.geonames.org/export/zip/"

    let private downloadZip countryCode =
        job {
            try
                let! file =
                    Request.createUrl Get <| sprintf "%s%s.zip" baseUrl countryCode
                    |> Request.responseAsBytes

                return file |> Result.Ok
            with
            | e -> return Result.Error e
        }

    let private saveZip countryCode file =
        job {
            try
                let filePath = baseDirectory @@ countryCode
                File.WriteAllBytes(sprintf "%s.zip" filePath, file)
                return filePath |> Result.Ok
            with
            | e -> return Result.Error e
        }

    let private saveCountryFile filePath =
        job {
            try
                let zipFileName = sprintf "%s.zip" filePath
                let archive = ZipFile.OpenRead(zipFileName)
                let countryCode = filePath.Split(Path.DirectorySeparatorChar) |> Seq.last

                archive.Entries
                |> Seq.find (fun zae ->
                    zae.Name <> "readme.txt"
                )
                |> fun entry -> entry.ExtractToFile(sprintf "%s.txt" (baseDirectory @@ countryCode))
                archive.Dispose()

                File.Delete(zipFileName)

                return Result.Ok filePath
            with
            | e -> return Result.Error e
        }

    let supportedCountries =
        // [ "AT", "Austria", "Österreich"
        //   "BR", "Brazil", "Brasil"
        //   "IM", "Isle of Man", "Isle of Man"
        //   "LK", "Sri Lanka", "ශ්‍රී ලංකාව" ]
        [ "AD", "Andorra", "Andorra"
          "AR", "Argentina", ""
          "AS", "American Samoa", ""
          "AT", "Austria", ""
          "AU", "Australia", ""
          "AX", "Åland Islands", ""
          "BD", "Bangladesh", "" ]
        //   "BE", "Belgium", ""
        //   "BG", "Bulgaria", ""
        //   "BM", "Bermuda", ""
        //   "BR", "Brazil", ""
        //   "BY", "Belarus", ""
        //   "CA", "Canada", "Canada"
        //   "CH", "Switzerland", ""
        //   "CO", "Colombia", ""
        //   "CR", "Costa Rica", ""
        //   "CZ", "Czechia", ""
        //   "DE", "Germany", "Deutschland"
        //   "DK", "Denmark", ""
        //   "DO", "Dominican Republic", ""
        //   "DZ", "Algeria", ""
        //   "ES", "Spain", ""
        //   "FI", "Finland", ""
        //   "FM", "Federated States of Micronesia", ""
        //   "FO", "Faroe Islands", ""
        //   "FR", "France", ""
        //   "GB", "Great Britain", ""
        //   "GF", "French Guiana", ""
        //   "GG", "Guernsey", ""
        //   "GL", "Greenland", ""
        //   "GP", "Guadeloupe", ""
        //   "GT", "Guatemala", ""
        //   "GU", "Guam", ""
        //   "HR", "Croatia", "Hrvatska"
        //   "HU", "Hungary", ""
        //   "IE", "Ireland", ""
        //   "IM", "Isle of Man", ""
        //   "IN", "India", ""
        //   "IS", "Iceland", "Ísland"
        //   "IT", "Italy", ""
        //   "JE", "Jersey", ""
        //   "JP", "Japan", ""
        //   "LI", "Liechtenstein", ""
        //   "LK", "Sri Lanka", ""
        //   "LT", "Lithuania", ""
        //   "LU", "Luxembourg", ""
        //   "LV", "Latvia", ""
        //   "MC", "Monaco", ""
        //   "MD", "Moldova", ""
        //   "MH", "Marshall Islands", ""
        //   "MK", "Macedonia", ""
        //   "MP", "Northern Mariana Islands", ""
        //   "MQ", "Martinique", ""
        //   "MT", "Malta", ""
        //   "MX", "Mexico", ""
        //   "MY", "Malaysia", ""
        //   "NC", "New Caledonia", ""
        //   "NL", "Netherlands", ""
        //   "NO", "Norway", ""
        //   "NZ", "New Zealand", ""
        //   "PH", "Philippines", ""
        //   "PK", "Pakistan", ""
        //   "PL", "Poland", ""
        //   "PM", "Saint Pierre and Miquelon", ""
        //   "PR", "Puerto Rico", ""
        //   "PT", "Portugal", ""
        //   "PW", "Palau", ""
        //   "RE", "Réunion", ""
        //   "RO", "Romania", ""
        //   "RU", "Russia", ""
        //   "SE", "Sweden", ""
        //   "SI", "Slovenia", ""
        //   "SJ", "Svalbard and Jan Mayen", ""
        //   "SK", "Slovakia", ""
        //   "SM", "San Marino", ""
        //   "TH", "Thailand", ""
        //   "TR", "Turkey", ""
        //   "UA", "Ukraine", ""
        //   "US", "United States of America", "United States of America"
        //   "UY", "Uruguay", ""
        //   "VA", "Holy See", ""
        //   "VI", "U.S. Virgin Islands", ""
        //   "WF", "Wallis and Futuna", ""
        //   "YT", "Mayotte", ""
        //   "ZA", "South Africa", "" ]

    let downloadPostalCodesForCountry countryCode =
        let workflow = downloadZip >=> (saveZip countryCode) >=> saveCountryFile
        let workUnit =
            job {
                let stopWatch = System.Diagnostics.Stopwatch.StartNew()
                let! wu = workflow countryCode
                stopWatch.Stop()
                printfn "%s: Download took %fms" countryCode stopWatch.Elapsed.TotalMilliseconds

                return wu
            }
        workUnit
