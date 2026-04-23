using System.Globalization;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// 语言工具类（AOT 兼容）
/// </summary>
public static class LanguageHelper
{
    #region 1. Minecraft 支持的完整语言列表（AOT 安全，无反射）
    /// <summary>
    /// 获取所有 Minecraft 支持的语言（代码 + 显示名称）
    /// </summary>
    public static List<LanguageItem> GetAllMinecraftLanguages()
    {
        return new List<LanguageItem>
        {
            new("zh_cn", "简体中文"),
            new("zh_tw", "繁体中文"),
            new("en_us", "English (US)"),
            new("en_gb", "English (UK)"),
            new("ja_jp", "日本語"),
            new("ko_kr", "한국어"),
            new("de_de", "Deutsch"),
            new("fr_fr", "Français"),
            new("es_es", "Español (España)"),
            new("es_mx", "Español (México)"),
            new("ru_ru", "Русский"),
            new("pt_br", "Português (Brasil)"),
            new("pt_pt", "Português (Portugal)"),
            new("it_it", "Italiano"),
            new("nl_nl", "Nederlands"),
            new("pl_pl", "Polski"),
            new("tr_tr", "Türkçe"),
            new("ar_sa", "العربية"),
            new("cs_cz", "Čeština"),
            new("da_dk", "Dansk"),
            new("fi_fi", "Suomi"),
            new("hu_hu", "Magyar"),
            new("id_id", "Bahasa Indonesia"),
            new("ms_my", "Bahasa Melayu"),
            new("no_no", "Norsk"),
            new("ro_ro", "Română"),
            new("sk_sk", "Slovenčina"),
            new("sv_se", "Svenska"),
            new("th_th", "ไทย"),
            new("vi_vn", "Tiếng Việt"),
            new("uk_ua", "Українська"),
            new("hi_in", "हिन्दी"),
            new("bn_in", "বাংলা"),
            new("ta_in", "தமிழ்"),
            new("te_in", "తెలుగు"),
            new("mr_in", "मराठी"),
            new("gu_in", "ગુજરાતી")
        };
    }

    /// <summary>
    /// 语言项（代码 + 显示名称）
    /// </summary>
    public record LanguageItem(string Code, string DisplayName);
    #endregion

    #region 2. 获取系统本地语言代码（AOT 兼容，无反射）
    /// <summary>
    /// 获取系统默认语言代码（适配 Minecraft 格式：语言码_国家码）
    /// </summary>
    public static string GetSystemLanguageCode()
    {
        try
        {
            // AOT 安全：使用 CultureInfo.CurrentUICulture（无动态特性）
            var culture = CultureInfo.CurrentUICulture;
            var systemCode = culture.Name.Replace('-', '_').ToLowerInvariant();

            // 映射系统语言到 Minecraft 支持的代码（避免非法值）
            return MapToMinecraftCode(systemCode);
        }
        catch
        {
            // 异常时返回默认简体中文
            return "zh_cn";
        }
    }

    /// <summary>
    /// 系统语言码 -> Minecraft 语言码 映射（AOT 安全，无字典反射）
    /// </summary>
    private static string MapToMinecraftCode(string systemCode)
    {
        return systemCode switch
        {
            // 中文
            "zh_cn" or "zh_hans" => "zh_cn",
            "zh_tw" or "zh_hant" or "zh_hk" => "zh_tw",
            // 英语
            "en" or "en_us" => "en_us",
            "en_gb" or "en_uk" => "en_gb",
            // 日语
            "ja" or "ja_jp" => "ja_jp",
            // 韩语
            "ko" or "ko_kr" => "ko_kr",
            // 德语
            "de" or "de_de" => "de_de",
            // 法语
            "fr" or "fr_fr" => "fr_fr",
            // 西班牙语
            "es" or "es_es" => "es_es",
            "es_mx" => "es_mx",
            // 俄语
            "ru" or "ru_ru" => "ru_ru",
            // 葡萄牙语
            "pt" or "pt_br" => "pt_br",
            "pt_pt" => "pt_pt",
            // 其他常见语言
            "it" or "it_it" => "it_it",
            "nl" or "nl_nl" => "nl_nl",
            "pl" or "pl_pl" => "pl_pl",
            "tr" or "tr_tr" => "tr_tr",
            "ar" or "ar_sa" => "ar_sa",
            "cs" or "cs_cz" => "cs_cz",
            "da" or "da_dk" => "da_dk",
            "fi" or "fi_fi" => "fi_fi",
            "hu" or "hu_hu" => "hu_hu",
            "id" or "id_id" => "id_id",
            "ms" or "ms_my" => "ms_my",
            "no" or "no_no" => "no_no",
            "ro" or "ro_ro" => "ro_ro",
            "sk" or "sk_sk" => "sk_sk",
            "sv" or "sv_se" => "sv_se",
            "th" or "th_th" => "th_th",
            "vi" or "vi_vn" => "vi_vn",
            "uk" or "uk_ua" => "uk_ua",
            // 未匹配时默认英文
            _ => "en_us"
        };
    }
    #endregion
}
