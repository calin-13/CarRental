CREATE PROCEDURE GetRevenueByCarCategory
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE 
            WHEN c.DailyRate < 100 THEN 'Economy'
            WHEN c.DailyRate BETWEEN 100 AND 300 THEN 'Standard'
            WHEN c.DailyRate > 300 THEN 'Luxury'
        END AS Category,
        COUNT(r.Id) AS TotalReservations,
        SUM(r.TotalCost) AS TotalRevenue,
        AVG(r.TotalCost) AS AverageRevenue
    FROM Reservations r
    INNER JOIN Cars c ON r.CarId = c.Id
    WHERE r.IsActive = 1
    GROUP BY 
        CASE 
            WHEN c.DailyRate < 100 THEN 'Economy'
            WHEN c.DailyRate BETWEEN 100 AND 300 THEN 'Standard'
            WHEN c.DailyRate > 300 THEN 'Luxury'
        END
    ORDER BY TotalRevenue DESC;
END
GO

-- Procedura 2: Top clienti dupa numar de rezervari
CREATE PROCEDURE GetTopClientsByReservations
    @TopCount INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopCount)
        c.Id,
        c.FirstName,
        c.LastName,
        c.Email,
        COUNT(r.Id) AS TotalReservations,
        SUM(r.TotalCost) AS TotalSpent,
        MAX(r.EndDate) AS LastReservationDate
    FROM Clients c
    INNER JOIN Reservations r ON c.Id = r.ClientId
    WHERE r.IsActive = 1
    GROUP BY c.Id, c.FirstName, c.LastName, c.Email
    ORDER BY TotalReservations DESC, TotalSpent DESC;
END
GO
