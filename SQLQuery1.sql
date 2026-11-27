BEGIN TRANSACTION;

-- Safety check: make sure no prescription uses these drugs
IF EXISTS (
    SELECT 1
    FROM Prescriptions p
    INNER JOIN Drugs d ON p.DrugId = d.DrugID
    WHERE LEN(d.DrugID) = 11
)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, 'Some drugs are already used in prescriptions. Update cancelled.', 1;
END;

-- Standardize Drug IDs to DRG + 7 digits
UPDATE Drugs
SET DrugID =
    'DRG' +
    RIGHT(
        '0000000' +
        CAST(CAST(SUBSTRING(DrugID, 4, 8) AS INT) AS VARCHAR(7)),
        7
    )
WHERE LEN(DrugID) = 11;

COMMIT TRANSACTION;