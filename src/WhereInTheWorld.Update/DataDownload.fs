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
    // let private baseUrl = @"http://localhost:8080/"

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
        [ "AD", "Andorra", "Andorra"
          "AR", "Argentina", "Argentina"
          "AS", "American Samoa", "Amerika Sāmoa"
          "AT", "Austria", "Österreich"
          "AU", "Australia", "Australia"
          "AX", "Åland Islands", "Åland"
          "BD", "Bangladesh", "বাংলাদেশ"
          "BE", "Belgium", "België"
          "BG", "Bulgaria", "България"
          "BM", "Bermuda", "Bermuda"
          "BR", "Brazil", "Brasil"
          "BY", "Belarus", "Беларусь"
          "CA", "Canada", "Canada"
          "CH", "Switzerland", "Schweiz"
          "CO", "Colombia", "Colombia"
          "CR", "Costa Rica", "Costa Rica"
          "CZ", "Czech Republic", "Česká republika"
          "DE", "Germany", "Deutschland"
          "DK", "Denmark", "Danmark"
          "DO", "Dominican Republic", "República Dominicana"
          "DZ", "Algeria", "الجزائر"
          "ES", "Spain", "España"
          "FI", "Finland", "Suomi"
          "FM", "Federated States of Micronesia", "Federated States of Micronesia"
          "FO", "Faroe Islands", "Føroyar"
          "FR", "France", "France"
          "GB", "Great Britain", "Great Britain"
          "GF", "French Guiana", "Guyane française"
          "GG", "Guernsey", "Guernsey"
          "GL", "Greenland", "Kalaallit Nunaat"
          "GP", "Guadeloupe", "Guadeloupe"
          "GT", "Guatemala", "Guatemala"
          "GU", "Guam", "Guam"
          "HR", "Croatia", "Hrvatska"
          "HU", "Hungary", "Magyarország"
          "IE", "Ireland", "Ireland"
          "IM", "Isle of Man", "Isle of Man"
          "IN", "India", "भारत"
          "IS", "Iceland", "Ísland"
          "IT", "Italy", "Italia"
          "JE", "Jersey", "Jersey"
          "JP", "Japan", "日本"
          "LI", "Liechtenstein", "Liechtenstein"
          "LK", "Sri Lanka", "ශ්‍රී ලංකාව"
          "LT", "Lithuania", "Lietuva"
          "LU", "Luxembourg", "Lëtzebuerg"
          "LV", "Latvia", "Latvija"
          "MC", "Monaco", "Monaco"
          "MD", "Moldova", "Moldova"
          "MH", "Marshall Islands", "Aolepān Aorōkin M̧ajeļ"
          "MK", "Macedonia", "Македонија"
          "MP", "Northern Mariana Islands", "Northern Mariana Islands"
          "MQ", "Martinique", "Martinique"
          "MT", "Malta", "Malta"
          "MX", "Mexico", "México"
          "MY", "Malaysia", "Malaysia"
          "NC", "New Caledonia", "Nouvelle-Calédonie"
          "NL", "Netherlands", "Nederland"
          "NO", "Norway", "Norge"
          "NZ", "New Zealand", "New Zealand"
          "PH", "Philippines", "Pilipinas"
          "PK", "Pakistan", "پاکِستان‬‎"
          "PL", "Poland", "Polska"
          "PM", "Saint Pierre and Miquelon", "Saint-Pierre-et-Miquelon"
          "PR", "Puerto Rico", "Puerto Rico"
          "PT", "Portugal", "Portugal"
          "PW", "Palau", "Palau"
          "RE", "Réunion", "Réunion"
          "RO", "Romania", "România"
          "RU", "Russia", "Росси́я"
          "SE", "Sweden", "Sverige"
          "SI", "Slovenia", "Slovenija"
          "SJ", "Svalbard and Jan Mayen", "Svalbard og Jan Mayen"
          "SK", "Slovakia", "Slovensko"
          "SM", "San Marino", "San Marino"
          "TH", "Thailand", "ประเทศไทย"
          "TR", "Turkey", "Türkiye"
          "UA", "Ukraine", "Україна"
          "US", "United States of America", "United States of America"
          "UY", "Uruguay", "Uruguay"
          "VA", "Holy See", "Sancta Sedes"
          "VI", "U.S. Virgin Islands", "U.S. Virgin Islands"
          "WF", "Wallis and Futuna", "Wallis-et-Futuna"
          "YT", "Mayotte", "Mayotte"
          "ZA", "South Africa", "Suid-Afrika" ]

    let downloadPostalCodesForCountry jobStatusChannel countryCode =
        let workflow = downloadZip >=> (saveZip countryCode) >=> saveCountryFile
        job {
            let! result = workflow countryCode

            do! Ch.give jobStatusChannel (Completed countryCode)

            return result
        }
