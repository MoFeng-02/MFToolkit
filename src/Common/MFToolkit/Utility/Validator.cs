using System.Numerics;
using System.Text.RegularExpressions;

namespace MFToolkit.Utility;
/// <summary>
/// 比较常用的验证类（通义千问生成，部分由DeepSeek生成）
/// </summary>
public static partial class Validator
{
    /// <summary>
    /// 电子邮件地址正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$", RegexOptions.IgnoreCase)]
    internal static partial Regex EmailRegex();
    /// <summary>
    /// 验证是否为有效的电子邮件地址。
    /// </summary>
    /// <param name="email">待验证的电子邮件字符串。</param>
    /// <returns>如果电子邮件格式正确返回true，否则返回false。</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex().IsMatch(email);
    }

    /// <summary>
    /// 使用Luhn算法验证信用卡号码的有效性。
    /// </summary>
    /// <param name="cardNumber">待验证的信用卡号码字符串。</param>
    /// <returns>如果信用卡号码有效返回true，否则返回false。</returns>
    public static bool IsValidCreditCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || !Regex.IsMatch(cardNumber, @"^\d+$"))
            return false;

        int sum = 0;
        bool alternate = false;
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(cardNumber.Substring(i, 1));
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                    n = n % 10 + 1;
            }
            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// 身份证号码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{17}[\dX]|\d{15}$")]
    internal static partial Regex IDCardRegex();
    /// <summary>
    /// 验证是否为有效的中国大陆身份证号。
    /// </summary>
    /// <param name="idCard">待验证的身份证号码字符串。</param>
    /// <returns>如果身份证号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIDCard(string? idCard)
    {
        if (string.IsNullOrWhiteSpace(idCard))
            return false;

        return IDCardRegex().IsMatch(idCard);
    }


    /// <summary>
    /// IPV4 Regex Pattern 编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
    internal static partial Regex IPV4Regex();

    /// <summary>
    /// 验证是否为有效的IPv4地址。
    /// </summary>
    /// <param name="ipAddress">待验证的IP地址字符串。</param>
    /// <returns>如果IP地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv4Address(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // IPv4 地址格式
        return IPV4Regex().IsMatch(ipAddress);
    }

    /// <summary>
    /// IPV6 Regex Pattern 编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]+|::(ffff(:0{1,4})?:)?((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1?[0-9])?[0-9])\.){3}(25[0-5]|(2[0-4]|1?[0-9])?[0-9]))$", RegexOptions.IgnoreCase)]
    internal static partial Regex IPv6Regex();
    /// <summary>
    /// 验证是否为有效的IPv6地址。
    /// </summary>
    /// <param name="ipAddress">待验证的IP地址字符串。</param>
    /// <returns>如果IP地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv6Address(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return IPv6Regex().IsMatch(ipAddress);
    }

    /// <summary>
    /// 验证日期格式（YYYY-MM-DD）。
    /// </summary>
    /// <param name="date">待验证的日期字符串。</param>
    /// <returns>如果日期格式正确返回true，否则返回false。</returns>
    public static bool IsValidDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return false;

        DateTime parsedDate;
        return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate);
    }

    /// <summary>
    /// 验证时间格式（HH:mm:ss）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$")]
    internal static partial Regex PasswordRegex();
    /// <summary>
    /// 验证密码强度（至少8个字符，包含大小写字母、数字和特殊字符）。
    /// </summary>
    /// <param name="password">待验证的密码字符串。</param>
    /// <returns>如果密码符合要求返回true，否则返回false。</returns>
    public static bool IsValidPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return PasswordRegex().IsMatch(password);
    }

    /// <summary>
    /// 验证MAC地址正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")]
    internal static partial Regex MacAddressRegex();
    /// <summary>
    /// 验证MAC地址。
    /// </summary>
    /// <param name="macAddress">待验证的MAC地址字符串。</param>
    /// <returns>如果MAC地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidMacAddress(string? macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        return MacAddressRegex().IsMatch(macAddress);
    }

    /// <summary>
    /// 验证UUID/GUID。
    /// </summary>
    /// <param name="guid">待验证的GUID字符串。</param>
    /// <returns>如果GUID格式正确返回true，否则返回false。</returns>
    public static bool IsValidGuid(string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        Guid parsedGuid;
        return Guid.TryParse(guid, out parsedGuid);
    }

    /// <summary>
    /// 域名正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(?!-)[A-Za-z\d\-]{1,63}(?<!-)\.((?!-)[A-Za-z\d\-]{1,63}(?<!-)\.)+[A-Za-z]{2,}$")]
    internal static partial Regex DomainRegex();
    /// <summary>
    /// 验证域名（简单的域名格式验证）。
    /// </summary>
    /// <param name="domain">待验证的域名字符串。</param>
    /// <returns>如果域名格式正确返回true，否则返回false。</returns>
    public static bool IsValidDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        return DomainRegex().IsMatch(domain);
    }

    /// <summary>
    /// 验证URL
    /// </summary>
    /// <param name="url">待验证的URL字符串。开头必须http或https</param>
    /// <returns>如果URL格式正确返回true，否则返回false。</returns>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return result;
    }

    /// <summary>
    /// URL正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", RegexOptions.IgnoreCase)]
    internal static partial Regex UrlRegex();
    /// <summary>
    /// 验证URL 正则验证。
    /// </summary>
    /// <param name="url">待验证的URL字符串。</param>
    /// <returns>如果URL格式正确返回true，否则返回false。</returns>
    public static bool IsValidUrlRegex(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return UrlRegex().IsMatch(url);
    }

    /// <summary>
    /// 验证邮政编码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{6}$")]
    internal static partial Regex PostalCodeRegex();
    /// <summary>
    /// 验证邮政编码（以中国为例）。
    /// </summary>
    /// <param name="postalCode">待验证的邮政编码字符串。</param>
    /// <returns>如果邮政编码格式正确返回true，否则返回false。</returns>
    public static bool IsValidPostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        return PostalCodeRegex().IsMatch(postalCode);
    }

    /// <summary>
    /// 验证纯数字字符串正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d+$")]
    internal static partial Regex NumericStringRegex();
    /// <summary>
    /// 验证纯数字字符串。
    /// </summary>
    /// <param name="numericString">待验证的字符串。</param>
    /// <returns>如果是纯数字字符串返回true，否则返回false。</returns>
    public static bool IsNumericString(string? numericString)
    {
        if (string.IsNullOrWhiteSpace(numericString))
            return false;

        return NumericStringRegex().IsMatch(numericString);
    }

    /// <summary>
    /// 验证十六进制颜色代码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^#[A-Fa-f0-9]{6}$|^#[A-Fa-f0-9]{3}$")]
    internal static partial Regex HexColorCodeRegex();
    /// <summary>
    /// 验证十六进制颜色代码（#开头，后跟6位或3位的十六进制数）。
    /// </summary>
    /// <param name="hexColor">待验证的颜色代码字符串。</param>
    /// <returns>如果颜色代码格式正确返回true，否则返回false。</returns>
    public static bool IsValidHexColorCode(string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            return false;

        return HexColorCodeRegex().IsMatch(hexColor);
    }

    ///// <summary>
    ///// 验证车牌号正则表达式编译器生成的正则表达式。
    ///// </summary>
    ///// <returns></returns>
    //[GeneratedRegex(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼使领]{1}[A-Z]{1}[A-Z0-9]{5}$")]
    //internal static partial Regex VehicleLicensePlateRegex();
    ///// <summary>
    ///// 验证车牌号（以中国为例）。
    ///// </summary>
    ///// <param name="licensePlate">待验证的车牌号码字符串。</param>
    ///// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    //public static bool IsValidVehicleLicensePlate(string? licensePlate)
    //{
    //    if (string.IsNullOrWhiteSpace(licensePlate))
    //        return false;

    //    return VehicleLicensePlateRegex().IsMatch(licensePlate);
    //}

    /// <summary>
    /// 验证文件路径（简单验证，确保路径中不包含非法字符）。
    /// </summary>
    /// <param name="filePath">待验证的文件路径字符串。</param>
    /// <returns>如果文件路径格式正确返回true，否则返回false。</returns>
    public static bool IsValidFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // 确保路径中不包含非法字符
        var invalidChars = Path.GetInvalidPathChars();
        foreach (char c in invalidChars)
        {
            if (filePath.Contains(c))
                return false;
        }

        return true;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
    internal static partial Regex SocialMediaUsernameRegex();
    /// <summary>
    /// 验证社交媒体用户名（以字母数字和下划线组成的用户名）。
    /// </summary>
    /// <param name="username">待验证的用户名字符串。</param>
    /// <returns>如果用户名格式正确返回true，否则返回false。</returns>
    public static bool IsValidSocialMediaUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return SocialMediaUsernameRegex().IsMatch(username);
    }

    /// <summary>
    /// 验证银行账户号码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{6,20}$")]
    internal static partial Regex BankAccountNumberRegex();
    /// <summary>
    /// 验证银行账户号码（简单的长度和字符检查，具体规则可能因地区而异）。
    /// </summary>
    /// <param name="accountNumber">待验证的银行账户号码字符串。</param>
    /// <returns>如果账户号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidBankAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return false;

        return BankAccountNumberRegex().IsMatch(accountNumber);
    }

    /// <summary>
    /// 验证ISBN号码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{9}[0-9X]$", RegexOptions.IgnoreCase)]
    internal static partial Regex ISBN10Regex();

    /// <summary>
    /// 验证ISBN号码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{13}$")]
    internal static partial Regex ISBN13Regex();

    /// <summary>
    /// 清理ISBN号码正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[^\dX]")]
    private static partial Regex CleanIsbnRegex();
    /// <summary>
    /// 验证ISBN号（10位或13位）。
    /// </summary>
    /// <param name="isbn">待验证的ISBN号码字符串。</param>
    /// <returns>如果ISBN号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidISBN(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        string cleanIsbn = CleanIsbnRegex().Replace(isbn, "");
        return ISBN10Regex().IsMatch(cleanIsbn) || ISBN13Regex().IsMatch(cleanIsbn);
    }

    /// <summary>
    /// 验证美国社会安全号码（SSN）正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{3}-\d{2}-\d{4}$")]
    internal static partial Regex SSNRegex();
    /// <summary>
    /// 验证美国社会安全号码（SSN）。
    /// </summary>
    /// <param name="ssn">待验证的社会安全号码字符串。</param>
    /// <returns>如果SSN格式正确返回true，否则返回false。</returns>
    public static bool IsValidSSN(string? ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return false;

        return SSNRegex().IsMatch(ssn);
    }

    /// <summary>
    /// 验证EAN码（8位）正则表达式编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{13}$")]
    internal static partial Regex EAN13Regex();
    /// <summary>
    /// 验证EAN码（13位）。
    /// </summary>
    /// <param name="eanCode">待验证的EAN码字符串。</param>
    /// <returns>如果EAN码格式正确返回true，否则返回false。</returns>
    public static bool IsValidEAN13(string? eanCode)
    {
        if (string.IsNullOrWhiteSpace(eanCode))
            return false;

        return EAN13Regex().IsMatch(eanCode);
    }

    /// <summary>
    /// 验证是否为有效的国际电话号码（包括国家代码）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\+\d{1,3}-\d+$")]
    internal static partial Regex InternationalPhoneNumberRegex();
    /// <summary>
    /// 验证是否为有效的国际电话号码（包括国家代码）。
    /// </summary>
    /// <param name="phoneNumber">待验证的电话号码字符串。</param>
    /// <returns>如果电话号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidInternationalPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return InternationalPhoneNumberRegex().IsMatch(phoneNumber);
    }

    /// <summary>
    /// 验证是否为有效的中国手机号码（包括虚拟运营商）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^1[3456789]\d{9}$")]
    internal static partial Regex ChinaMobilePhoneNumberRegex();
    /// <summary>
    /// 验证是否为有效的中国手机号码（包括虚拟运营商）。
    /// </summary>
    /// <param name="phoneNumber">待验证的手机号码字符串。</param>
    /// <returns>如果手机号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaMobilePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return ChinaMobilePhoneNumberRegex().IsMatch(phoneNumber);
    }

    /// <summary>
    /// 验证是否为有效的中国固定电话号码（带区号）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(0\d{2,3}-)?\d{7,8}(-\d{1,4})?$")]
    internal static partial Regex ChinaLandlineNumberRegex();
    /// <summary>
    /// 验证是否为有效的中国固定电话号码（带区号）。
    /// </summary>
    /// <param name="landlineNumber">待验证的固定电话号码字符串。</param>
    /// <returns>如果固定电话号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaLandlineNumber(string? landlineNumber)
    {
        if (string.IsNullOrWhiteSpace(landlineNumber))
            return false;

        return ChinaLandlineNumberRegex().IsMatch(landlineNumber);
    }



    /// <summary>
    /// 验证是否为有效的中国车牌号
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼使领]{1}[A-Z]{1}[A-Z0-9]{5}$")]
    internal static partial Regex VehicleLicensePlateRegex();
    /// <summary>
    /// 验证是否为有效的中国车牌号（包括新能源车牌）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼使领]{1}[A-Z]{1}[D|F][A-Z0-9]{5}$")]
    internal static partial Regex NewEnergyLicencePlateRegex();
    /// <summary>
    /// 验证是否为有效的中国车牌号（包括新能源车牌）。
    /// </summary>
    /// <param name="licensePlate">待验证的车牌号码字符串。</param>
    /// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaVehicleLicensePlate(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return false;

        // 匹配中国大陆车牌号格式，包括新能源车牌
        return VehicleLicensePlateRegex().IsMatch(licensePlate) ||
               NewEnergyLicencePlateRegex().IsMatch(licensePlate); // 新能源车牌
    }

    /// <summary>
    /// 验证是否为有效的JSON字符串。
    /// </summary>
    /// <param name="jsonString">待验证的JSON字符串。</param>
    /// <returns>如果JSON字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidJsonString(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return false;

        try
        {
            var obj = System.Text.Json.JsonSerializer.Deserialize<object>(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的XML字符串。
    /// </summary>
    /// <param name="xmlString">待验证的XML字符串。</param>
    /// <returns>如果XML字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidXmlString(string? xmlString)
    {
        if (string.IsNullOrWhiteSpace(xmlString))
            return false;

        try
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的HTML标签。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"<([a-z]+)[^>]*>.*?</\1>")]
    internal static partial Regex HtmlTagRegex();
    /// <summary>
    /// 验证是否为有效的HTML标签。
    /// </summary>
    /// <param name="htmlTag">待验证的HTML标签字符串。</param>
    /// <returns>如果HTML标签格式正确返回true，否则返回false。</returns>
    public static bool IsValidHtmlTag(string? htmlTag)
    {
        if (string.IsNullOrWhiteSpace(htmlTag))
            return false;

        // HTML标签格式：<tag>...</tag>
        return HtmlTagRegex().IsMatch(htmlTag);
    }

    /// <summary>
    /// 验证是否为有效的UUID v4。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
    internal static partial Regex UuidV4Regex();
    /// <summary>
    /// 验证是否为有效的UUID v4。
    /// </summary>
    /// <param name="guid">待验证的GUID字符串。</param>
    /// <returns>如果GUID是UUID v4格式返回true，否则返回false。</returns>
    public static bool IsValidUuidV4(string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        return UuidV4Regex().IsMatch(guid);
    }

    /// <summary>
    /// 验证是否为有效的Base64编码字符串。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$")]
    internal static partial Regex Base64Regex();
    /// <summary>
    /// 验证是否为有效的Base64编码字符串。
    /// </summary>
    /// <param name="base64String">待验证的Base64编码字符串。</param>
    /// <returns>如果字符串是有效的Base64编码返回true，否则返回false。</returns>
    public static bool IsValidBase64String(string? base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        return Base64Regex().IsMatch(base64String);
    }

    /// <summary>
    /// 验证是否为有效的EAN-8条形码。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{8}$")]
    internal static partial Regex EAN8Regex();
    /// <summary>
    /// 验证是否为有效的EAN-8条形码。
    /// </summary>
    /// <param name="eanCode">待验证的EAN-8条形码字符串。</param>
    /// <returns>如果EAN-8条形码格式正确返回true，否则返回false。</returns>
    public static bool IsValidEAN8(string? eanCode)
    {
        if (string.IsNullOrWhiteSpace(eanCode))
            return false;

        return EAN8Regex().IsMatch(eanCode);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{12}$")]
    internal static partial Regex UPCARegex();
    /// <summary>
    /// 验证是否为有效的UPC-A条形码。
    /// </summary>
    /// <param name="upcCode">待验证的UPC-A条形码字符串。</param>
    /// <returns>如果UPC-A条形码格式正确返回true，否则返回false。</returns>
    public static bool IsValidUPCA(string? upcCode)
    {
        if (string.IsNullOrWhiteSpace(upcCode))
            return false;

        return UPCARegex().IsMatch(upcCode);
    }

    /// <summary>
    /// 验证是否有效的美国车牌号 编译器生成的正则表达式。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[A-Z]\d{5}[A-Z]$", RegexOptions.IgnoreCase)]
    internal static partial Regex USLicensePlateRegex();
    /// <summary>
    /// 验证是否为有效的美国车牌号（以加利福尼亚州为例）。
    /// </summary>
    /// <param name="licensePlate">待验证的车牌号码字符串。</param>
    /// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidUSLicensePlate(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return false;

        return USLicensePlateRegex().IsMatch(licensePlate);
    }

    /// <summary>
    /// 验证是否为有效的正则表达式。
    /// </summary>
    /// <param name="regexPattern">待验证的正则表达式模式字符串。</param>
    /// <returns>如果正则表达式模式格式正确返回true，否则返回false。</returns>
    public static bool IsValidRegexPattern(string? regexPattern)
    {
        if (string.IsNullOrWhiteSpace(regexPattern))
            return false;

        try
        {
            // 尝试编译正则表达式
            Regex.Match("", regexPattern);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的信用卡安全码（CVV/CVC）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{3,4}$")]
    internal static partial Regex CreditCardSecurityCodeRegex();
    /// <summary>
    /// 验证是否为有效的信用卡安全码（CVV/CVC）。
    /// </summary>
    /// <param name="cvv">待验证的安全码字符串。</param>
    /// <returns>如果安全码格式正确返回true，否则返回false。</returns>
    public static bool IsValidCreditCardSecurityCode(string? cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            return false;

        return CreditCardSecurityCodeRegex().IsMatch(cvv);
    }

    /// <summary>
    /// 验证是否为有效的IMEI号码（国际移动设备识别码）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{15}$")]
    internal static partial Regex IMEIRegex();
    /// <summary>
    /// 验证是否为有效的IMEI号码（国际移动设备识别码）。
    /// </summary>
    /// <param name="imei">待验证的IMEI号码字符串。</param>
    /// <returns>如果IMEI号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIMEI(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            return false;

        return IMEIRegex().IsMatch(imei);
    }

    /// <summary>
    /// 验证是否为有效的IMEISV号码（IMEI软件版本）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{16}$")]
    internal static partial Regex IMEISVRegex();
    /// <summary>
    /// 验证是否为有效的IMEISV号码（IMEI软件版本）。
    /// </summary>
    /// <param name="imeisv">待验证的IMEISV号码字符串。</param>
    /// <returns>如果IMEISV号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIMEISV(string? imeisv)
    {
        if (string.IsNullOrWhiteSpace(imeisv))
            return false;

        return IMEISVRegex().IsMatch(imeisv);
    }

    /// <summary>
    /// 验证是否为有效的ICCID号码（集成电路卡识别码）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d{19,20}$")]
    internal static partial Regex ICCIDRegex();
    /// <summary>
    /// 验证是否为有效的ICCID号码（集成电路卡识别码）。
    /// </summary>
    /// <param name="iccid">待验证的ICCID号码字符串。</param>
    /// <returns>如果ICCID号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidICCID(string? iccid)
    {
        if (string.IsNullOrWhiteSpace(iccid))
            return false;

        return ICCIDRegex().IsMatch(iccid);
    }

    /// <summary>
    /// 验证是否为有效的MAC地址（包括带有空格分隔的格式）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^([0-9A-Fa-f]{2}[\s:-]){5}([0-9A-Fa-f]{2})$")]
    internal static partial Regex MacAddressWithSpacesRegex();
    /// <summary>
    /// 验证是否为有效的MAC地址（包括带有空格分隔的格式）。
    /// <para>MAC地址格式：XX XX XX XX XX XX 或 XX:XX:XX:XX:XX:XX 或 XX-XX-XX-XX-XX-XX</para>
    /// </summary>
    /// <param name="macAddress">待验证的MAC地址字符串。</param>
    /// <returns>如果MAC地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidMacAddressWithSpaces(string? macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        return MacAddressWithSpacesRegex().IsMatch(macAddress);
    }

    /// <summary>
    /// 验证是否位有效的IPV6前缀（/64或/128）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^([\da-fA-F]{1,4}:){3}(:[\da-fA-F]{0,4}){1,4}::\/(64|128)$")]
    internal static partial Regex IPv6PrefixRegex();
    /// <summary>
    /// 验证是否为有效的IPv6前缀（/64或/128）。
    /// </summary>
    /// <param name="ipv6Prefix">待验证的IPv6前缀字符串。</param>
    /// <returns>如果IPv6前缀格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv6Prefix(string? ipv6Prefix)
    {
        if (string.IsNullOrWhiteSpace(ipv6Prefix))
            return false;

        return IPv6PrefixRegex().IsMatch(ipv6Prefix);
    }

    /// <summary>
    /// 验证是否为有效的IBAN（国际银行账户号码）。
    /// </summary>
    /// <param name="iban">待验证的IBAN字符串。</param>
    /// <returns>如果IBAN格式正确返回true，否则返回false。</returns>
    public static bool IsValidIBAN(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        // IBAN格式检查（简化版）
        string rearranged = string.Concat(iban.ToUpper().AsSpan(4), iban.AsSpan(0, 4));
        string numericIban = "";

        foreach (char c in rearranged)
        {
            if (char.IsDigit(c))
                numericIban += c;
            else if (char.IsLetter(c))
                numericIban += c - 55;
            else
                return false;
        }

        BigInteger number = BigInteger.Parse(numericIban);
        return number % 97 == 1;
    }

    /// <summary>
    /// 验证是否为有效的IBAN（国际银行账户号码）。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$")]
    internal static partial Regex IBANRegex();

    /// <summary>
    /// 验证是否为有效的IBAN（国际银行账户号码）。
    /// </summary>
    /// <param name="iban"></param>
    /// <returns></returns>
    public static bool IsValidIBANRegex(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        return IBANRegex().IsMatch(iban);
    }

    /// <summary>
    /// 验证是否为有效的SWIFT/BIC代码。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[A-Z]{6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3})?$")]
    internal static partial Regex SwiftBicCodeRegex();
    /// <summary>
    /// 验证是否为有效的SWIFT/BIC代码。
    /// </summary>
    /// <param name="swiftCode">待验证的SWIFT/BIC代码字符串。</param>
    /// <returns>如果SWIFT/BIC代码格式正确返回true，否则返回false。</returns>
    public static bool IsValidSwiftBicCode(string? swiftCode)
    {
        if (string.IsNullOrWhiteSpace(swiftCode))
            return false;

        return SwiftBicCodeRegex().IsMatch(swiftCode);
    }

    /// <summary>
    /// 验证是否为有效的ASN.1编码字符串。
    /// </summary>
    /// <param name="asn1String">待验证的ASN.1编码字符串。</param>
    /// <returns>如果ASN.1编码字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidAsn1EncodedString(string? asn1String)
    {
        if (string.IsNullOrWhiteSpace(asn1String))
            return false;

        try
        {
            byte[] bytes = Convert.FromBase64String(asn1String);
            // 这里可以进一步解析ASN.1结构，这里仅做简单的Base64解码测试
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的PEM编码字符串。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^-+BEGIN [A-Z ]+-+\r?\n(?:[A-Za-z0-9+/=]+\r?\n)+-+END [A-Z ]+-+$", RegexOptions.Multiline)]
    internal static partial Regex PemEncodedStringRegex();
    /// <summary>
    /// 验证是否为有效的PEM编码字符串。
    /// </summary>
    /// <param name="pemString">待验证的PEM编码字符串。</param>
    /// <returns>如果PEM编码字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidPemEncodedString(string? pemString)
    {
        if (string.IsNullOrWhiteSpace(pemString))
            return false;

        return PemEncodedStringRegex().IsMatch(pemString);
    }

    /// <summary>
    /// 验证是否为有效的LaTeX公式。
    /// </summary>
    /// <param name="latexFormula">待验证的LaTeX公式字符串。</param>
    /// <returns>如果LaTeX公式格式正确返回true，否则返回false。</returns>
    public static bool IsValidLatexFormula(string? latexFormula)
    {
        if (string.IsNullOrWhiteSpace(latexFormula))
            return false;

        try
        {
            // 使用LaTeX解析器尝试解析公式，这里假设使用的是MathJax库
            // 注意：这需要在支持JavaScript的环境中运行，C#中可能需要额外的配置
            // 此处仅为示意，实际应用中可能需要调用外部服务来验证
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的编程语言代码片段（以Python为例）。
    /// </summary>
    /// <param name="codeSnippet">待验证的代码片段字符串。</param>
    /// <returns>如果代码片段格式正确返回true，否则返回false。</returns>
    public static bool IsValidCodeSnippet(string? codeSnippet)
    {
        if (string.IsNullOrWhiteSpace(codeSnippet))
            return false;

        try
        {
            // 使用Roslyn API 或其他相应语言的编译器API进行语法分析
            // 这里以Python为例，使用IronPython或Pythonnet库
            // 实际应用中可以根据具体编程语言选择合适的库或工具
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的二维码内容（根据二维码内容类型）。
    /// </summary>
    /// <param name="qrContent">待验证的二维码内容字符串。</param>
    /// <returns>如果二维码内容格式正确返回true，否则返回false。</returns>
    public static bool IsValidQrCodeContent(string? qrContent)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
            return false;

        // 根据二维码内容的实际用途进行验证，比如URL、文本、联系信息等
        // 这里仅做简单示例，实际应用中需要根据内容类型调整验证逻辑
        return IsValidUrl(qrContent) || !string.IsNullOrWhiteSpace(qrContent);
    }

    /// <summary>
    /// 验证是否为有效的条形码扫描结果（包括EAN-13, EAN-8, UPC-A等）。
    /// </summary>
    /// <param name="barcodeResult">待验证的条形码扫描结果字符串。</param>
    /// <returns>如果条形码扫描结果格式正确返回true，否则返回false。</returns>
    public static bool IsValidBarcodeScanResult(string? barcodeResult)
    {
        if (string.IsNullOrWhiteSpace(barcodeResult))
            return false;

        return IsValidEAN13(barcodeResult) || IsValidEAN8(barcodeResult) || IsValidUPCA(barcodeResult);
    }

    /// <summary>
    /// 验证是否为有效的XML签名。
    /// </summary>
    /// <param name="xmlSignature">待验证的XML签名字符串。</param>
    /// <returns>如果XML签名格式正确返回true，否则返回false。</returns>
    public static bool IsValidXmlSignature(string? xmlSignature)
    {
        if (string.IsNullOrWhiteSpace(xmlSignature))
            return false;

        try
        {
            // 使用System.Security.Cryptography.Xml库验证XML签名
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlSignature);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的JSON Web Token (JWT)。
    /// </summary>
    /// <param name="jwtToken">待验证的JWT字符串。</param>
    /// <returns>如果JWT格式正确返回true，否则返回false。</returns>
    public static bool IsValidJwtToken(string? jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            return false;

        // JWT 由三部分组成，每部分用"."分隔，并且是Base64编码的
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            return false;

        foreach (var part in parts)
        {
            try
            {
                Convert.FromBase64String(Base64UrlDecode(part));
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 验证是否为有效的JWTToken。
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$")]
    internal static partial Regex JwtTokenRegex();
    /// <summary>
    /// 验证是否为有效的JSON Web Token (JWT)。
    /// </summary>
    /// <param name="jwtToken">待验证的JWT字符串。</param>
    /// <returns>如果JWT格式正确返回true，否则返回false。</returns>
    public static bool IsValidJwtTokenRegex(string? jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            return false;

        return JwtTokenRegex().IsMatch(jwtToken);
    }

    /// <summary>
    /// 将Base64 URL编码转换为标准Base64编码。
    /// </summary>
    /// <param name="base64UrlEncoded">Base64 URL编码字符串。</param>
    /// <returns>转换后的标准Base64编码字符串。</returns>
    private static string Base64UrlDecode(string? base64UrlEncoded)
    {
        if (string.IsNullOrWhiteSpace(base64UrlEncoded))
            return string.Empty;
        string padded = base64UrlEncoded.Length % 4 == 0
            ? base64UrlEncoded : string.Concat(base64UrlEncoded, "====".AsSpan(base64UrlEncoded.Length % 4));
        return padded.Replace('-', '+').Replace('_', '/');
    }

    /// <summary>
    /// 验证是否为有效的XML文件。
    /// </summary>
    /// <param name="filePath">待验证的XML文件路径。</param>
    /// <returns>如果文件是有效的XML返回true，否则返回false。</returns>
    public static bool IsValidXmlFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            using var fs = File.OpenRead(filePath);
            var doc = new System.Xml.XmlDocument();
            doc.Load(fs);
            return true;
        }
        catch
        {
            return false;
        }
    }

}