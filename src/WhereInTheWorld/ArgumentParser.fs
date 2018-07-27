namespace WhereInTheWorld

open Argu

type ListOptions =
| Supported
| Available

module ArgumentParser =
    type Arguments =
    | [<MainCommand>] PostalCode of postalCode: string
    | Update of countryCode: string option
    | List of ListOptions option
    | ClearDatabase

    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | PostalCode _ -> "Postal Code to look up. If the update option is specified, this will be ignored"
                | Update _ -> "Update the local database"
                | List _ -> "List available or all supported countries"
                | ClearDatabase -> "Clear the local database to start anew"
