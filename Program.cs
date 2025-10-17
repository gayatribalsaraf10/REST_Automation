using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string getUrl = "https://api.restful-api.dev/objects/1";  // Existing object ID
        string postUrl = "https://api.restful-api.dev/objects";   // POST endpoint

        try
        {
            // --- GET Request ---
            Console.WriteLine("Fetching product data (GET)...");
            Product product = await ApiClient.GetProductAsync(getUrl);

            if (product != null)
            {
                Console.WriteLine("\n--- Product Retrieved ---");
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

            // --- POST Request ---
            Product newProduct = new Product
            {
                Name = "Apple MacBook Pro 16",
                Data = new ProductData
                {
                    Year = 2019,
                    Price = 1849.99,
                    CPUModel = "Intel Core i9",
                    HardDiskSize = "1 TB"
                }
            };

            Console.WriteLine("\nCreating new product (POST)...");
            Product createdProduct = await ApiClient.CreateProductAsync(postUrl, newProduct);

            if (createdProduct != null)
            {
                Console.WriteLine("\n--- New Product Created ---");
                Console.WriteLine($"ID: {createdProduct.Id}");
                Console.WriteLine($"Name: {createdProduct.Name}");
                Console.WriteLine($"Price: {createdProduct.Data?.Price}");
            }
            else
            {
                Console.WriteLine("Failed to create new product.");
            }

            // --- GET the posted product ---
            string getUrl2 = $"https://api.restful-api.dev/objects/{createdProduct.Id}";
            Product postProduct = await ApiClient.GetProductAsync(getUrl2);
            if (postProduct != null)
            {
                Console.WriteLine("\n--- Fetched Posted Product ---");
                Console.WriteLine($"ID: {postProduct.Id}");
                Console.WriteLine($"Name: {postProduct.Name}");
                Console.WriteLine($"Year: {postProduct.Data?.Year}");
                Console.WriteLine($"Price: {postProduct.Data?.Price}");
                Console.WriteLine($"CPU: {postProduct.Data?.CPUModel}");
                Console.WriteLine($"Disk: {postProduct.Data?.HardDiskSize}");
            }

            // --- PUT Request ---
            if (createdProduct != null)
            {
                Console.WriteLine("\nUpdating product (PUT) request...");

                // Modify the product
                createdProduct.Name = "Apple MacBook Pro 16 - Updated";
                createdProduct.Data.Price = 1999.99;
                createdProduct.Data.HardDiskSize = "2 TB";

                string putUrl = $"https://api.restful-api.dev/objects/{createdProduct.Id}";

                Product updatedProduct = await ApiClient.UpdateProductAsync(putUrl, createdProduct);

                if (updatedProduct != null)
                {
                    Console.WriteLine("\n--- Product Updated ---");
                    Console.WriteLine($"ID: {updatedProduct.Id}");
                    Console.WriteLine($"Name: {updatedProduct.Name}");
                    Console.WriteLine($"Price: {updatedProduct.Data?.Price}");
                    Console.WriteLine($"Disk: {updatedProduct.Data?.HardDiskSize}");
                }
                else
                {
                    Console.WriteLine("Failed to update product.");
                }
            }
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

    // GET
    public static async Task<Product> GetProductAsync(string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nRaw GET JSON:\n{jsonResponse}");

            Product product = JsonSerializer.Deserialize<Product>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return product;
        }
        else
        {
            Console.WriteLine($"GET Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
    }

    // POST
    public static async Task<Product> CreateProductAsync(string url, Product newProduct)
    {
        string jsonContent = JsonSerializer.Serialize(newProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nRaw POST JSON:\n{responseBody}");

            Product createdProduct = JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return createdProduct;
        }
        else
        {
            Console.WriteLine($"POST Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
    }

    // PUT
    public static async Task<Product> UpdateProductAsync(string url, Product updatedProduct)
    {
        string jsonContent = JsonSerializer.Serialize(updatedProduct);
        HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PutAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\nRaw PUT JSON:\n{responseBody}");

            Product updated = JsonSerializer.Deserialize<Product>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return updated;
        }
        else
        {
            Console.WriteLine($"PUT Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
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
