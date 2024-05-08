namespace MFToolkit.Maui.Utils;
public class FilePickerUtil
{
    /// <summary>
    /// 打开选取单个文件
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<FileResult?> PickAsync(PickOptions? options = null)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            return result;
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// 打开选取多个文件
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null)
    {
        try
        {
            var result = await FilePicker.Default.PickMultipleAsync(options);
            return result;
        }
        catch
        {
            return null;
        }
    }
}
