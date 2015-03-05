CREATE TABLE [dbo].[Mappings]
(
	[Id] int IDENTITY,
	[Domain] nvarchar(255),
	[IP] nvarchar(15),
	[Active] bit NOT NULL,
	[ShowInfoBox] bit NOT NULL,
	[XLeft] int,
	[XRight] int,
	[YTop] int,
	[YBottom] int,
)
