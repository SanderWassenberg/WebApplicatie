using System;

namespace SwapGame_API
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }




        private static readonly string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public static WeatherForecast[] Get(HttpContext httpContext)
        {
            const int num_forecasts = 5;

            var forecast = new WeatherForecast[num_forecasts];
            for (int i = 0; i < num_forecasts; i++)
            {
                forecast[i] = new WeatherForecast()
                {
                    Date         = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary      = summaries[Random.Shared.Next(summaries.Length)],
                };
            }
            return forecast;
        }
    }
}