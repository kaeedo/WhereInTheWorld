namespace WhereInTheWorld.Update

open System

module Models =
    [<CLIMutable>]
    type Country =
        { Id: int
          Code: string
          Name: string
          LocalizedName: string }

    [<CLIMutable>]
    type Subdivision =
        { Id: int
          Code: string
          Name: string }

    [<CLIMutable>]
    type PostalCode =
        { Id: int
          PostalCode: string
          PlaceName: string
          CountryId: int
          SubdivisionId: int
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: int option
          Latitude: float option
          Longitude: float option
          Accuracy: int option }

    type Information =
        { Country: Country
          Subdivision: Subdivision
          PostalCode: PostalCode }


    type FileImport =
        { CountryCode: string
          PostalCode: string
          PlaceName: string
          SubdivisionName: string
          SubdivisionCode: string
          CountyName: string option
          CountyCode: string option
          CommunityName: string option
          CommunityCode: int option
          Latitude: float option
          Longitude: float option
          Accuracy: int option }

    type Errors =
        | UnableToReadInput of Exception