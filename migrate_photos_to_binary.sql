-- =====================================================
-- Migration script: string paths -> varbinary(max)
-- Run this on existing RentalCarDB if needed
-- =====================================================

-- 1. Add new binary columns (if not exist)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Cars') AND name = 'PhotoData')
    ALTER TABLE Cars ADD PhotoData varbinary(max) null;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Users') AND name = 'IdentitySelfiePhotoData')
    ALTER TABLE Users ADD IdentitySelfiePhotoData varbinary(max) null;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Orders') AND name = 'FrontPhotoData')
    ALTER TABLE Orders ADD FrontPhotoData varbinary(max) null;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Orders') AND name = 'RearPhotoData')
    ALTER TABLE Orders ADD RearPhotoData varbinary(max) null;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Orders') AND name = 'SidePhotoData')
    ALTER TABLE Orders ADD SidePhotoData varbinary(max) null;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = object_id('Reviews') AND name = 'PhotoData')
    ALTER TABLE Reviews ADD PhotoData varbinary(max) null;

-- 2. DROP old string columns (only after confirming new columns work)
-- Uncomment these after successful migration:
-- ALTER TABLE Cars DROP COLUMN PhotoPath;
-- ALTER TABLE Users DROP COLUMN IdentitySelfiePhotoPath;
-- ALTER TABLE Orders DROP COLUMN FrontPhotoPath;
-- ALTER TABLE Orders DROP COLUMN RearPhotoPath;
-- ALTER TABLE Orders DROP COLUMN SidePhotoPath;
-- ALTER TABLE Reviews DROP COLUMN PhotoPath;
