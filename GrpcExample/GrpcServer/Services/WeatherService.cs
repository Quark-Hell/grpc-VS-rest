using Grpc.Core;
using WeatherServer;

namespace GrpcServer.Services;

public class WeatherServiceImpl : WeatherService.WeatherServiceBase
{
    private readonly Random _random = new Random();

    public override Task<WeatherResponse> GetCurrentWeather(
        WeatherRequest request, ServerCallContext context)
    {
        return Task.FromResult(new WeatherResponse
        {
            City = request.City,
            Temperature = _random.Next(10, 30),
            Description = "Солнечно",
            Timestamp = DateTime.UtcNow.ToString("O")
        });
    }
    public override Task<RepWeatherResponse> GetCurrentWeathers(WeatherRequest request, ServerCallContext context)
    {
        WeatherResponse[] wr = new WeatherResponse[5];
        for (int i = 0; i < 5; i++)
        {
            WeatherResponse response = new WeatherResponse();
            response.City = request.City;
            response.Temperature = _random.Next(10, 30);
            response.Description = "Солнечно";
            response.Timestamp = DateTime.UtcNow.ToString("O");
            wr[i] = response;
        }
        return Task.FromResult(new RepWeatherResponse
        {
            WeatherResponses = { wr }
        });
    }

    public override Task<RepWeatherResponse> TestGRPC(WeatherRequest request, ServerCallContext context)
    {
        WeatherResponse[] wr = new WeatherResponse[10000];
        for (int i = 0; i < 10000; i++)
        {
            WeatherResponse response = new WeatherResponse();
            response.City = request.City;
            response.Temperature = _random.Next(10, 30);
            response.Description = "Солнечно";
            response.Timestamp = DateTime.UtcNow.ToString("O");
            wr[i] = response;
        }
        return Task.FromResult(new RepWeatherResponse
        {
            WeatherResponses = { wr }
        });
    }

    public override async Task MonitorWeather(
        WeatherRequest request,
        IServerStreamWriter<WeatherResponse> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var weather = new WeatherResponse
            {
                City = request.City,
                Temperature = _random.Next(10, 30),
                Description = "Солнечно",
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            await responseStream.WriteAsync(weather);
            await Task.Delay(1000, context.CancellationToken);
        }
    }

    public override async Task MonitorWeathers(
        WeatherRequest request,
        IServerStreamWriter<RepWeatherResponse> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            WeatherResponse[] wr = new WeatherResponse[5];
            for (int i = 0; i < 5; i++)
            {
                WeatherResponse response = new WeatherResponse();
                response.City = request.City;
                response.Temperature = _random.Next(10, 30);
                response.Description = "Солнечно";
                response.Timestamp = DateTime.UtcNow.ToString("O");
                wr[i] = response;
            }

            await responseStream.WriteAsync(new RepWeatherResponse { WeatherResponses = { wr } });
            await Task.Delay(1000, context.CancellationToken);
        }
    }


}