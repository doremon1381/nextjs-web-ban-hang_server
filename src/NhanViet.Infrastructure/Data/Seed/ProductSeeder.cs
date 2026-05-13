using Microsoft.EntityFrameworkCore;
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;

namespace NhanViet.Infrastructure.Data.Seed;

public static class ProductSeeder
{
    public static async Task SeedAsync(NhanVietDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var products = GetInitialProducts();
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private static List<Product> GetInitialProducts()
    {
        var list = new List<Product>();

        list.Add(CreateProduct(
            "nhan-long-tuoi-loai-1", "Nhãn lồng tươi loại 1",
            "Cùi dày, hạt nhỏ, ngọt thanh", null,
            ProductCategory.Fresh, "kg", "/images/nhan-long-tuoi-loai-1.jpg", [],
            5.0m, "Bán chạy", true,
            "Hưng Yên", "Tháng 7-8", "Túi lưới 1kg/2kg/5kg", "Để ngăn mát 3-5 ngày", "Giao trong ngày tại HN",
            new[] { ("1kg", 69_000m, (decimal?)null, 50), ("2kg", 129_000m, (decimal?)140_000m, 30), ("5kg", 299_000m, (decimal?)350_000m, 20) }
        ));

        list.Add(CreateProduct(
            "nhan-long-tuoi-nguyen-chum", "Nhãn lồng tươi nguyên chùm",
            "Nhãn nguyên chùm, tươi tự nhiên, giữ được lâu hơn", null,
            ProductCategory.Fresh, "kg", "/images/nhan-long-tuoi-nguyen-chum.jpg", [],
            4.8m, null, false,
            "Hưng Yên", "Tháng 7-8", "Thùng xốp 5kg/10kg", "Để ngăn mát 5-7 ngày", "Giao toàn quốc",
            new[] { ("5kg", 320_000m, (decimal?)null, 15), ("10kg", 599_000m, (decimal?)650_000m, 10) }
        ));

        list.Add(CreateProduct(
            "nhan-say-deo", "Nhãn sấy dẻo",
            "Nhãn sấy dẻo nguyên chất, không phụ gia, vị ngọt tự nhiên", null,
            ProductCategory.Dried, "túi", "/images/nhan-say-deo.jpg", [],
            4.9m, "Mới", true,
            "Hưng Yên", "Quanh năm", "Túi zip 250g/500g", "Nơi khô ráo, thoáng mát 3 tháng", "Giao toàn quốc",
            new[] { ("250g", 85_000m, (decimal?)null, 100), ("500g", 159_000m, (decimal?)180_000m, 80) }
        ));

        list.Add(CreateProduct(
            "nhan-say-kho", "Nhãn sấy khô",
            "Nhãn sấy khô truyền thống, bảo quản lâu dài", null,
            ProductCategory.Dried, "túi", "/images/nhan-say-kho.jpg", [],
            4.7m, null, false,
            "Hưng Yên", "Quanh năm", "Túi hút chân không 200g/400g", "Nơi khô ráo 6 tháng", "Giao toàn quốc",
            new[] { ("200g", 65_000m, (decimal?)null, 120), ("400g", 120_000m, (decimal?)130_000m, 90) }
        ));

        list.Add(CreateProduct(
            "combo-qua-tang-co-ban", "Combo quà tặng cơ bản",
            "Hộp quà gồm nhãn tươi 1kg + nhãn sấy dẻo 250g, phù hợp làm quà biếu", null,
            ProductCategory.Combo, "hộp", "/images/combo-co-ban.jpg", [],
            5.0m, "Quà tặng", true,
            "Hưng Yên", null, "Hộp quà sang trọng", "Theo hướng dẫn từng loại", "Giao toàn quốc",
            new[] { ("Combo cơ bản", 149_000m, (decimal?)169_000m, 40) }
        ));

        list.Add(CreateProduct(
            "combo-qua-tang-cao-cap", "Combo quà tặng cao cấp",
            "Hộp quà cao cấp gồm nhãn tươi 2kg + nhãn sấy dẻo 500g + nhãn sấy khô 200g", null,
            ProductCategory.Combo, "hộp", "/images/combo-cao-cap.jpg", [],
            5.0m, "Hot", true,
            "Hưng Yên", null, "Hộp gỗ cao cấp", "Theo hướng dẫn từng loại", "Giao toàn quốc miễn phí",
            new[] { ("Combo cao cấp", 399_000m, (decimal?)450_000m, 25) }
        ));

        return list;
    }

    private static Product CreateProduct(
        string slug, string name, string description, string? fullDescription,
        ProductCategory category, string unit, string image, List<string> images,
        decimal rating, string? badge, bool featured,
        string? origin, string? harvest, string? packaging, string? storage, string? shipping,
        (string Name, decimal Price, decimal? OldPrice, int Stock)[] variants)
    {
        var product = Product.Create(
            slug, name, description, fullDescription, category, unit, image, images,
            rating, badge, featured, origin, harvest, packaging, storage, shipping);

        foreach (var (vName, price, oldPrice, stock) in variants)
            product.AddVariant(ProductVariant.Create(product.Id, vName, price, oldPrice, stock));

        return product;
    }
}
