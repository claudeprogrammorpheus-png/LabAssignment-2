IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Agent] (
    [AgentID] int NOT NULL IDENTITY,
    [AgentName] nvarchar(100) NOT NULL,
    [Address] nvarchar(200) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Agent] PRIMARY KEY ([AgentID])
);

CREATE TABLE [Item] (
    [ItemID] int NOT NULL IDENTITY,
    [ItemName] nvarchar(100) NOT NULL,
    [Size] nvarchar(30) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    CONSTRAINT [PK_Item] PRIMARY KEY ([ItemID]),
    CONSTRAINT [CK_Item_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_Item_Stock] CHECK ([StockQuantity] >= 0)
);

CREATE TABLE [Order] (
    [OrderID] int NOT NULL IDENTITY,
    [OrderDate] datetime2 NOT NULL,
    [AgentID] int NOT NULL,
    [OrderStatus] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY ([OrderID]),
    CONSTRAINT [CK_Order_Status] CHECK ([OrderStatus] IN ('Pending','Processing','Shipped','Delivered','Cancelled')),
    CONSTRAINT [FK_Order_Agent_AgentID] FOREIGN KEY ([AgentID]) REFERENCES [Agent] ([AgentID]) ON DELETE NO ACTION
);

CREATE TABLE [Users] (
    [UserID] int NOT NULL IDENTITY,
    [UserName] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Password] nvarchar(200) NOT NULL,
    [Lock] bit NOT NULL,
    [Role] nvarchar(10) NOT NULL,
    [FailedLoginCount] int NOT NULL,
    [AgentID] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserID]),
    CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Admin','Agent')),
    CONSTRAINT [FK_Users_Agent_AgentID] FOREIGN KEY ([AgentID]) REFERENCES [Agent] ([AgentID]) ON DELETE CASCADE
);

CREATE TABLE [OrderDetail] (
    [ID] int NOT NULL IDENTITY,
    [OrderID] int NOT NULL,
    [ItemID] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrderDetail] PRIMARY KEY ([ID]),
    CONSTRAINT [CK_OrderDetail_Qty] CHECK ([Quantity] > 0),
    CONSTRAINT [FK_OrderDetail_Item_ItemID] FOREIGN KEY ([ItemID]) REFERENCES [Item] ([ItemID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrderDetail_Order_OrderID] FOREIGN KEY ([OrderID]) REFERENCES [Order] ([OrderID]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AgentID', N'Address', N'AgentName', N'Phone') AND [object_id] = OBJECT_ID(N'[Agent]'))
    SET IDENTITY_INSERT [Agent] ON;
INSERT INTO [Agent] ([AgentID], [Address], [AgentName], [Phone])
VALUES (1, N'12 Nguyen Trai, District 1, HCMC', N'Minh Phat Trading Co.', N'0283 822 1101'),
(2, N'45 Le Van Sy, Tan Binh, HCMC', N'Hoang Long Grocery', N'0283 844 2202'),
(3, N'88 Vo Van Ngan, Thu Duc, HCMC', N'Thanh Dat Mini Mart', N'0283 896 3303'),
(4, N'210 Hung Vuong, Bien Hoa, Dong Nai', N'An Khang Distributor', N'0251 382 4404'),
(5, N'5 Tran Phu, Thu Dau Mot, Binh Duong', N'Phuoc Loc Store', N'0274 383 5505'),
(6, N'302 Cach Mang Thang 8, District 3, HCMC', N'Sai Gon Fresh Mart', N'0283 930 6606'),
(7, N'17 Hau Giang, District 6, HCMC', N'Bao Ngoc Wholesale', N'0283 960 7707'),
(8, N'99 Nguyen Van Linh, District 7, HCMC', N'Tan Tien Supermarket', N'0283 775 8808'),
(9, N'64 Ly Thuong Kiet, Vung Tau', N'Vinh Hung Agency', N'0254 385 9909'),
(10, N'23 Pham Van Dong, Go Vap, HCMC', N'Hong Phuc Convenience', N'0283 589 1010'),
(11, N'150 30/4 Street, Ninh Kieu, Can Tho', N'Dai Phat Food Supply', N'0292 382 1111'),
(12, N'7 Quang Trung, My Tho, Tien Giang', N'Kim Ngan Grocery', N'0273 387 1212'),
(13, N'41 Tran Hung Dao, Long Xuyen, An Giang', N'Phu Quy Retail', N'0296 384 1313'),
(14, N'88 Nguyen Hue, Tay Ninh', N'Thinh Vuong Mart', N'0276 382 1414'),
(15, N'36 Le Loi, Ben Tre', N'Ngoc Anh Shop', N'0275 382 1515'),
(16, N'120 Dien Bien Phu, Binh Thanh, HCMC', N'Quang Minh Trading', N'0283 512 1616'),
(17, N'9 Truong Chinh, Tan Phu, HCMC', N'Hai Dang Beverage', N'0283 815 1717'),
(18, N'75 Ba Thang Hai, District 10, HCMC', N'Song Thu Distributor', N'0283 865 1818'),
(19, N'18 Nguyen Thi Minh Khai, Da Lat', N'Hung Thinh Store', N'0263 382 1919'),
(20, N'52 Hoang Dieu, Phan Thiet, Binh Thuan', N'Lan Chi Mini Market', N'0252 382 2020');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AgentID', N'Address', N'AgentName', N'Phone') AND [object_id] = OBJECT_ID(N'[Agent]'))
    SET IDENTITY_INSERT [Agent] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ItemID', N'ItemName', N'Price', N'Size', N'StockQuantity') AND [object_id] = OBJECT_ID(N'[Item]'))
    SET IDENTITY_INSERT [Item] ON;
INSERT INTO [Item] ([ItemID], [ItemName], [Price], [Size], [StockQuantity])
VALUES (1, N'Aquafina Mineral Water', 105000.0, N'500ml x 24 bottles', 420),
(2, N'Coca-Cola Original', 215000.0, N'330ml x 24 cans', 350),
(3, N'Pepsi Cola', 205000.0, N'330ml x 24 cans', 310),
(4, N'Sting Strawberry Energy', 230000.0, N'330ml x 24 bottles', 260),
(5, N'C2 Green Tea Lemon', 175000.0, N'455ml x 24 bottles', 180),
(6, N'TH True Milk Less Sugar', 365000.0, N'180ml x 48 boxes', 140),
(7, N'Vinamilk Fresh Milk', 385000.0, N'1L x 12 boxes', 95),
(8, N'Hao Hao Shrimp Noodles', 118000.0, N'75g x 30 packs', 500),
(9, N'Omachi Beef Noodles', 195000.0, N'80g x 30 packs', 220),
(10, N'Neptune Cooking Oil', 540000.0, N'1L x 12 bottles', 75),
(11, N'Chin-su Fish Sauce', 410000.0, N'500ml x 12 bottles', 60),
(12, N'Bien Hoa Refined Sugar', 520000.0, N'1kg x 20 bags', 45),
(13, N'ST25 Jasmine Rice', 875000.0, N'25kg bag', 38),
(14, N'Trung Nguyen G7 Coffee 3in1', 1150000.0, N'16g x 50 sachets x 12', 30),
(15, N'Nescafe Cafe Viet', 960000.0, N'17g x 35 sachets x 12', 8),
(16, N'OMO Laundry Powder', 640000.0, N'3kg x 4 bags', 55),
(17, N'Sunlight Dishwashing Liquid', 395000.0, N'750g x 12 bottles', 70),
(18, N'P/S Toothpaste', 980000.0, N'180g x 36 tubes', 12),
(19, N'Lifebuoy Hand Wash', 870000.0, N'450g x 12 bottles', 25),
(20, N'Oreo Chocolate Cookies', 420000.0, N'133g x 24 packs', 6),
(21, N'Choco-Pie Orion', 880000.0, N'12 pcs x 16 boxes', 40),
(22, N'Kinh Do Cosy Biscuits', 560000.0, N'240g x 20 packs', 3),
(23, N'Tiger Beer', 395000.0, N'330ml x 24 cans', 160),
(24, N'Heineken Beer', 465000.0, N'330ml x 24 cans', 120),
(25, N'Pocari Sweat', 290000.0, N'500ml x 24 bottles', 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ItemID', N'ItemName', N'Price', N'Size', N'StockQuantity') AND [object_id] = OBJECT_ID(N'[Item]'))
    SET IDENTITY_INSERT [Item] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AgentID', N'Email', N'FailedLoginCount', N'Lock', N'Password', N'Role', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserID], [AgentID], [Email], [FailedLoginCount], [Lock], [Password], [Role], [UserName])
VALUES (1, NULL, N'admin@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.obLD1OX2BxgpOktcbX6PkA==.wEpCK/J75qaDTrjtoVmTGxMFFfArgrfLvM+TW4PHE70=', N'Admin', N'admin'),
(2, NULL, N'warehouse@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.obLD1OX2BxgpOktcbX6PkA==.wEpCK/J75qaDTrjtoVmTGxMFFfArgrfLvM+TW4PHE70=', N'Admin', N'warehouse');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AgentID', N'Email', N'FailedLoginCount', N'Lock', N'Password', N'Role', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderID', N'AgentID', N'OrderDate', N'OrderStatus') AND [object_id] = OBJECT_ID(N'[Order]'))
    SET IDENTITY_INSERT [Order] ON;
INSERT INTO [Order] ([OrderID], [AgentID], [OrderDate], [OrderStatus])
VALUES (1, 1, '2026-09-02T10:00:00.0000000', N'Delivered'),
(2, 2, '2026-09-04T11:00:00.0000000', N'Delivered'),
(3, 3, '2026-09-08T12:00:00.0000000', N'Delivered'),
(4, 1, '2026-09-12T13:00:00.0000000', N'Delivered'),
(5, 4, '2026-09-15T14:00:00.0000000', N'Delivered'),
(6, 5, '2026-09-18T15:00:00.0000000', N'Cancelled'),
(7, 6, '2026-09-21T16:00:00.0000000', N'Delivered'),
(8, 7, '2026-09-25T09:00:00.0000000', N'Delivered'),
(9, 8, '2026-09-28T10:00:00.0000000', N'Delivered'),
(10, 2, '2026-10-01T11:00:00.0000000', N'Delivered'),
(11, 9, '2026-10-04T12:00:00.0000000', N'Shipped'),
(12, 10, '2026-10-07T13:00:00.0000000', N'Shipped'),
(13, 1, '2026-10-10T14:00:00.0000000', N'Shipped'),
(14, 11, '2026-10-13T15:00:00.0000000', N'Processing'),
(15, 12, '2026-10-16T16:00:00.0000000', N'Processing'),
(16, 3, '2026-10-19T09:00:00.0000000', N'Processing'),
(17, 13, '2026-10-22T10:00:00.0000000', N'Pending'),
(18, 14, '2026-10-25T11:00:00.0000000', N'Pending'),
(19, 1, '2026-10-27T12:00:00.0000000', N'Pending'),
(20, 15, '2026-10-29T13:00:00.0000000', N'Pending');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderID', N'AgentID', N'OrderDate', N'OrderStatus') AND [object_id] = OBJECT_ID(N'[Order]'))
    SET IDENTITY_INSERT [Order] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AgentID', N'Email', N'FailedLoginCount', N'Lock', N'Password', N'Role', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserID], [AgentID], [Email], [FailedLoginCount], [Lock], [Password], [Role], [UserName])
VALUES (3, 1, N'agent01@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent01'),
(4, 2, N'agent02@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent02'),
(5, 3, N'agent03@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent03'),
(6, 4, N'agent04@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent04'),
(7, 5, N'agent05@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent05'),
(8, 6, N'agent06@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent06'),
(9, 7, N'agent07@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent07'),
(10, 8, N'agent08@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent08'),
(11, 9, N'agent09@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent09'),
(12, 10, N'agent10@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent10'),
(13, 11, N'agent11@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent11'),
(14, 12, N'agent12@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent12'),
(15, 13, N'agent13@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent13'),
(16, 14, N'agent14@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent14'),
(17, 15, N'agent15@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent15'),
(18, 16, N'agent16@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent16'),
(19, 17, N'agent17@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent17'),
(20, 18, N'agent18@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent18'),
(21, 19, N'agent19@b2bwholesale.vn', 0, CAST(0 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent19'),
(22, 20, N'agent20@b2bwholesale.vn', 0, CAST(1 AS bit), N'100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=', N'Agent', N'agent20');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AgentID', N'Email', N'FailedLoginCount', N'Lock', N'Password', N'Role', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'ItemID', N'OrderID', N'Quantity', N'UnitAmount') AND [object_id] = OBJECT_ID(N'[OrderDetail]'))
    SET IDENTITY_INSERT [OrderDetail] ON;
INSERT INTO [OrderDetail] ([ID], [ItemID], [OrderID], [Quantity], [UnitAmount])
VALUES (1, 1, 1, 20, 105000.0),
(2, 2, 1, 10, 215000.0),
(3, 8, 2, 40, 118000.0),
(4, 11, 2, 5, 410000.0),
(5, 23, 3, 15, 395000.0),
(6, 24, 3, 10, 465000.0),
(7, 1, 4, 30, 105000.0),
(8, 6, 4, 8, 365000.0),
(9, 13, 5, 6, 875000.0),
(10, 12, 5, 4, 520000.0),
(11, 14, 6, 2, 1150000.0),
(12, 2, 7, 25, 215000.0),
(13, 4, 7, 12, 230000.0),
(14, 16, 8, 6, 640000.0),
(15, 17, 8, 10, 395000.0),
(16, 9, 9, 20, 195000.0),
(17, 8, 9, 30, 118000.0),
(18, 1, 10, 15, 105000.0),
(19, 3, 10, 12, 205000.0),
(20, 21, 11, 5, 880000.0),
(21, 7, 12, 10, 385000.0),
(22, 2, 13, 20, 215000.0),
(23, 23, 13, 18, 395000.0),
(24, 10, 14, 6, 540000.0),
(25, 5, 15, 24, 175000.0),
(26, 19, 16, 4, 870000.0),
(27, 18, 17, 3, 980000.0),
(28, 24, 18, 12, 465000.0),
(29, 1, 19, 25, 105000.0),
(30, 9, 20, 14, 195000.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ID', N'ItemID', N'OrderID', N'Quantity', N'UnitAmount') AND [object_id] = OBJECT_ID(N'[OrderDetail]'))
    SET IDENTITY_INSERT [OrderDetail] OFF;

CREATE UNIQUE INDEX [IX_Agent_Phone] ON [Agent] ([Phone]);

CREATE INDEX [IX_Item_ItemName] ON [Item] ([ItemName]);

CREATE INDEX [IX_Order_AgentID] ON [Order] ([AgentID]);

CREATE INDEX [IX_Order_OrderDate] ON [Order] ([OrderDate]);

CREATE INDEX [IX_OrderDetail_ItemID] ON [OrderDetail] ([ItemID]);

CREATE INDEX [IX_OrderDetail_OrderID] ON [OrderDetail] ([OrderID]);

CREATE INDEX [IX_Users_AgentID] ON [Users] ([AgentID]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261030133817_InitialCreate', N'9.0.10');

COMMIT;
GO

