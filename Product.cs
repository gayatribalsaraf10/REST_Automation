using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

    public static void Print(Product p)
    {
        Console.WriteLine($"ID: {p?.Id}");
        Console.WriteLine($"Name: {p?.Name}");
        Console.WriteLine($"Year: {p?.Data?.Year}");
        Console.WriteLine($"Price: {p?.Data?.Price}");
        Console.WriteLine($"CPU: {p?.Data?.CPUModel}");
        Console.WriteLine($"Disk: {p?.Data?.HardDiskSize}");
    }
}
