using System.Text.Json.Serialization;

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
