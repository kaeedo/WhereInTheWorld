namespace WhereInTheWorld.Update

open System.IO
open System.IO.Compression
open Hopac
open Hopac.Infixes
open HttpFs.Client
open WhereInTheWorld.Utilities.ResultUtilities
open WhereInTheWorld.Utilities.IoUtilities
open WhereInTheWorld.Utilities.Models

module DataDownload =
    let private baseUrl = "http://download.geonames.org/export/zip/"

    let private downloadZip countryCode =
        job {
            let! file =
                Request.createUrl Get (sprintf "%s%s.zip" baseUrl countryCode)
                |> Request.responseAsBytes

            return file |> Result.Ok
        }


    let private saveZip countryCode file =
        let filePath = baseDirectory @@ countryCode
        File.WriteAllBytes(sprintf "%s.zip" filePath, file)
        filePath |> Result.Ok

    let private saveCountryFile filePath =
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

    let downloadPostalCodesForCountry statusChannel countryCode =
        let workflow = downloadZip >=> Job.lift (saveZip countryCode) >=> Job.lift saveCountryFile
        job {
            do! statusChannel *<- (DownloadStatus.Started countryCode)

            let! result = workflow countryCode

            do! statusChannel *<- (Completed countryCode)

            return result
        }
