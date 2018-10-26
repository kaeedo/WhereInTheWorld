# Where in the World
.net Core Global tool to get information about a place either from Postal Codes or city name.

All data acquired from: www.geonames.org licensed under Creative Commons 3

There exists a secondary support repository for mirroring zip files from geonames, as well as creating lists of supported countries and additional information: https://github.com/kaeedo/WhereInTheWorldSupport/

# How to use

### Install
* `dotnet tool install -g WhereInTheWorld`

### Usage

	USAGE: witw [--help] [--update [<countryCode>]] [--list [<supported|available>]]
				[--cleardatabase] [--info] [<postalCode>]

	SEARCHQUERY:

		<postalCode>          Postal code. Wrap in quotes to search by city name

	OPTIONS:

		--update [<countryCode>]
							  Update the local database
		--list [<supported|available>]
							  List available or all supported countries
		--cleardatabase       Clear the local database to start anew
		--help                display this list of options.



### Examples

    witw --update us

	witw 10001
		Information about "10001" (found 1 result):
		--------------------------------------------------
		Place name: New York with postal code: 10001
			In County: New York (061)
			Within Subdivision: New York (NY)
			In Country: United States of America (US)
		-------------------------

	witw "Casper"
        Information about "Casper" (found 1 result):
        --------------------------------------------------
        Place name: Casper has following postal codes:
        82601, 82602, 82604, 82605, 82609
            In County: Natrona (025)
            Within Subdivision: Wyoming (WY)
            In Country: United States of America (US)
        -------------------------
