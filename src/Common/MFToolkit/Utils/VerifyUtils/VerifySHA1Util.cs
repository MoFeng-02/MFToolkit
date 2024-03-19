using System.Security.Cryptography;
using System.Text;

namespace MFToolkit.Utils.VerifyUtils;
public class VerifySHA1Util
{
    /// <summary>
    /// 校验SHA1
    /// </summary>
    /// <param name="s">待校验的signature签名</param>
    /// <param name="encoding">指定字符格式</param>
    /// <param name="strs">待加密校验的string Array</param>
    /// <returns></returns>
    public static bool VerifySHA1(string s, Encoding? encoding = default, params string[] strs)
    {
        encoding ??= Encoding.UTF8;
        // 字典排序
        Array.Sort(strs);
        // 拼接
        var tmpStr = string.Join("", strs);
        // 校验
        var vBuffer = encoding.GetBytes(tmpStr);
        var data = SHA1.HashData(vBuffer);
        StringBuilder sub = new();
        foreach (var t in data)
        {
            // x表示16进制 而且区分大小写   X输出为大写   x输出为小写
            sub.Append(t.ToString("x2"));
        }
        var vstr = sub.ToString();
        return vstr.ToLower() == s;
    }
    /// <summary>
    /// 返回校验待校验的值
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    public static string VerifySHA1(Encoding? encoding = default, params string[] strs)
    {
        encoding ??= Encoding.UTF8;
        // 字典排序
        Array.Sort(strs);
        // 拼接
        var tmpStr = string.Join("", strs);
        // 校验
        var vBuffer = encoding.GetBytes(tmpStr);
        var data = SHA1.HashData(vBuffer);
        StringBuilder sub = new();
        foreach (var t in data)
        {
            // x表示16进制 而且区分大小写   X输出为大写   x输出为小写
            sub.Append(t.ToString("x2"));
        }
        var vstr = sub.ToString();
        return vstr;
    }
}
