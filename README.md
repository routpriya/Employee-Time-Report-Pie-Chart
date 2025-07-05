  Pie Chart Visualization in C#

This project generates a **PNG pie chart** based on employee time entry data fetched from a REST API.

 📊 Functionality
- Fetches time entries from the provided API.
- Calculates total hours worked by each employee.
- Generates a **pie chart** showing percentage of total hours per employee.
- Saves the chart as `chart.png`.

 🔧 Requirements
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Windows OS (uses `System.Drawing.Common`, which is supported on Windows)

 ▶️ How to Run
1. Open terminal in the project folder.
2. Run the following command:

```bash
dotnet run
