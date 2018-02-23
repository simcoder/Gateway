using GOC.ApiGateway;
using Microsoft.Extensions.Logging;

public abstract class RepositoryBase<T> where T : class
{

    protected readonly IGocHttpClient HttpClient;


    private ILoggerFactory _loggerFactory;

    protected RepositoryBase(IGocHttpClient httpClient, ILoggerFactory loggerFactory)
    {
        HttpClient = httpClient;
        LoggerFactory = loggerFactory;
    }

    protected ILogger<T> Logger { get; set; }

    protected ILoggerFactory LoggerFactory
    {
        get => _loggerFactory;
        set
        {
            Logger = value.CreateLogger<T>();
            _loggerFactory = value;
        }
    }
}