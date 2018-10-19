SELECT
    c.Code AS 'CountryCode',
    c.Name AS 'CountryName',
    pc.PostalCode AS 'PostalCode',
    pc.PlaceName AS 'PlaceName',
    s.Code AS 'SubdivisionCode',
    s.Name AS 'SubdivisionName',
    pc.CountyName AS 'CountyName',
    pc.CountyCode AS 'CountyCode',
    pc.CommunityName AS 'CommunityName',
    pc.CommunityCode AS 'CommunityCode'
FROM PostalCode pc
JOIN Subdivision s on pc.SubdivisionId = s.Id
JOIN Country c on s.CountryId = c.Id
WHERE UPPER(REPLACE(pc.PlaceName, ' ', '')) like @Input || '%'
