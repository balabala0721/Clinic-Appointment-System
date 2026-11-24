INSERT INTO Drugs
(DrugId, DrugName, DrugCategory, MedicalUses,
 UnitPrice, StockQuantity, StockStatus, MinStockLevel)
VALUES
(1001, 'Paracetamol', 0, 'Relief of pain and fever', 5.50, 100, 0, 20),
(1002, 'Amoxicillin', 1, 'Treatment of bacterial infections', 12.00, 10, 1, 20),
(1003, 'Cetirizine', 2, 'Relief of allergy symptoms', 8.50, 0, 2, 10),
(1004, 'Ibuprofen', 0, 'Relief of pain and inflammation', 7.00, 50, 0, 15),
(1005, 'Omeprazole', 4, 'Treatment of acid reflux', 15.00, 5, 1, 10);

-- Verify inserted rows
SELECT * FROM Drugs
WHERE DrugId BETWEEN 1001 AND 1005;
