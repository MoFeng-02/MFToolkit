namespace MFToolkit.Minecraft.Entities.Launcher;

/// <summary>
/// 启动结果
/// </summary>
public class LaunchResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ProcessId { get; set; }
    public DateTime LaunchTime { get; set; } = DateTime.Now;
}
