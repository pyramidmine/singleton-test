using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace singleton_test;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.WriteLine($"Environment.CurrentDirectory: ${Environment.CurrentDirectory}");
        Console.WriteLine($"Directory.GetCurrentDirectory(): ${Directory.GetCurrentDirectory()}");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.config", optional: false, reloadOnChange: true)
            .Build();

        // 컨피규레이션 파일이 변경됐을 때 호출될 콜백 설정
        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            ConfigurationChangedCallback,
            configuration);

        Config.Instance.Bind(configuration);
        Console.WriteLine(Config.Instance.Server.GetHashCode());
        Console.WriteLine(Config.Instance.Server.Name);
        Console.WriteLine(Config.Instance.Server.Host);

        AutoResetEvent cancelKeyPressed = new(false);
        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
        cancelKeyPressed.WaitOne();

        void OnExit(object? sender, ConsoleCancelEventArgs args)
        {
            cancelKeyPressed.Set();
        }
    }

    private static void ConfigurationChangedCallback(IConfiguration configuration)
    {
        Config.Instance.Bind(configuration);
        Console.WriteLine(Config.Instance.Server.GetHashCode());
        Console.WriteLine(Config.Instance.Server.Name);
        Console.WriteLine(Config.Instance.Server.Host);
    }
}

public sealed class Config
{
    public readonly ServerOptions Server = new();

    public void Bind(IConfiguration configuration)
    {
        Server.Bind(configuration);
    }

    private static Lazy<Config> instance = new Lazy<Config>(() => new Config());

    private Config()
    {
    }

    public static Config Instance
    {
        get
        {
            return instance.Value;
        }
    }
}

public sealed class ServerOptions
{
    public string? Name { get; set; }
    public string? Host { get; set; }

    public void Bind(IConfiguration configuration)
    {
        ServerOptions so = new();
        configuration.Bind("ServerOptions", so);
        this.Name = so.Name;
        this.Host = so.Host;
    }
}
