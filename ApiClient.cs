using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
