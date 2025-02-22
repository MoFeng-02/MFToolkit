#nullable disable

namespace MFToolkit.Garnet.Models;
public class ConnectConfiguration
{
    public string Address { get; set; }
    public int Port { get; set; }

    public override string ToString()
    {
        return $"{Address}:{Port}";
    }
    public static implicit operator string(ConnectConfiguration config)
    {
        return config.ToString();
    }
}
