INSERT OR IGNORE INTO Country(Code, Name)
VALUES(@countryCode, @countryName);

INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
VALUES ((SELECT Id FROM Country WHERE Code = @countryCode), @subdivisionName, @subdivisionCode);

INSERT OR IGNORE INTO PostalCode(
    PostalCode,
    PlaceName,
    SubdivisionId,
    CountyName,
    CountyCode,
    CommunityName,
    CommunityCode)
VALUES (
    @postalCode,
    @placeName,
    (SELECT Id FROM Subdivision WHERE Code = @subdivisionCode),
    @countyName,
    @countyCode,
    @communityName,
    @communityCode)
