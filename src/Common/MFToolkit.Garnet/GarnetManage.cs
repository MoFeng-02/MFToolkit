using MFToolkit.Garnet.Interfaces;

namespace MFToolkit.Garnet;
public class GarnetManage
{
#if NET8_0
    private static object _lock = new();
#elif NET9_0_OR_GREATER
    private static Lock _lock = new();
#endif
    private static IGarnetService? _service;
    /// <summary>
    /// 注册基本实例
    /// </summary>
    /// <param name="service"></param>
    public static void Register(IGarnetService service)
    {
        lock (_lock)
        {
            if (_service != null) throw new("请勿多次重复注册");
            _service = service;
        }
    }
    public static IGarnetService Default => _service ?? throw new("未注册连接，无法使用");

}
