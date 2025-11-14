using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ClosedXML.Excel;

// ---------------- MAIN PROGRAM ----------------
public class Program
{
    public static async Task Main(string[] args)
    {
        string postUrl = "https://api.restful-api.dev/objects";

        try
        {
            Console.WriteLine("Fetching sample product (GET)...");
            await Product.GetDataAsync("https://api.restful-api.dev/objects/2");

            // --- Excel input ---
            
            string fileName = "Data for POST.xlsx";
            string folderPath = Environment.CurrentDirectory;
            Console.WriteLine($"Using folder path: {folderPath}");

            List<Product> products = Product.ReadProductsFromExcel(folderPath, fileName);

            if (products.Count == 0)
            {
                Console.WriteLine("No valid products found in Excel. Exiting...");
                return;
            }

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

                // --- PUT ---
                Console.WriteLine("\nUpdating product (PUT)...");
                await Product.PutDataAsync(createdProduct);

                // --- PATCH ---
                Console.WriteLine("\nPatching product (PATCH)...");
                await Product.PatchDataAsync(createdProduct);

                // --- DELETE ---
                Console.WriteLine("\nDeleting product (DELETE)...");
                await Product.DeleteDataAsync(createdProduct);
            }

            Console.WriteLine("\n✅ Process completed for all Excel products.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error: {ex.Message}");
        }
    }
}

// ---------------- API CLIENT ----------------
public class ApiClient
{
    public static readonly HttpClient client = new HttpClient();

    static ApiClient()
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // GET
    public static async Task<Product> GetProductAsync(string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        string jsonResponse = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"GET Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        return JsonSerializer.Deserialize<Product>(jsonResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // POST
    public static async Task<Product> CreateProductAsync(string url, Product newProduct)
    {
        string jsonContent = JsonSerializer.Serialize(newProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"POST Error: {response.StatusCode}");
            return null;
        }

        return JsonSerializer.Deserialize<Product>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // PUT
    public static async Task<Product> UpdateProductAsync(string url, Product updatedProduct)
    {
        string jsonContent = JsonSerializer.Serialize(updatedProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PutAsync(url, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"PUT Error: {response.StatusCode}");
            return null;
        }

        return JsonSerializer.Deserialize<Product>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // PATCH
    public static async Task<Product> PatchProductAsync(string url, object patchData)
    {
        string jsonContent = JsonSerializer.Serialize(patchData);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = await client.SendAsync(request);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"PATCH Error: {response.StatusCode}");
            return null;
        }

        return JsonSerializer.Deserialize<Product>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // DELETE
    public static async Task<bool> DeleteProductAsync(string url)
    {
        HttpResponseMessage response = await client.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"DELETE Error: {response.StatusCode}");
            return false;
        }

        Console.WriteLine("Product deleted successfully.");
        return true;
    }
}

// ---------------- PRODUCT CLASS ----------------
public class Product
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("data")]
    public ProductData Data { get; set; }

    // GET
    public static async Task GetDataAsync(string url)
    {
        Product product = await ApiClient.GetProductAsync(url);
        if (product != null) Print(product);
    }

    // PUT
    public static async Task PutDataAsync(Product product)
    {
        var updated = new Product
        {
            Id = product.Id,
            Name = product.Name + " - Updated via PUT",
            Data = new ProductData
            {
                Year = product.Data.Year,
                Price = product.Data.Price + 100,
                CPUModel = product.Data.CPUModel,
                HardDiskSize = "2 TB"
            }
        };

        string putUrl = $"https://api.restful-api.dev/objects/{product.Id}";
        Product result = await ApiClient.UpdateProductAsync(putUrl, updated);

        if (result != null)
        {
            Console.WriteLine("\n--- Product Updated (PUT) ---");
            Print(result);
        }
    }

    // PATCH
    public static async Task PatchDataAsync(Product product)
    {
        var patchBody = new
        {
            name = product.Name + " - Patched",
            data = new Dictionary<string, object>
                {
                    ["CPU model"] = "14-Core CPU"
                }
        };

        string patchUrl = $"https://api.restful-api.dev/objects/{product.Id}";
        Product patched = await ApiClient.PatchProductAsync(patchUrl, patchBody);

        if (patched != null)
        {
            Console.WriteLine("\n--- Product Patched ---");
            Print(patched);
        }
    }

    // DELETE
    public static async Task DeleteDataAsync(Product product)
    {
        string deleteUrl = $"https://api.restful-api.dev/objects/{product.Id}";
        bool deleted = await ApiClient.DeleteProductAsync(deleteUrl);

        if (deleted)
        {
            Console.WriteLine("\n--- Product Deleted ---");
            Console.WriteLine($"Deleted Product ID: {product.Id}");
        }
    }

    // --- EXCEL READER ---
    public static List<Product> ReadProductsFromExcel(string folderPath, string fileName)
    {
        var products = new List<Product>();
        string filePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"❌ File not found: {filePath}");
            return products;
        }

        try
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var range = worksheet.RangeUsed();

                if (range == null)
                {
                    Console.WriteLine("❌ Excel file is empty.");
                    return products;
                }

                var headers = range.FirstRowUsed()
                                   .Cells()
                                   .Select(c => c.GetString().Trim().ToLowerInvariant())
                                   .ToList();

                string[] expected = { "name", "year", "price", "cpu model", "hard disk size" };

                if (!expected.All(h => headers.Contains(h)))
                {
                    Console.WriteLine("❌ Invalid Excel headers.");
                    Console.WriteLine("Expected: " + string.Join(", ", expected));
                    Console.WriteLine("Found: " + string.Join(", ", headers));
                    return products;
                }

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    try
                    {
                        string name = row.Cell(headers.IndexOf("name") + 1).GetString();
                        int year = Convert.ToInt32(row.Cell(headers.IndexOf("year") + 1).GetDouble());
                        double price = row.Cell(headers.IndexOf("price") + 1).GetDouble();
                        string cpu = row.Cell(headers.IndexOf("cpu model") + 1).GetString();
                        string disk = row.Cell(headers.IndexOf("hard disk size") + 1).GetString();

                        if (string.IsNullOrWhiteSpace(name)) continue;

                        products.Add(new Product
                        {
                            Name = name,
                            Data = new ProductData
                            {
                                Year = year,
                                Price = price,
                                CPUModel = cpu,
                                HardDiskSize = disk
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Skipping invalid row: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error reading Excel: {ex.Message}");
        }

        Console.WriteLine($"\n✅ {products.Count} valid products loaded from Excel.");
        return products;
    }

    public static void Print(Product p)
    {
        Console.WriteLine($"ID: {p.Id}");
        Console.WriteLine($"Name: {p.Name}");
        Console.WriteLine($"Year: {p.Data?.Year}");
        Console.WriteLine($"Price: {p.Data?.Price}");
        Console.WriteLine($"CPU: {p.Data?.CPUModel}");
        Console.WriteLine($"Disk: {p.Data?.HardDiskSize}");
    }
}

// ---------------- PRODUCT DATA CLASS ----------------
public class ProductData
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("CPU model")]
    public string CPUModel { get; set; }

    [JsonPropertyName("hard disk size")]
    public string HardDiskSize { get; set; }
}
