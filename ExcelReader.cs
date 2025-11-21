using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

public static class ExcelReader
{
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
}
