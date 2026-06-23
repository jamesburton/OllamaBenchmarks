int ProductOperations.ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
{
            int count = 0;
            var products = db.Products.Where(p => p.Category == category).ToList();
            foreach (var product in products)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                count++;
                SaveChangesAsync(db);
            }
            return count;
        }