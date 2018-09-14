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
    | Info

    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | PostalCode _ -> "Postal Code to look up. If the update option is specified, this will be ignored"
                | Update _ -> "Update the local database"
                | List _ -> "List available or all supported countries"
                | ClearDatabase -> "Clear the local database to start anew"
                | Info -> "Show information about WhereInTheWorld"

    let (|ShowInformation|HasQuery|UpdateCountry|ListAvailable|ListSupported|HasClearDatabase|HelpRequested|) (input: ParseResults<Arguments>) =
        if input.Contains Info then ShowInformation
        elif input.Contains PostalCode then HasQuery
        elif input.Contains Update then UpdateCountry
        elif input.Contains List
        then
            let listRequest = input.GetResult List
            let isAvailableRequest = listRequest |> Option.exists (fun l -> l = Available)
            let isSupportedRequest = listRequest |> Option.exists (fun l -> l = Supported)

            if not isAvailableRequest && not isSupportedRequest
            then HelpRequested
            elif isAvailableRequest
            then ListAvailable
            else ListSupported
        elif input.Contains ClearDatabase then HasClearDatabase
        else HelpRequested
