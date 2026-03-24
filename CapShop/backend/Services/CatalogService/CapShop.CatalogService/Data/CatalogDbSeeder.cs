using CapShop.CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.CatalogService.Data;

public static class CatalogDbSeeder
{
    public static async Task SeedAsync(CatalogDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Categories.AnyAsync())
            return;

        var electronics = new Category { Name = "Electronics", Description = "Phones, laptops, gadgets" };
        var clothing = new Category { Name = "Clothing", Description = "Men and women apparel" };
        var books = new Category { Name = "Books", Description = "Fiction, non-fiction, academic" };
        var home = new Category { Name = "Home & Kitchen", Description = "Appliances and decor" };

        db.Categories.AddRange(electronics, clothing, books, home);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(), Name = "Wireless Earbuds", Description = "Bluetooth 5.3 with noise cancellation",
                Price = 2499, DiscountPrice = 1999, Stock = 50, ImageUrl = "https://www.beatsbydre.com/content/dam/beats/web/product/earbuds/solo-buds/pdp/product-carousel/matte-black/black-01-solobuds.jpg",
                IsFeatured = true, CategoryId = electronics.Id
            },
            new Product
            {
                Id = Guid.NewGuid(), Name = "Laptop Stand", Description = "Aluminum adjustable stand",
                Price = 1299, Stock = 30, ImageUrl = "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcT-UMQ5ZD3pG5MI_6SHu8QzeQHVZyI93PiudBb4KmJZrtfJq-JW8Vyx9NhyVkj15nH1imIIte1T3joUQiI1_x5YeRQ_pKvXp7CfJgR2ezdCV4GFCWGK84o6SonBjvlbQnr9KFG_pgg&usqp=CAc",
                IsFeatured = true, CategoryId = electronics.Id
            },
            new Product
            {
                Id = Guid.NewGuid(), Name = "Cotton T-Shirt", Description = "100% cotton, round neck",
                Price = 599, DiscountPrice = 449, Stock = 100, ImageUrl = "https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcROYUk1YIgCUPave76qP0S9zFz4EO22DmbkHRsk4db94yvoiAHUFpkTg5sxYnP_tSMJXm2ECkGl7v0OQVY7f2a0lk8r-c635LeO9V3x9xayXnxUa24amEacuqfCpNOehOW-MVM0E1dPlMU&usqp=CAc",
                IsFeatured = false, CategoryId = clothing.Id
            },
            new Product
            {
                Id = Guid.NewGuid(), Name = "Running Shoes", Description = "Lightweight mesh running shoes",
                Price = 3499, DiscountPrice = 2799, Stock = 25, ImageUrl = "https://encrypted-tbn3.gstatic.com/shopping?q=tbn:ANd9GcQxvBtJDdmOdvQlKlrbTS1tU-C53Jov0lINem6dTAeBrA0u6xtMVs-7fcEJ2YRGCRQ2cUd8oGukzZp4Ns3EshrO4CKJPSBiKzSvu5RWIrxKeZ4HAKS_NPc-QiUikb9NoG3wZ9AF0Xw&usqp=CAc",
                IsFeatured = true, CategoryId = clothing.Id
            },
            new Product
            {
                Id = Guid.NewGuid(), Name = "Clean Code", Description = "Robert C. Martin - A handbook of agile software craftsmanship",
                Price = 499, Stock = 40, ImageUrl = "https://encrypted-tbn3.gstatic.com/shopping?q=tbn:ANd9GcRqWZoVFyllWP2kEUs6A-M7VLIbTorw6Yrrf_a88uZQVR3H-58N7myjbKHwXPz-QMs2-mTns5o6IqJtM6KaZW9jTe1B1PLPNuMrYc6rnLPLBX8Mk2beI9nPoR0VVWFW460j7FjsmQ&usqp=CAc",
                IsFeatured = true, CategoryId = books.Id
            },
            new Product
            {
                Id = Guid.NewGuid(), Name = "Stainless Steel Bottle", Description = "500ml insulated water bottle",
                Price = 799, DiscountPrice = 599, Stock = 60, ImageUrl = "https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcR4ZhVKKO1iU8JChR8sICFkVIg9xOkI7D2nssEAb3GYha2SuaGfCSO5bCAV_YMbUhjVjRQkslLoGz1vJvYW4tUZ5UedVOKIKz37ewlcfin9jQZA-nlPrCCoNIRnNLjaVaz0BTVTWQ&usqp=CAc",
                IsFeatured = false, CategoryId = home.Id
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}