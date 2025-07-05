using System;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

class Program
{
    class TimeEntry
    {
        public string? EmployeeName { get; set; }
        public string? StarTimeUtc { get; set; }
        public string? EndTimeUtc { get; set; }
    }

    class EmployeeTotal
    {
        public string Name { get; set; } = "";
        public double Hours { get; set; }
    }

    static async Task Main()
    {
        Console.WriteLine("⏳ Fetching time entries...");

        string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        var client = new HttpClient();
        string json = await client.GetStringAsync(apiUrl);

        var entries = JsonSerializer.Deserialize<List<TimeEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (entries == null)
        {
            Console.WriteLine("❌ No entries found.");
            return;
        }

        var grouped = new Dictionary<string, double>();

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.EmployeeName)) continue;

            if (DateTime.TryParse(entry.StarTimeUtc, null, DateTimeStyles.AdjustToUniversal, out var start) &&
                DateTime.TryParse(entry.EndTimeUtc, null, DateTimeStyles.AdjustToUniversal, out var end))
            {
                double hours = (end - start).TotalHours;
                if (hours < 0) hours = 0;

                if (grouped.ContainsKey(entry.EmployeeName))
                    grouped[entry.EmployeeName] += hours;
                else
                    grouped[entry.EmployeeName] = hours;
            }
        }

        var employees = grouped.Select(e => new EmployeeTotal
        {
            Name = e.Key,
            Hours = Math.Round(e.Value, 2)
        }).ToList();

        Console.WriteLine($"✅ Processed {employees.Count} employees.");

        GeneratePieChart(employees, "employee_chart.png");
        Console.WriteLine("📊 Pie chart saved as 'employee_chart.png'");
    }

    static void GeneratePieChart(List<EmployeeTotal> data, string outputPath)
    {
        int width = 600, height = 600;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        float total = (float)data.Sum(d => d.Hours);
        float startAngle = 0;

        var rand = new Random();
        var paint = new SKPaint { IsAntialias = true };
        var font = new SKPaint { Color = SKColors.Black, TextSize = 14 };

        int legendY = 20;

        foreach (var emp in data)
        {
            float sweep = (float)(emp.Hours / total) * 360f;
            paint.Color = new SKColor(
                (byte)rand.Next(256),
                (byte)rand.Next(256),
                (byte)rand.Next(256)
            );

            canvas.DrawArc(new SKRect(50, 50, 450, 450), startAngle, sweep, true, paint);
            canvas.DrawRect(470, legendY, 20, 20, paint);
            canvas.DrawText($"{emp.Name} ({emp.Hours:F1} hrs)", 500, legendY + 15, font);

            startAngle += sweep;
            legendY += 30;
        }

        using var image = surface.Snapshot();
        using var dataImg = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        dataImg.SaveTo(stream);
    }
}
