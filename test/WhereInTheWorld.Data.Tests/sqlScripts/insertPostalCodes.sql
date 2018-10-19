INSERT OR IGNORE INTO Country(Code, Name)
VALUES(@CountryCode, @CountryName);

INSERT OR IGNORE INTO Subdivision(CountryId, Name, Code)
VALUES ((SELECT Id FROM Country WHERE Code = @CountryCode), @SubdivisionName, @SubdivisionCode);

INSERT OR IGNORE INTO PostalCode(
    PostalCode,
    PlaceName,
    SubdivisionId,
    CountyName,
    CountyCode,
    CommunityName,
    CommunityCode)
VALUES (
    @PostalCode,
    @PlaceName,
    (SELECT Id FROM Subdivision WHERE Code = @SubdivisionCode),
    @CountyName,
    @CountyCode,
    @CommunityName,
    @CommunityCode)
