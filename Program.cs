using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string getUrl = "https://api.restful-api.dev/objects/2";
        string postUrl = "https://api.restful-api.dev/objects";

        try
        {
            // --- GET Request ---
            Console.WriteLine("Fetching product data (GET)...");
            await Product.GetDataAsync(getUrl);

            // --- POST Request (JSON input) ---
            Console.WriteLine("\nEnter JSON data for new product (example below):");
            Console.WriteLine(@"{
    ""name"": ""Apple MacBook Pro 16"",
    ""data"": {
        ""year"": 2019,
        ""price"": 1849.99,
        ""CPU model"": ""Intel Core i9"",
        ""Hard disk size"": ""1 TB""
    }
}");
            Console.Write("\nPaste your JSON here: ");
            string jsonInput = Console.ReadLine();

            Console.WriteLine("\nAdding new product (POST)...");
            Product createdProduct = await Product.PostDataFromJsonAsync(postUrl, jsonInput);

            if (createdProduct == null)
            {
                Console.WriteLine("Failed to create product. Exiting...");
                return;
            }

            // --- GET the posted product ---
            string getUrl2 = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
            await Product.GetDataAsync(getUrl2);

            // --- PUT Request ---
            Console.WriteLine("\nUpdating product (PUT) request...");
            await Product.PutDataAsync(createdProduct);

            // --- PATCH Request ---
            Console.WriteLine("\nUpdating product (PATCH) request for name and CPU Model...");
            await Product.PatchDataAsync(createdProduct);

            // --- DELETE Request ---
            Console.WriteLine("\nDeleting product (DELETE) request...");
            string deletedId = await Product.DeleteDataAsync(createdProduct);

            Console.WriteLine("\nConfirming DELETE action...");
            string getUrl3 = $"https://api.restful-api.dev/objects/{deletedId}";
            await Product.GetDataAsync(getUrl3);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request exception: {e.Message}");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"JSON deserialization exception: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
        }
    }
}

public class ApiClient
{
    private static readonly HttpClient client = new HttpClient();

    static ApiClient()
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // GET
    public static async Task<Product> GetProductAsync(string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"GET Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // POST
    public static async Task<Product> CreateProductAsync(string url, Product newProduct)
    {
        string jsonContent = JsonSerializer.Serialize(newProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"POST Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // PUT
    public static async Task<Product> UpdateProductAsync(string url, Product updatedProduct)
    {
        string jsonContent = JsonSerializer.Serialize(updatedProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PutAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"PUT Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // PATCH
    public static async Task<Product> PatchProductAsync(string url, Product patchData)
    {
        string jsonContent = JsonSerializer.Serialize(patchData);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        HttpResponseMessage response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"PATCH Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // DELETE
    public static async Task<bool> DeleteProductAsync(string url)
    {
        HttpResponseMessage response = await client.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"DELETE Error: {response.StatusCode} - {response.ReasonPhrase}");
            return false;
        }

        Console.WriteLine("Product deleted successfully.");
        return true;
    }
}

public class Product
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("data")]
    public ProductData Data { get; set; }

    public static async Task GetDataAsync(string geturl)
    {
        Product product = await ApiClient.GetProductAsync(geturl);

        if (product != null)
        {
            Console.WriteLine($"ID: {product.Id}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Year: {product.Data?.Year}");
            Console.WriteLine($"Price: {product.Data?.Price}");
            Console.WriteLine($"CPU: {product.Data?.CPUModel}");
            Console.WriteLine($"Disk: {product.Data?.HardDiskSize}");
        }
        else
        {
            Console.WriteLine("Failed to retrieve product data.");
        }
    }

    // NEW METHOD — accept JSON input for POST
    public static async Task<Product> PostDataFromJsonAsync(string postUrl, string jsonData)
    {
        try
        {
            Product newProduct = JsonSerializer.Deserialize<Product>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (newProduct == null)
            {
                Console.WriteLine("Invalid JSON format.");
                return null;
            }

            Product createdProduct = await ApiClient.CreateProductAsync(postUrl, newProduct);

            if (createdProduct != null)
            {
                Console.WriteLine("\n--- New Product Created ---");
                Console.WriteLine($"ID: {createdProduct.Id}");
                Console.WriteLine($"Name: {createdProduct.Name}");
                Console.WriteLine($"Year: {createdProduct.Data?.Year}");
                Console.WriteLine($"Price: {createdProduct.Data?.Price}");
                Console.WriteLine($"CPU: {createdProduct.Data?.CPUModel}");
                Console.WriteLine($"Disk: {createdProduct.Data?.HardDiskSize}");
            }

            return createdProduct;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
            return null;
        }
    }

    public static async Task PutDataAsync(Product createdProduct)
    {
        createdProduct.Name = "Apple MacBook Pro 16 - Updated through PUT";
        createdProduct.Data.Price = 1999.99;
        createdProduct.Data.HardDiskSize = "2 TB";
        string putUrl = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
        Product updatedProduct = await ApiClient.UpdateProductAsync(putUrl, createdProduct);

        if (updatedProduct != null)
        {
            Console.WriteLine("\n--- Product Updated ---");
            Console.WriteLine($"ID: {updatedProduct.Id}");
            Console.WriteLine($"Name: {updatedProduct.Name}");
            Console.WriteLine($"Year: {updatedProduct.Data?.Year}");
            Console.WriteLine($"Price: {updatedProduct.Data?.Price}");
            Console.WriteLine($"CPU: {updatedProduct.Data?.CPUModel}");
            Console.WriteLine($"Disk: {updatedProduct.Data?.HardDiskSize}");
        }
    }

    public static async Task PatchDataAsync(Product createdProduct)
    {
        createdProduct.Name = "Apple MacBook Pro 16 - Updated through PATCH";
        createdProduct.Data.Year = 2019;
        createdProduct.Data.CPUModel = "14 Core CPU";

        string patchUrl = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
        Product patchedProduct = await ApiClient.PatchProductAsync(patchUrl, createdProduct);

        if (patchedProduct != null)
        {
            Console.WriteLine("\n--- Product Patched ---");
            Console.WriteLine($"ID: {patchedProduct.Id}");
            Console.WriteLine($"Name: {patchedProduct.Name}");
            Console.WriteLine($"Year: {patchedProduct.Data?.Year}");
            Console.WriteLine($"Price: {patchedProduct.Data?.Price}");
            Console.WriteLine($"CPU Model: {patchedProduct.Data?.CPUModel}");
            Console.WriteLine($"Hard Disk Size: {patchedProduct.Data?.HardDiskSize}");
        }
    }

    public static async Task<string> DeleteDataAsync(Product createdProduct)
    {
        string deleteUrl = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
        bool isDeleted = await ApiClient.DeleteProductAsync(deleteUrl);

        if (isDeleted)
        {
            Console.WriteLine($"\n--- Product Deleted ---");
            Console.WriteLine($"Deleted Product ID: {createdProduct.Id}");
        }

        return createdProduct.Id;
    }
}

public class ProductData
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("CPU model")]
    public string CPUModel { get; set; }

    [JsonPropertyName("Hard disk size")]
    public string HardDiskSize { get; set; }
}
