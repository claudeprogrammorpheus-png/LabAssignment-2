using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace B2B.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agent",
                columns: table => new
                {
                    AgentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agent", x => x.AgentID);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemID);
                    table.CheckConstraint("CK_Item_Price", "[Price] >= 0");
                    table.CheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgentID = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderID);
                    table.CheckConstraint("CK_Order_Status", "[OrderStatus] IN ('Pending','Processing','Shipped','Delivered','Cancelled')");
                    table.ForeignKey(
                        name: "FK_Order_Agent_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Lock = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false),
                    AgentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('Admin','Agent')");
                    table.ForeignKey(
                        name: "FK_Users_Agent_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.ID);
                    table.CheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_OrderDetail_Item_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Order",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Agent",
                columns: new[] { "AgentID", "Address", "AgentName", "Phone" },
                values: new object[,]
                {
                    { 1, "12 Nguyen Trai, District 1, HCMC", "Minh Phat Trading Co.", "0283 822 1101" },
                    { 2, "45 Le Van Sy, Tan Binh, HCMC", "Hoang Long Grocery", "0283 844 2202" },
                    { 3, "88 Vo Van Ngan, Thu Duc, HCMC", "Thanh Dat Mini Mart", "0283 896 3303" },
                    { 4, "210 Hung Vuong, Bien Hoa, Dong Nai", "An Khang Distributor", "0251 382 4404" },
                    { 5, "5 Tran Phu, Thu Dau Mot, Binh Duong", "Phuoc Loc Store", "0274 383 5505" },
                    { 6, "302 Cach Mang Thang 8, District 3, HCMC", "Sai Gon Fresh Mart", "0283 930 6606" },
                    { 7, "17 Hau Giang, District 6, HCMC", "Bao Ngoc Wholesale", "0283 960 7707" },
                    { 8, "99 Nguyen Van Linh, District 7, HCMC", "Tan Tien Supermarket", "0283 775 8808" },
                    { 9, "64 Ly Thuong Kiet, Vung Tau", "Vinh Hung Agency", "0254 385 9909" },
                    { 10, "23 Pham Van Dong, Go Vap, HCMC", "Hong Phuc Convenience", "0283 589 1010" },
                    { 11, "150 30/4 Street, Ninh Kieu, Can Tho", "Dai Phat Food Supply", "0292 382 1111" },
                    { 12, "7 Quang Trung, My Tho, Tien Giang", "Kim Ngan Grocery", "0273 387 1212" },
                    { 13, "41 Tran Hung Dao, Long Xuyen, An Giang", "Phu Quy Retail", "0296 384 1313" },
                    { 14, "88 Nguyen Hue, Tay Ninh", "Thinh Vuong Mart", "0276 382 1414" },
                    { 15, "36 Le Loi, Ben Tre", "Ngoc Anh Shop", "0275 382 1515" },
                    { 16, "120 Dien Bien Phu, Binh Thanh, HCMC", "Quang Minh Trading", "0283 512 1616" },
                    { 17, "9 Truong Chinh, Tan Phu, HCMC", "Hai Dang Beverage", "0283 815 1717" },
                    { 18, "75 Ba Thang Hai, District 10, HCMC", "Song Thu Distributor", "0283 865 1818" },
                    { 19, "18 Nguyen Thi Minh Khai, Da Lat", "Hung Thinh Store", "0263 382 1919" },
                    { 20, "52 Hoang Dieu, Phan Thiet, Binh Thuan", "Lan Chi Mini Market", "0252 382 2020" }
                });

            migrationBuilder.InsertData(
                table: "Item",
                columns: new[] { "ItemID", "ItemName", "Price", "Size", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Aquafina Mineral Water", 105000m, "500ml x 24 bottles", 420 },
                    { 2, "Coca-Cola Original", 215000m, "330ml x 24 cans", 350 },
                    { 3, "Pepsi Cola", 205000m, "330ml x 24 cans", 310 },
                    { 4, "Sting Strawberry Energy", 230000m, "330ml x 24 bottles", 260 },
                    { 5, "C2 Green Tea Lemon", 175000m, "455ml x 24 bottles", 180 },
                    { 6, "TH True Milk Less Sugar", 365000m, "180ml x 48 boxes", 140 },
                    { 7, "Vinamilk Fresh Milk", 385000m, "1L x 12 boxes", 95 },
                    { 8, "Hao Hao Shrimp Noodles", 118000m, "75g x 30 packs", 500 },
                    { 9, "Omachi Beef Noodles", 195000m, "80g x 30 packs", 220 },
                    { 10, "Neptune Cooking Oil", 540000m, "1L x 12 bottles", 75 },
                    { 11, "Chin-su Fish Sauce", 410000m, "500ml x 12 bottles", 60 },
                    { 12, "Bien Hoa Refined Sugar", 520000m, "1kg x 20 bags", 45 },
                    { 13, "ST25 Jasmine Rice", 875000m, "25kg bag", 38 },
                    { 14, "Trung Nguyen G7 Coffee 3in1", 1150000m, "16g x 50 sachets x 12", 30 },
                    { 15, "Nescafe Cafe Viet", 960000m, "17g x 35 sachets x 12", 8 },
                    { 16, "OMO Laundry Powder", 640000m, "3kg x 4 bags", 55 },
                    { 17, "Sunlight Dishwashing Liquid", 395000m, "750g x 12 bottles", 70 },
                    { 18, "P/S Toothpaste", 980000m, "180g x 36 tubes", 12 },
                    { 19, "Lifebuoy Hand Wash", 870000m, "450g x 12 bottles", 25 },
                    { 20, "Oreo Chocolate Cookies", 420000m, "133g x 24 packs", 6 },
                    { 21, "Choco-Pie Orion", 880000m, "12 pcs x 16 boxes", 40 },
                    { 22, "Kinh Do Cosy Biscuits", 560000m, "240g x 20 packs", 3 },
                    { 23, "Tiger Beer", 395000m, "330ml x 24 cans", 160 },
                    { 24, "Heineken Beer", 465000m, "330ml x 24 cans", 120 },
                    { 25, "Pocari Sweat", 290000m, "500ml x 24 bottles", 0 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AgentID", "Email", "FailedLoginCount", "Lock", "Password", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, null, "admin@b2bwholesale.vn", 0, false, "100000.obLD1OX2BxgpOktcbX6PkA==.wEpCK/J75qaDTrjtoVmTGxMFFfArgrfLvM+TW4PHE70=", "Admin", "admin" },
                    { 2, null, "warehouse@b2bwholesale.vn", 0, false, "100000.obLD1OX2BxgpOktcbX6PkA==.wEpCK/J75qaDTrjtoVmTGxMFFfArgrfLvM+TW4PHE70=", "Admin", "warehouse" }
                });

            migrationBuilder.InsertData(
                table: "Order",
                columns: new[] { "OrderID", "AgentID", "OrderDate", "OrderStatus" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 2, 10, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 2, 2, new DateTime(2026, 9, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 3, 3, new DateTime(2026, 9, 8, 12, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 4, 1, new DateTime(2026, 9, 12, 13, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 5, 4, new DateTime(2026, 9, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 6, 5, new DateTime(2026, 9, 18, 15, 0, 0, 0, DateTimeKind.Unspecified), "Cancelled" },
                    { 7, 6, new DateTime(2026, 9, 21, 16, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 8, 7, new DateTime(2026, 9, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 9, 8, new DateTime(2026, 9, 28, 10, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 10, 2, new DateTime(2026, 10, 1, 11, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 11, 9, new DateTime(2026, 10, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 12, 10, new DateTime(2026, 10, 7, 13, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 13, 1, new DateTime(2026, 10, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 14, 11, new DateTime(2026, 10, 13, 15, 0, 0, 0, DateTimeKind.Unspecified), "Processing" },
                    { 15, 12, new DateTime(2026, 10, 16, 16, 0, 0, 0, DateTimeKind.Unspecified), "Processing" },
                    { 16, 3, new DateTime(2026, 10, 19, 9, 0, 0, 0, DateTimeKind.Unspecified), "Processing" },
                    { 17, 13, new DateTime(2026, 10, 22, 10, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 18, 14, new DateTime(2026, 10, 25, 11, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 19, 1, new DateTime(2026, 10, 27, 12, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 20, 15, new DateTime(2026, 10, 29, 13, 0, 0, 0, DateTimeKind.Unspecified), "Pending" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AgentID", "Email", "FailedLoginCount", "Lock", "Password", "Role", "UserName" },
                values: new object[,]
                {
                    { 3, 1, "agent01@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent01" },
                    { 4, 2, "agent02@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent02" },
                    { 5, 3, "agent03@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent03" },
                    { 6, 4, "agent04@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent04" },
                    { 7, 5, "agent05@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent05" },
                    { 8, 6, "agent06@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent06" },
                    { 9, 7, "agent07@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent07" },
                    { 10, 8, "agent08@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent08" },
                    { 11, 9, "agent09@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent09" },
                    { 12, 10, "agent10@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent10" },
                    { 13, 11, "agent11@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent11" },
                    { 14, 12, "agent12@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent12" },
                    { 15, 13, "agent13@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent13" },
                    { 16, 14, "agent14@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent14" },
                    { 17, 15, "agent15@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent15" },
                    { 18, 16, "agent16@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent16" },
                    { 19, 17, "agent17@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent17" },
                    { 20, 18, "agent18@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent18" },
                    { 21, 19, "agent19@b2bwholesale.vn", 0, false, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent19" },
                    { 22, 20, "agent20@b2bwholesale.vn", 0, true, "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=", "Agent", "agent20" }
                });

            migrationBuilder.InsertData(
                table: "OrderDetail",
                columns: new[] { "ID", "ItemID", "OrderID", "Quantity", "UnitAmount" },
                values: new object[,]
                {
                    { 1, 1, 1, 20, 105000m },
                    { 2, 2, 1, 10, 215000m },
                    { 3, 8, 2, 40, 118000m },
                    { 4, 11, 2, 5, 410000m },
                    { 5, 23, 3, 15, 395000m },
                    { 6, 24, 3, 10, 465000m },
                    { 7, 1, 4, 30, 105000m },
                    { 8, 6, 4, 8, 365000m },
                    { 9, 13, 5, 6, 875000m },
                    { 10, 12, 5, 4, 520000m },
                    { 11, 14, 6, 2, 1150000m },
                    { 12, 2, 7, 25, 215000m },
                    { 13, 4, 7, 12, 230000m },
                    { 14, 16, 8, 6, 640000m },
                    { 15, 17, 8, 10, 395000m },
                    { 16, 9, 9, 20, 195000m },
                    { 17, 8, 9, 30, 118000m },
                    { 18, 1, 10, 15, 105000m },
                    { 19, 3, 10, 12, 205000m },
                    { 20, 21, 11, 5, 880000m },
                    { 21, 7, 12, 10, 385000m },
                    { 22, 2, 13, 20, 215000m },
                    { 23, 23, 13, 18, 395000m },
                    { 24, 10, 14, 6, 540000m },
                    { 25, 5, 15, 24, 175000m },
                    { 26, 19, 16, 4, 870000m },
                    { 27, 18, 17, 3, 980000m },
                    { 28, 24, 18, 12, 465000m },
                    { 29, 1, 19, 25, 105000m },
                    { 30, 9, 20, 14, 195000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Phone",
                table: "Agent",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemName",
                table: "Item",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Order_AgentID",
                table: "Order",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderDate",
                table: "Order",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ItemID",
                table: "OrderDetail",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderID",
                table: "OrderDetail",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgentID",
                table: "Users",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Agent");
        }
    }
}
