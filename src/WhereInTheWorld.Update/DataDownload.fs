namespace WhereInTheWorld.Update

open Hopac
open HttpFs.Client

module DataDownload =
    let private baseUrl = "http://download.geonames.org/export/zip/"
    let supportedCountries =
        [ "CA", "Canada", "Canada"
          "DE", "Germany", "Deutschland"
          "US", "United States of America", "United States of America" ]

    let downloadZip countryCode =
        countryCode
