using B2B.Core;
using Microsoft.EntityFrameworkCore;

namespace B2B.Data;

/// <summary>Realistic sample data (25 items, 20 agents, 22 users, 20 orders, 30 order lines).</summary>
public static class SeedData
{
    // PBKDF2 hashes of "Admin@123" and "Agent@123" (fixed salts so migrations stay stable).
    public const string AdminHash = "100000.obLD1OX2BxgpOktcbX6PkA==.wEpCK/J75qaDTrjtoVmTGxMFFfArgrfLvM+TW4PHE70=";
    public const string AgentHash = "100000.Dx4tPEtaaXiHlqW0w9Lh8A==.veeHw7jGT8ZlDdKFcZjzeteFl4HjQ2FKydRsl7P14wk=";

    public static readonly Item[] Items =
    [
        new() { ItemID = 1,  ItemName = "Aquafina Mineral Water",      Size = "500ml x 24 bottles", Price = 105000,  StockQuantity = 420 },
        new() { ItemID = 2,  ItemName = "Coca-Cola Original",          Size = "330ml x 24 cans",    Price = 215000,  StockQuantity = 350 },
        new() { ItemID = 3,  ItemName = "Pepsi Cola",                  Size = "330ml x 24 cans",    Price = 205000,  StockQuantity = 310 },
        new() { ItemID = 4,  ItemName = "Sting Strawberry Energy",     Size = "330ml x 24 bottles", Price = 230000,  StockQuantity = 260 },
        new() { ItemID = 5,  ItemName = "C2 Green Tea Lemon",          Size = "455ml x 24 bottles", Price = 175000,  StockQuantity = 180 },
        new() { ItemID = 6,  ItemName = "TH True Milk Less Sugar",     Size = "180ml x 48 boxes",   Price = 365000,  StockQuantity = 140 },
        new() { ItemID = 7,  ItemName = "Vinamilk Fresh Milk",         Size = "1L x 12 boxes",      Price = 385000,  StockQuantity = 95  },
        new() { ItemID = 8,  ItemName = "Hao Hao Shrimp Noodles",      Size = "75g x 30 packs",     Price = 118000,  StockQuantity = 500 },
        new() { ItemID = 9,  ItemName = "Omachi Beef Noodles",         Size = "80g x 30 packs",     Price = 195000,  StockQuantity = 220 },
        new() { ItemID = 10, ItemName = "Neptune Cooking Oil",         Size = "1L x 12 bottles",    Price = 540000,  StockQuantity = 75  },
        new() { ItemID = 11, ItemName = "Chin-su Fish Sauce",          Size = "500ml x 12 bottles", Price = 410000,  StockQuantity = 60  },
        new() { ItemID = 12, ItemName = "Bien Hoa Refined Sugar",      Size = "1kg x 20 bags",      Price = 520000,  StockQuantity = 45  },
        new() { ItemID = 13, ItemName = "ST25 Jasmine Rice",           Size = "25kg bag",           Price = 875000,  StockQuantity = 38  },
        new() { ItemID = 14, ItemName = "Trung Nguyen G7 Coffee 3in1", Size = "16g x 50 sachets x 12", Price = 1150000, StockQuantity = 30 },
        new() { ItemID = 15, ItemName = "Nescafe Cafe Viet",           Size = "17g x 35 sachets x 12", Price = 960000,  StockQuantity = 8  },
        new() { ItemID = 16, ItemName = "OMO Laundry Powder",          Size = "3kg x 4 bags",       Price = 640000,  StockQuantity = 55  },
        new() { ItemID = 17, ItemName = "Sunlight Dishwashing Liquid", Size = "750g x 12 bottles",  Price = 395000,  StockQuantity = 70  },
        new() { ItemID = 18, ItemName = "P/S Toothpaste",              Size = "180g x 36 tubes",    Price = 980000,  StockQuantity = 12  },
        new() { ItemID = 19, ItemName = "Lifebuoy Hand Wash",          Size = "450g x 12 bottles",  Price = 870000,  StockQuantity = 25  },
        new() { ItemID = 20, ItemName = "Oreo Chocolate Cookies",      Size = "133g x 24 packs",    Price = 420000,  StockQuantity = 6   },
        new() { ItemID = 21, ItemName = "Choco-Pie Orion",             Size = "12 pcs x 16 boxes",  Price = 880000,  StockQuantity = 40  },
        new() { ItemID = 22, ItemName = "Kinh Do Cosy Biscuits",       Size = "240g x 20 packs",    Price = 560000,  StockQuantity = 3   },
        new() { ItemID = 23, ItemName = "Tiger Beer",                  Size = "330ml x 24 cans",    Price = 395000,  StockQuantity = 160 },
        new() { ItemID = 24, ItemName = "Heineken Beer",               Size = "330ml x 24 cans",    Price = 465000,  StockQuantity = 120 },
        new() { ItemID = 25, ItemName = "Pocari Sweat",                Size = "500ml x 24 bottles", Price = 290000,  StockQuantity = 0   },
    ];

    public static readonly Agent[] Agents =
    [
        new() { AgentID = 1,  AgentName = "Minh Phat Trading Co.",      Address = "12 Nguyen Trai, District 1, HCMC",        Phone = "0283 822 1101" },
        new() { AgentID = 2,  AgentName = "Hoang Long Grocery",         Address = "45 Le Van Sy, Tan Binh, HCMC",            Phone = "0283 844 2202" },
        new() { AgentID = 3,  AgentName = "Thanh Dat Mini Mart",        Address = "88 Vo Van Ngan, Thu Duc, HCMC",           Phone = "0283 896 3303" },
        new() { AgentID = 4,  AgentName = "An Khang Distributor",       Address = "210 Hung Vuong, Bien Hoa, Dong Nai",      Phone = "0251 382 4404" },
        new() { AgentID = 5,  AgentName = "Phuoc Loc Store",            Address = "5 Tran Phu, Thu Dau Mot, Binh Duong",     Phone = "0274 383 5505" },
        new() { AgentID = 6,  AgentName = "Sai Gon Fresh Mart",         Address = "302 Cach Mang Thang 8, District 3, HCMC", Phone = "0283 930 6606" },
        new() { AgentID = 7,  AgentName = "Bao Ngoc Wholesale",         Address = "17 Hau Giang, District 6, HCMC",          Phone = "0283 960 7707" },
        new() { AgentID = 8,  AgentName = "Tan Tien Supermarket",       Address = "99 Nguyen Van Linh, District 7, HCMC",    Phone = "0283 775 8808" },
        new() { AgentID = 9,  AgentName = "Vinh Hung Agency",           Address = "64 Ly Thuong Kiet, Vung Tau",             Phone = "0254 385 9909" },
        new() { AgentID = 10, AgentName = "Hong Phuc Convenience",      Address = "23 Pham Van Dong, Go Vap, HCMC",          Phone = "0283 589 1010" },
        new() { AgentID = 11, AgentName = "Dai Phat Food Supply",       Address = "150 30/4 Street, Ninh Kieu, Can Tho",     Phone = "0292 382 1111" },
        new() { AgentID = 12, AgentName = "Kim Ngan Grocery",           Address = "7 Quang Trung, My Tho, Tien Giang",       Phone = "0273 387 1212" },
        new() { AgentID = 13, AgentName = "Phu Quy Retail",             Address = "41 Tran Hung Dao, Long Xuyen, An Giang",  Phone = "0296 384 1313" },
        new() { AgentID = 14, AgentName = "Thinh Vuong Mart",           Address = "88 Nguyen Hue, Tay Ninh",                 Phone = "0276 382 1414" },
        new() { AgentID = 15, AgentName = "Ngoc Anh Shop",              Address = "36 Le Loi, Ben Tre",                      Phone = "0275 382 1515" },
        new() { AgentID = 16, AgentName = "Quang Minh Trading",         Address = "120 Dien Bien Phu, Binh Thanh, HCMC",     Phone = "0283 512 1616" },
        new() { AgentID = 17, AgentName = "Hai Dang Beverage",          Address = "9 Truong Chinh, Tan Phu, HCMC",           Phone = "0283 815 1717" },
        new() { AgentID = 18, AgentName = "Song Thu Distributor",       Address = "75 Ba Thang Hai, District 10, HCMC",      Phone = "0283 865 1818" },
        new() { AgentID = 19, AgentName = "Hung Thinh Store",           Address = "18 Nguyen Thi Minh Khai, Da Lat",         Phone = "0263 382 1919" },
        new() { AgentID = 20, AgentName = "Lan Chi Mini Market",        Address = "52 Hoang Dieu, Phan Thiet, Binh Thuan",   Phone = "0252 382 2020" },
    ];

    public static User[] Users()
    {
        var list = new List<User>
        {
            new() { UserID = 1, UserName = "admin", Email = "admin@b2bwholesale.vn", Password = AdminHash, Role = Roles.Admin },
            new() { UserID = 2, UserName = "warehouse", Email = "warehouse@b2bwholesale.vn", Password = AdminHash, Role = Roles.Admin },
        };
        for (int i = 1; i <= 20; i++)
            list.Add(new User
            {
                UserID = i + 2, UserName = $"agent{i:00}", Email = $"agent{i:00}@b2bwholesale.vn",
                Password = AgentHash, Role = Roles.Agent, AgentID = i,
                Lock = i == 20, // one locked account to demonstrate lockout
            });
        return list.ToArray();
    }

    public static readonly Order[] Orders =
    [
        O(1, 1, "2026-09-02", OrderStatuses.Delivered), O(2, 2, "2026-09-04", OrderStatuses.Delivered),
        O(3, 3, "2026-09-08", OrderStatuses.Delivered), O(4, 1, "2026-09-12", OrderStatuses.Delivered),
        O(5, 4, "2026-09-15", OrderStatuses.Delivered), O(6, 5, "2026-09-18", OrderStatuses.Cancelled),
        O(7, 6, "2026-09-21", OrderStatuses.Delivered), O(8, 7, "2026-09-25", OrderStatuses.Delivered),
        O(9, 8, "2026-09-28", OrderStatuses.Delivered), O(10, 2, "2026-10-01", OrderStatuses.Delivered),
        O(11, 9, "2026-10-04", OrderStatuses.Shipped), O(12, 10, "2026-10-07", OrderStatuses.Shipped),
        O(13, 1, "2026-10-10", OrderStatuses.Shipped), O(14, 11, "2026-10-13", OrderStatuses.Processing),
        O(15, 12, "2026-10-16", OrderStatuses.Processing), O(16, 3, "2026-10-19", OrderStatuses.Processing),
        O(17, 13, "2026-10-22", OrderStatuses.Pending), O(18, 14, "2026-10-25", OrderStatuses.Pending),
        O(19, 1, "2026-10-27", OrderStatuses.Pending), O(20, 15, "2026-10-29", OrderStatuses.Pending),
    ];

    public static readonly OrderDetail[] OrderDetails =
    [
        D(1, 1, 1, 20), D(2, 1, 2, 10), D(3, 2, 8, 40), D(4, 2, 11, 5), D(5, 3, 23, 15),
        D(6, 3, 24, 10), D(7, 4, 1, 30), D(8, 4, 6, 8), D(9, 5, 13, 6), D(10, 5, 12, 4),
        D(11, 6, 14, 2), D(12, 7, 2, 25), D(13, 7, 4, 12), D(14, 8, 16, 6), D(15, 8, 17, 10),
        D(16, 9, 9, 20), D(17, 9, 8, 30), D(18, 10, 1, 15), D(19, 10, 3, 12), D(20, 11, 21, 5),
        D(21, 12, 7, 10), D(22, 13, 2, 20), D(23, 13, 23, 18), D(24, 14, 10, 6), D(25, 15, 5, 24),
        D(26, 16, 19, 4), D(27, 17, 18, 3), D(28, 18, 24, 12), D(29, 19, 1, 25), D(30, 20, 9, 14),
    ];

    private static Order O(int id, int agent, string date, string status) =>
        new() { OrderID = id, AgentID = agent, OrderDate = DateTime.Parse(date).AddHours(9 + id % 8), OrderStatus = status };

    private static OrderDetail D(int id, int order, int item, int qty) =>
        new() { ID = id, OrderID = order, ItemID = item, Quantity = qty, UnitAmount = Items.First(i => i.ItemID == item).Price };

    public static void Apply(ModelBuilder b)
    {
        // HasData needs plain objects without navigation values.
        b.Entity<Item>().HasData(Items.Select(i => new Item { ItemID = i.ItemID, ItemName = i.ItemName, Size = i.Size, Price = i.Price, StockQuantity = i.StockQuantity }));
        b.Entity<Agent>().HasData(Agents.Select(a => new Agent { AgentID = a.AgentID, AgentName = a.AgentName, Address = a.Address, Phone = a.Phone }));
        b.Entity<User>().HasData(Users());
        b.Entity<Order>().HasData(Orders);
        b.Entity<OrderDetail>().HasData(OrderDetails);
    }
}
