using GOC.ApiGateway;

public class RespositoryBase
{
    protected readonly IGocHttpClient Client;

    public RespositoryBase(IGocHttpClient client)
    {
        Client = client;
    }
}