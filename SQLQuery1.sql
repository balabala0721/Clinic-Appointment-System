INSERT INTO Drugs
(DrugId, DrugName, DrugCategory, MedicalUses,
 UnitPrice, StockQuantity, StockStatus, MinStockLevel,
 CreatedDateTime)
VALUES
(1001, 'Paracetamol', 0, 'Relief of pain',
 5.50, 100, 0, 20, GETDATE()),

(1002, 'Amoxicillin', 1, 'Treatment of bacterial infections',
 12.00, 10, 1, 20, GETDATE()),

(1003, 'Cetirizine', 2, 'Relief of allergy symptoms',
 8.50, 50, 0, 10, GETDATE()),

(1004, 'Ibuprofen', 3, 'Relief of fever',
 7.00, 30, 0, 15, GETDATE()),

(1005, 'Omeprazole', 4, 'Treatment of acid reflux',
 15.00, 5, 1, 10, GETDATE()),

(1006, 'Dextromethorphan', 5, 'Relief of dry cough',
 9.50, 80, 0, 10, GETDATE()),

(1007, 'Guaifenesin', 6, 'Helps loosen mucus',
 11.00, 40, 0, 10, GETDATE()),

(1008, 'Loperamide', 7, 'Relief of diarrhoea symptoms',
 6.50, 0, 2, 10, GETDATE()),

(1009, 'Lactulose', 8, 'Relief of constipation',
 14.00, 25, 0, 10, GETDATE()),

(1010, 'Clotrimazole', 9, 'Treatment of fungal infections',
 10.00, 15, 1, 20, GETDATE()),

(1011, 'Povidone-Iodine', 10, 'Antiseptic skin cleansing',
 8.00, 60, 0, 10, GETDATE()),

(1012, 'Metformin', 11, 'Management of type 2 diabetes',
 13.00, 100, 0, 20, GETDATE()),

(1013, 'Amlodipine', 12, 'Management of high blood pressure',
 16.00, 8, 1, 20, GETDATE()),

(1014, 'Salbutamol', 13, 'Relief of bronchospasm',
 18.00, 0, 2, 10, GETDATE()),

(1015, 'Hydrocortisone', 14, 'Relief of skin inflammation',
 12.00, 35, 0, 10, GETDATE());

 SELECT MAX(DrugId) AS LastDrugId
FROM Drugs;