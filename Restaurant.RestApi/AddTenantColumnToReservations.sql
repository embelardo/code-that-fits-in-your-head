/* Copyright (c) Mark Seemann 2020. All rights reserved. */
ALTER TABLE [dbo].[Reservations]
ADD [RestaurantId] INT NULL
GO

UPDATE [dbo].[Reservations]
SET [RestaurantId] = 1
WHERE [RestaurantId] IS NULL
GO

ALTER TABLE [dbo].[Reservations]
ALTER COLUMN [RestaurantId] INT NOT NULL
GO