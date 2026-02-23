-- Add PayOSQrCode column to Transactions table
USE RealEstateListingPlatform;
GO

-- Check if column doesn't exist before adding
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Transactions' 
    AND COLUMN_NAME = 'PayOSQrCode'
)
BEGIN
    PRINT '>>> Adding PayOSQrCode column to Transactions table...';
    
    ALTER TABLE Transactions
    ADD PayOSQrCode NVARCHAR(MAX) NULL;
    
    PRINT '>>> PayOSQrCode column added successfully!';
END
ELSE
BEGIN
    PRINT '>>> PayOSQrCode column already exists.';
END
GO
