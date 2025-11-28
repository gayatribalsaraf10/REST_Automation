using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string postUrl = "https://api.restful-api.dev/objects";
        string searchProductName = "iPhone 17";

        try
        {
            Console.WriteLine("Fetching sample product (GET)...");
            await Product.GetDataAsync("https://api.restful-api.dev/objects/2");

            // --- Excel input ---
            string fileName = "Data for POST.xlsx";
            string folderPath = Environment.CurrentDirectory;
            Console.WriteLine($"Using folder path: {folderPath}");

            List<Product> products = ExcelReader.ReadProductsFromExcel(folderPath, fileName);

            if (products.Count == 0)
            {
                Console.WriteLine("No valid products found in Excel. Exiting...");
                return;
            }
            List< Product> newlyAddedProduct   = new List<Product>();

            foreach (var prod in products)
            {
                // --- POST ---
                Console.WriteLine($"\nPosting product: {prod.Name}");
                Product createdProduct = await ApiClient.CreateProductAsync(postUrl, prod);

                if (createdProduct == null)
                {
                    Console.WriteLine("POST failed, skipping...");
                    continue;
                }

                Console.WriteLine("\n--- Created Product ---");
                Product.Print(createdProduct);

                // --- GET ---
                Console.WriteLine("\nFetching created product (GET)...");
                string getUrl = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
                await Product.GetDataAsync(getUrl);
                newlyAddedProduct.Add(createdProduct);
            }
                Product selectedProduct = newlyAddedProduct.Find(p =>p.Name != null &
                p.Name.Equals(searchProductName, StringComparison.OrdinalIgnoreCase));

                if (selectedProduct == null)
                {
                    Console.WriteLine("Product 'iPhone 17' was NOT found among posted products.");
                    return;
                }   

                // --- PUT ---
                Console.WriteLine("\nUpdating product (PUT)...");
                await Product.PutDataAsync(selectedProduct);

                // --- PATCH ---
                Console.WriteLine("\nPatching product (PATCH)...");
                await Product.PatchDataAsync(selectedProduct);

                // --- DELETE ---
                Console.WriteLine("\nDeleting product (DELETE)...");
                await Product.DeleteDataAsync(selectedProduct);
                    Console.WriteLine("\n Process completed for all Excel products.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
