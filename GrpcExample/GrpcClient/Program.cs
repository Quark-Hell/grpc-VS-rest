using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

using Grpc.Net.Client;
using Grpc.Core;

using Newtonsoft.Json;

using WeatherClient;
using System.Threading.Channels;

public class WeatherResponse
{
    public string City { get; set; }
    public double Temperature { get; set; }
    public string Description { get; set; }
}

public class Program
{
    private static readonly HttpClient client = new HttpClient();


    #region Rest

    public static async Task GetWeatherRest()
    {
        var city = "Москва";
        try
        {
            var response = await client.GetStringAsync($"https://localhost:7276/api/weather/current?city={city}");

            var currentWeather = JsonConvert.DeserializeObject<WeatherResponse[]>(response);

            foreach (var wr in currentWeather)
            {
                Console.WriteLine($"Текущая погода (REST): {wr.City}, {wr.Temperature}°C, {wr.Description}");
            }
            Console.WriteLine("=================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запросе REST API: {ex.Message}");
        }
    }

    public static async Task MonitorWeatherRest()
    {
        var city = "Москва";
        try
        {
            var stream = await client.GetStreamAsync($"https://localhost:7276/api/weather/monitor?city={city}");
            using var reader = new System.IO.StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var json = line.Substring(6);
                    var weather = JsonConvert.DeserializeObject<WeatherResponse[]>(json);
                    foreach (var wr in weather)
                    {
                        Console.WriteLine($"Текущая погода (REST): {wr.City}, {wr.Temperature}°C, {wr.Description}");
                    }
                    Console.WriteLine("=================================================");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при мониторинге REST API: {ex.Message}");
        }
    }

    public static async Task TestRest()
    {
        var city = "Москва";

        var response = await client.GetStringAsync($"https://localhost:7276/api/weather/current?city={city}");
    }

    public static async Task BigTestRest()
    {
        var city = "Москва";

        var response = await client.GetStringAsync($"https://localhost:7276/api/weather/test?city={city}");
    }


    #endregion

    #region GPRC

    public static async Task GetWeatherGRPC()
    {
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var grpcClient = new WeatherService.WeatherServiceClient(channel);

        var currentWeather = await grpcClient.GetCurrentWeathersAsync(
            new WeatherRequest { City = "Москва" });

        foreach (var wr in currentWeather.WeatherResponses)
        {
            Console.WriteLine($"Текущая погода (gRPC): {wr.City}, {wr.Temperature}°C, {wr.Description}");
        }
        Console.WriteLine("=================================================");
    }

    public static async Task MonitorWeatherGRPC()
    {
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var grpcClient = new WeatherService.WeatherServiceClient(channel);

        using var monitoring = grpcClient.MonitorWeathers(new WeatherRequest { City = "Москва" });

        try
        {
            await foreach (var weather in monitoring.ResponseStream.ReadAllAsync())
            {
                foreach (var response in weather.WeatherResponses)
                {
                    Console.WriteLine($"Мониторинг погоды (gRPC): {response.City}, {response.Temperature}°C, {response.Description}");
                }
                Console.WriteLine("=================================================");
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"Ошибка при запросе gRPC: {ex.Message}");
        }
    }

    public static async Task TestGRPC()
    {
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var grpcClient = new WeatherService.WeatherServiceClient(channel);

        var currentWeather = await grpcClient.GetCurrentWeathersAsync(
            new WeatherRequest { City = "Москва" });
    }

    public static async Task BigTestGRPC()
    {
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
        {
            HttpHandler = httpHandler
        });

        var grpcClient = new WeatherService.WeatherServiceClient(channel);

        var currentWeather = await grpcClient.TestGRPCAsync(
            new WeatherRequest { City = "Москва" });
    }

    #endregion













    public static async Task Main(string[] args)
    {
        Console.WriteLine("Выберите режим работы:");
        Console.WriteLine("1 - REST");
        Console.WriteLine("2 - gRPC");
        Console.WriteLine("3 - стресс тест");
        var choice = Console.ReadLine();



        switch (choice)
        {
            case "1":
                await GetWeatherRest();
                Console.WriteLine("Начинаем мониторинг погоды через REST...");
                Console.WriteLine("=================================================");
                await MonitorWeatherRest();
                break;

            case "2":
                await GetWeatherGRPC();
                Console.WriteLine("Начинаем мониторинг погоды через GRPC...");
                Console.WriteLine("=================================================");
                await MonitorWeatherGRPC();
                break;

            case "3":
                var restSW = Stopwatch.StartNew();
                var grpcSW = Stopwatch.StartNew();

                Console.WriteLine("Начинаем тестирование. Это может занять пару минут. Пожалуйста, подождите...");

                for (int i = 0; i < 10000; i++)
                {
                    await TestRest();

                }
                restSW.Stop();

                for (int i = 0; i < 10000; i++)
                {
                    await TestGRPC();
                }
                grpcSW.Stop();

                Console.WriteLine($"Rest - Время для 10000 сообщений: {restSW.ElapsedMilliseconds} мс");
                Console.WriteLine($"GRPC - Время для 10000 сообщений: {grpcSW.ElapsedMilliseconds} мс");

                Console.WriteLine("");
                Console.WriteLine("Проводим второе тестирование. Пожалуйста, подождите...");

                grpcSW.Reset();
                grpcSW.Start();
                await BigTestRest();
                restSW.Stop();

                grpcSW.Reset();
                grpcSW.Start();
                await BigTestGRPC();
                grpcSW.Stop();

                Console.WriteLine($"Rest - Время для принятия одного большого сообщения: {restSW.ElapsedMilliseconds} мс");
                Console.WriteLine($"GRPC - Время для принятия одного большого сообщения: {grpcSW.ElapsedMilliseconds} мс");

                break;

            default:
                Console.WriteLine("Некорректный выбор.");
                break;
        }
    }
}