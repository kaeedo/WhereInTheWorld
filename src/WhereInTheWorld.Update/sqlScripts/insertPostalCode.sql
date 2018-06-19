-- DELETE FROM PostalCode WHERE SubdivisionId IN
--     (SELECT Id FROM Subdivision WHERE CountryId = (SELECT Id FROM Country WHERE Code = @countryCode));

-- DELETE FROM Subdivision WHERE CountryId = (SELECT Id FROM Country WHERE Code = @countryCode);

-- DELETE FROM Country WHERE Code = @countryCode;

INSERT OR IGNORE INTO Country(Code, Name, LocalizedName)
VALUES(@countryCode, @countryName, @countryLocalizedName);

INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
VALUES ((SELECT Id FROM Country WHERE Code = @countryCode), @subdivisionName, @subdivisionCode);

INSERT OR IGNORE INTO PostalCode(
    PostalCode,
    PlaceName,
    SubdivisionId,
    CountyName,
    CountyCode,
    CommunityName,
    CommunityCode,
    Latitude,
    Longitude,
    Accuracy)
VALUES (
    @postalCode,
    @placeName,
    (SELECT Id FROM Subdivision WHERE Code = @subdivisionCode),
    @countyName,
    @countyCode,
    @communityName,
    @communityCode,
    @latitude,
    @longitude,
    @accuracy)
