namespace WhereInTheWorld

open Argu

module ArgumentParser =
    type Arguments =
    | [<MainCommand; First>] PostalCode of postalCode: string
    | Update of countryCode: string option
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | PostalCode _ -> "Postal Code to look up. If update is specified, this will be ignored"
                | Update _ -> "Update the local database. Omit countryCode to download all"