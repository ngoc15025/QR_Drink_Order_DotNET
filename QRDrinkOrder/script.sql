BEGIN TRANSACTION;
ALTER TABLE [OrderItems] ADD [SizeId] int NULL;

CREATE TABLE [Sizes] (
    [SizeId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [PriceOffset] decimal(18,2) NOT NULL,
    CONSTRAINT [PK__Size] PRIMARY KEY ([SizeId])
);

CREATE TABLE [Toppings] (
    [ToppingId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [PK__Topping] PRIMARY KEY ([ToppingId])
);

CREATE TABLE [OrderItemToppings] (
    [OrderItemId] int NOT NULL,
    [ToppingId] int NOT NULL,
    CONSTRAINT [PK__OrderItemTopping] PRIMARY KEY ([OrderItemId], [ToppingId]),
    CONSTRAINT [FK__OrderItemTopping__OrderItem] FOREIGN KEY ([OrderItemId]) REFERENCES [OrderItems] ([OrderItemID]) ON DELETE CASCADE,
    CONSTRAINT [FK__OrderItemTopping__Topping] FOREIGN KEY ([ToppingId]) REFERENCES [Toppings] ([ToppingId]) ON DELETE CASCADE
);

CREATE INDEX [IX_OrderItems_SizeId] ON [OrderItems] ([SizeId]);

CREATE INDEX [IX_OrderItemToppings_ToppingId] ON [OrderItemToppings] ([ToppingId]);

ALTER TABLE [OrderItems] ADD CONSTRAINT [FK__OrderItem__Size] FOREIGN KEY ([SizeId]) REFERENCES [Sizes] ([SizeId]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606145521_AddSizeAndTopping', N'9.0.0');

COMMIT;
GO

