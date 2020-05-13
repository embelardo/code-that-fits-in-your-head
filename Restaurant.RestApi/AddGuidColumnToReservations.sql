/* Copyright (c) Mark Seemann 2020. All rights reserved. */
ALTER TABLE [dbo].[Reservations]
ADD [PublicId] UNIQUEIDENTIFIER NULL
GO

UPDATE [dbo].[Reservations]
SET [PublicId] = NEWID()
WHERE [PublicId] IS NULL
GO

ALTER TABLE [dbo].[Reservations]
ALTER COLUMN [PublicId] UNIQUEIDENTIFIER NOT NULL
GO

ALTER TABLE [dbo].[Reservations]
ADD CONSTRAINT AK_PublicId UNIQUE([PublicId])
GO