using MFToolkit.Utils.AppExtensions;

namespace MFToolkit.Http.HttpClientFactorys;
internal partial class HttpClientExtension
{
    // 单例实例
    internal static readonly HttpClientExtension Instance = new();
    private readonly HttpClientFactoryService? clientFactoryService;
    internal HttpClientExtension()
    {
        try
        {
            clientFactoryService = AppUtil.GetService<HttpClientFactoryService>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("未找到获取Service", ex);
        }
    }
    internal HttpClient CreateHttpClient(string name = null)
    {
        var result = clientFactoryService?.CreateHttpClient(name);
        return result ?? throw new Exception("未实例化HTTP Client");
    }
}
