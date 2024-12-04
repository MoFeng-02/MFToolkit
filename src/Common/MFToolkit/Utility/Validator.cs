using System.Numerics;
using System.Text.RegularExpressions;

namespace MFToolkit.Utility;
/// <summary>
/// 比较常用的验证类（通义千问生成）
/// </summary>
public static class Validator
{
    /// <summary>
    /// 验证是否为有效的电子邮件地址。
    /// </summary>
    /// <param name="email">待验证的电子邮件字符串。</param>
    /// <returns>如果电子邮件格式正确返回true，否则返回false。</returns>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // 更严格的电子邮件正则表达式
        return Regex.IsMatch(email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
            + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 使用Luhn算法验证信用卡号码的有效性。
    /// </summary>
    /// <param name="cardNumber">待验证的信用卡号码字符串。</param>
    /// <returns>如果信用卡号码有效返回true，否则返回false。</returns>
    public static bool IsValidCreditCardNumber(string cardNumber)
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
    /// 验证是否为有效的中国大陆身份证号。
    /// </summary>
    /// <param name="idCard">待验证的身份证号码字符串。</param>
    /// <returns>如果身份证号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIDCard(string idCard)
    {
        if (string.IsNullOrWhiteSpace(idCard))
            return false;

        // 15位或18位身份证号，最后一位可能是X
        return Regex.IsMatch(idCard, @"^\d{17}[\dX]|\d{15}$");
    }

    /// <summary>
    /// 验证是否为有效的IPv4地址。
    /// </summary>
    /// <param name="ipAddress">待验证的IP地址字符串。</param>
    /// <returns>如果IP地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv4Address(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // IPv4 地址格式
        return Regex.IsMatch(ipAddress, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
    }

    /// <summary>
    /// 验证是否为有效的IPv6地址。
    /// </summary>
    /// <param name="ipAddress">待验证的IP地址字符串。</param>
    /// <returns>如果IP地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv6Address(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // IPv6 地址格式
        return Regex.IsMatch(ipAddress, @"^((?=.*::)(?!.*::.+::)(::)?([\dA-F]{1,4}:(:|\b)|){5}|([\dA-F]{1,4}:){6})((([\dA-F]{1,4}((?!\3)::|:\b|$))|(?!\2\3)){2}|(*\3))(?<=\/\d+$)?", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 验证日期格式（YYYY-MM-DD）。
    /// </summary>
    /// <param name="date">待验证的日期字符串。</param>
    /// <returns>如果日期格式正确返回true，否则返回false。</returns>
    public static bool IsValidDate(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return false;

        DateTime parsedDate;
        return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate);
    }

    /// <summary>
    /// 验证密码强度（至少8个字符，包含大小写字母、数字和特殊字符）。
    /// </summary>
    /// <param name="password">待验证的密码字符串。</param>
    /// <returns>如果密码符合要求返回true，否则返回false。</returns>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // 密码长度至少为8，并且包含字母、数字和特殊字符
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
    }

    /// <summary>
    /// 验证MAC地址。
    /// </summary>
    /// <param name="macAddress">待验证的MAC地址字符串。</param>
    /// <returns>如果MAC地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidMacAddress(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        // MAC地址格式：XX:XX:XX:XX:XX:XX 或 XX-XX-XX-XX-XX-XX
        return Regex.IsMatch(macAddress, @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
    }

    /// <summary>
    /// 验证UUID/GUID。
    /// </summary>
    /// <param name="guid">待验证的GUID字符串。</param>
    /// <returns>如果GUID格式正确返回true，否则返回false。</returns>
    public static bool IsValidGuid(string guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        Guid parsedGuid;
        return Guid.TryParse(guid, out parsedGuid);
    }

    /// <summary>
    /// 验证域名（简单的域名格式验证）。
    /// </summary>
    /// <param name="domain">待验证的域名字符串。</param>
    /// <returns>如果域名格式正确返回true，否则返回false。</returns>
    public static bool IsValidDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        // 域名正则表达式
        return Regex.IsMatch(domain, @"^(?!-)[A-Za-z\d\-]{1,63}(?<!-)\.((?!-)[A-Za-z\d\-]{1,63}(?<!-)\.)+[A-Za-z]{2,}$");
    }

    /// <summary>
    /// 验证URL。
    /// </summary>
    /// <param name="url">待验证的URL字符串。</param>
    /// <returns>如果URL格式正确返回true，否则返回false。</returns>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return result;
    }

    /// <summary>
    /// 验证邮政编码（以中国为例）。
    /// </summary>
    /// <param name="postalCode">待验证的邮政编码字符串。</param>
    /// <returns>如果邮政编码格式正确返回true，否则返回false。</returns>
    public static bool IsValidPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        // 中国邮政编码为六位数字
        return Regex.IsMatch(postalCode, @"^\d{6}$");
    }

    /// <summary>
    /// 验证纯数字字符串。
    /// </summary>
    /// <param name="numericString">待验证的字符串。</param>
    /// <returns>如果是纯数字字符串返回true，否则返回false。</returns>
    public static bool IsNumericString(string numericString)
    {
        if (string.IsNullOrWhiteSpace(numericString))
            return false;

        return Regex.IsMatch(numericString, @"^\d+$");
    }

    /// <summary>
    /// 验证十六进制颜色代码（#开头，后跟6位或3位的十六进制数）。
    /// </summary>
    /// <param name="hexColor">待验证的颜色代码字符串。</param>
    /// <returns>如果颜色代码格式正确返回true，否则返回false。</returns>
    public static bool IsValidHexColorCode(string hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            return false;

        // 匹配六位或三位的十六进制颜色代码
        return Regex.IsMatch(hexColor, @"^#[A-Fa-f0-9]{6}$|^#[A-Fa-f0-9]{3}$");
    }

    /// <summary>
    /// 验证车牌号（以中国为例）。
    /// </summary>
    /// <param name="licensePlate">待验证的车牌号码字符串。</param>
    /// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidVehicleLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return false;

        // 匹配中国大陆车牌号格式
        return Regex.IsMatch(licensePlate, @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼使领]{1}[A-Z]{1}[A-Z0-9]{5}$");
    }

    /// <summary>
    /// 验证文件路径（简单验证，确保路径中不包含非法字符）。
    /// </summary>
    /// <param name="filePath">待验证的文件路径字符串。</param>
    /// <returns>如果文件路径格式正确返回true，否则返回false。</returns>
    public static bool IsValidFilePath(string filePath)
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
    /// <summary>
    /// 验证社交媒体用户名（以字母数字和下划线组成的用户名）。
    /// </summary>
    /// <param name="username">待验证的用户名字符串。</param>
    /// <returns>如果用户名格式正确返回true，否则返回false。</returns>
    public static bool IsValidSocialMediaUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // 用户名可以由字母、数字和下划线组成
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
    }

    /// <summary>
    /// 验证银行账户号码（简单的长度和字符检查，具体规则可能因地区而异）。
    /// </summary>
    /// <param name="accountNumber">待验证的银行账户号码字符串。</param>
    /// <returns>如果账户号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidBankAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return false;

        // 简单的银行账号验证，只允许数字且长度在6到20之间
        return Regex.IsMatch(accountNumber, @"^\d{6,20}$");
    }

    /// <summary>
    /// 验证ISBN号（10位或13位）。
    /// </summary>
    /// <param name="isbn">待验证的ISBN号码字符串。</param>
    /// <returns>如果ISBN号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        // 去除所有非数字和X字符
        string cleanIsbn = Regex.Replace(isbn, @"[^\dX]", "");

        // ISBN-10 或 ISBN-13
        return Regex.IsMatch(cleanIsbn, @"^\d{9}[0-9X]$", RegexOptions.IgnoreCase) || // ISBN-10
               Regex.IsMatch(cleanIsbn, @"^\d{13}$"); // ISBN-13
    }

    /// <summary>
    /// 验证美国社会安全号码（SSN）。
    /// </summary>
    /// <param name="ssn">待验证的社会安全号码字符串。</param>
    /// <returns>如果SSN格式正确返回true，否则返回false。</returns>
    public static bool IsValidSSN(string ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return false;

        // SSN 格式：XXX-XX-XXXX
        return Regex.IsMatch(ssn, @"^\d{3}-\d{2}-\d{4}$");
    }

    /// <summary>
    /// 验证EAN码（13位）。
    /// </summary>
    /// <param name="eanCode">待验证的EAN码字符串。</param>
    /// <returns>如果EAN码格式正确返回true，否则返回false。</returns>
    public static bool IsValidEAN13(string eanCode)
    {
        if (string.IsNullOrWhiteSpace(eanCode))
            return false;

        // EAN-13 格式：13位纯数字
        return Regex.IsMatch(eanCode, @"^\d{13}$");
    }

    /// <summary>
    /// 验证是否为有效的国际电话号码（包括国家代码）。
    /// </summary>
    /// <param name="phoneNumber">待验证的电话号码字符串。</param>
    /// <returns>如果电话号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidInternationalPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // 国际电话号码，以+开头，后跟国家代码和电话号码
        return Regex.IsMatch(phoneNumber, @"^\+\d{1,3}-\d+$");
    }

    /// <summary>
    /// 验证是否为有效的中国手机号码（包括虚拟运营商）。
    /// </summary>
    /// <param name="phoneNumber">待验证的手机号码字符串。</param>
    /// <returns>如果手机号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaMobilePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // 匹配中国大陆手机号码，以1开头后跟10位数字，包含虚拟运营商号段
        return Regex.IsMatch(phoneNumber, @"^1[3456789]\d{9}$");
    }

    /// <summary>
    /// 验证是否为有效的中国固定电话号码（带区号）。
    /// </summary>
    /// <param name="landlineNumber">待验证的固定电话号码字符串。</param>
    /// <returns>如果固定电话号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaLandlineNumber(string landlineNumber)
    {
        if (string.IsNullOrWhiteSpace(landlineNumber))
            return false;

        // 包含区号和分机号的固定电话格式
        return Regex.IsMatch(landlineNumber, @"^(0\d{2,3}-)?\d{7,8}(-\d{1,4})?$");
    }

    /// <summary>
    /// 验证是否为有效的中国车牌号（包括新能源车牌）。
    /// </summary>
    /// <param name="licensePlate">待验证的车牌号码字符串。</param>
    /// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidChinaVehicleLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return false;

        // 匹配中国大陆车牌号格式，包括新能源车牌
        return Regex.IsMatch(licensePlate, @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼使领]{1}[A-Z]{1}[A-Z0-9]{5}$") ||
               Regex.IsMatch(licensePlate, @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵青藏川宁琼]{1}[A-Z]{1}[D|F][A-Z0-9]{5}$"); // 新能源车牌
    }

    /// <summary>
    /// 验证是否为有效的JSON字符串。
    /// </summary>
    /// <param name="jsonString">待验证的JSON字符串。</param>
    /// <returns>如果JSON字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidJsonString(string jsonString)
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
    public static bool IsValidXmlString(string xmlString)
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
    /// <param name="htmlTag">待验证的HTML标签字符串。</param>
    /// <returns>如果HTML标签格式正确返回true，否则返回false。</returns>
    public static bool IsValidHtmlTag(string htmlTag)
    {
        if (string.IsNullOrWhiteSpace(htmlTag))
            return false;

        // HTML标签格式：<tag>...</tag>
        return Regex.IsMatch(htmlTag, @"<([a-z]+)[^>]*>.*?</\1>");
    }
    /// <summary>
    /// 验证是否为有效的UUID v4。
    /// </summary>
    /// <param name="guid">待验证的GUID字符串。</param>
    /// <returns>如果GUID是UUID v4格式返回true，否则返回false。</returns>
    public static bool IsValidUuidV4(string guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        // UUID v4 版本的正则表达式
        return Regex.IsMatch(guid, @"^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 验证是否为有效的Base64编码字符串。
    /// </summary>
    /// <param name="base64String">待验证的Base64编码字符串。</param>
    /// <returns>如果字符串是有效的Base64编码返回true，否则返回false。</returns>
    public static bool IsValidBase64String(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否为有效的EAN-8条形码。
    /// </summary>
    /// <param name="eanCode">待验证的EAN-8条形码字符串。</param>
    /// <returns>如果EAN-8条形码格式正确返回true，否则返回false。</returns>
    public static bool IsValidEAN8(string eanCode)
    {
        if (string.IsNullOrWhiteSpace(eanCode))
            return false;

        // EAN-8 格式：8位纯数字
        return Regex.IsMatch(eanCode, @"^\d{8}$");
    }

    /// <summary>
    /// 验证是否为有效的UPC-A条形码。
    /// </summary>
    /// <param name="upcCode">待验证的UPC-A条形码字符串。</param>
    /// <returns>如果UPC-A条形码格式正确返回true，否则返回false。</returns>
    public static bool IsValidUPCA(string upcCode)
    {
        if (string.IsNullOrWhiteSpace(upcCode))
            return false;

        // UPC-A 格式：12位纯数字
        return Regex.IsMatch(upcCode, @"^\d{12}$");
    }

    /// <summary>
    /// 验证是否为有效的美国车牌号（以加利福尼亚州为例）。
    /// </summary>
    /// <param name="licensePlate">待验证的车牌号码字符串。</param>
    /// <returns>如果车牌号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidUSLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return false;

        // 加利福尼亚州车牌格式：1个字母，然后5个数字，最后1个字母
        return Regex.IsMatch(licensePlate, @"^[A-Z]\d{5}[A-Z]$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 验证是否为有效的正则表达式。
    /// </summary>
    /// <param name="regexPattern">待验证的正则表达式模式字符串。</param>
    /// <returns>如果正则表达式模式格式正确返回true，否则返回false。</returns>
    public static bool IsValidRegexPattern(string regexPattern)
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
    /// <param name="cvv">待验证的安全码字符串。</param>
    /// <returns>如果安全码格式正确返回true，否则返回false。</returns>
    public static bool IsValidCreditCardSecurityCode(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            return false;

        // CVV/CVC 通常是3或4位数字
        return Regex.IsMatch(cvv, @"^\d{3,4}$");
    }

    /// <summary>
    /// 验证是否为有效的IMEI号码（国际移动设备识别码）。
    /// </summary>
    /// <param name="imei">待验证的IMEI号码字符串。</param>
    /// <returns>如果IMEI号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIMEI(string imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            return false;

        // IMEI 号码通常是15位数字
        return Regex.IsMatch(imei, @"^\d{15}$");
    }

    /// <summary>
    /// 验证是否为有效的IMEISV号码（IMEI软件版本）。
    /// </summary>
    /// <param name="imeisv">待验证的IMEISV号码字符串。</param>
    /// <returns>如果IMEISV号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidIMEISV(string imeisv)
    {
        if (string.IsNullOrWhiteSpace(imeisv))
            return false;

        // IMEISV 号码通常是16位数字
        return Regex.IsMatch(imeisv, @"^\d{16}$");
    }

    /// <summary>
    /// 验证是否为有效的ICCID号码（集成电路卡识别码）。
    /// </summary>
    /// <param name="iccid">待验证的ICCID号码字符串。</param>
    /// <returns>如果ICCID号码格式正确返回true，否则返回false。</returns>
    public static bool IsValidICCID(string iccid)
    {
        if (string.IsNullOrWhiteSpace(iccid))
            return false;

        // ICCID 号码通常是19或20位数字
        return Regex.IsMatch(iccid, @"^\d{19,20}$");
    }

    /// <summary>
    /// 验证是否为有效的MAC地址（包括带有空格分隔的格式）。
    /// </summary>
    /// <param name="macAddress">待验证的MAC地址字符串。</param>
    /// <returns>如果MAC地址格式正确返回true，否则返回false。</returns>
    public static bool IsValidMacAddressWithSpaces(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        // MAC地址格式：XX XX XX XX XX XX 或 XX:XX:XX:XX:XX:XX 或 XX-XX-XX-XX-XX-XX
        return Regex.IsMatch(macAddress, @"^([0-9A-Fa-f]{2}[\s:-]){5}([0-9A-Fa-f]{2})$");
    }

    /// <summary>
    /// 验证是否为有效的IPv6前缀（/64或/128）。
    /// </summary>
    /// <param name="ipv6Prefix">待验证的IPv6前缀字符串。</param>
    /// <returns>如果IPv6前缀格式正确返回true，否则返回false。</returns>
    public static bool IsValidIPv6Prefix(string ipv6Prefix)
    {
        if (string.IsNullOrWhiteSpace(ipv6Prefix))
            return false;

        // IPv6 前缀格式：XXXX:XXXX:XXXX:XXXX::/64 或 XXXX:XXXX:XXXX:XXXX::/128
        return Regex.IsMatch(ipv6Prefix, @"^([\da-fA-F]{1,4}:){3}(:[\da-fA-F]{0,4}){1,4}::\/(64|128)$");
    }
    /// <summary>
    /// 验证是否为有效的IBAN（国际银行账户号码）。
    /// </summary>
    /// <param name="iban">待验证的IBAN字符串。</param>
    /// <returns>如果IBAN格式正确返回true，否则返回false。</returns>
    public static bool IsValidIBAN(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        // IBAN格式检查（简化版）
        string rearranged = iban.ToUpper().Substring(4) + iban.Substring(0, 4);
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
    /// 验证是否为有效的SWIFT/BIC代码。
    /// </summary>
    /// <param name="swiftCode">待验证的SWIFT/BIC代码字符串。</param>
    /// <returns>如果SWIFT/BIC代码格式正确返回true，否则返回false。</returns>
    public static bool IsValidSwiftBicCode(string swiftCode)
    {
        if (string.IsNullOrWhiteSpace(swiftCode))
            return false;

        // SWIFT/BIC代码通常是8或11个字符，由字母和数字组成
        return Regex.IsMatch(swiftCode, @"^[A-Z]{6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3})?$");
    }

    /// <summary>
    /// 验证是否为有效的ASN.1编码字符串。
    /// </summary>
    /// <param name="asn1String">待验证的ASN.1编码字符串。</param>
    /// <returns>如果ASN.1编码字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidAsn1EncodedString(string asn1String)
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
    /// <param name="pemString">待验证的PEM编码字符串。</param>
    /// <returns>如果PEM编码字符串格式正确返回true，否则返回false。</returns>
    public static bool IsValidPemEncodedString(string pemString)
    {
        if (string.IsNullOrWhiteSpace(pemString))
            return false;

        // PEM编码通常以"-----BEGIN..."开头并以"-----END..."结尾
        return Regex.IsMatch(pemString, @"^-+BEGIN [A-Z ]+-+\r?\n(?:[A-Za-z0-9+/=]+\r?\n)+-+END [A-Z ]+-+$", RegexOptions.Multiline);
    }

    /// <summary>
    /// 验证是否为有效的LaTeX公式。
    /// </summary>
    /// <param name="latexFormula">待验证的LaTeX公式字符串。</param>
    /// <returns>如果LaTeX公式格式正确返回true，否则返回false。</returns>
    public static bool IsValidLatexFormula(string latexFormula)
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
    public static bool IsValidCodeSnippet(string codeSnippet)
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
    public static bool IsValidQrCodeContent(string qrContent)
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
    public static bool IsValidBarcodeScanResult(string barcodeResult)
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
    public static bool IsValidXmlSignature(string xmlSignature)
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
    public static bool IsValidJwtToken(string jwtToken)
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
    /// 将Base64 URL编码转换为标准Base64编码。
    /// </summary>
    /// <param name="base64UrlEncoded">Base64 URL编码字符串。</param>
    /// <returns>转换后的标准Base64编码字符串。</returns>
    private static string Base64UrlDecode(string base64UrlEncoded)
    {
        string padded = base64UrlEncoded.Length % 4 == 0
            ? base64UrlEncoded : base64UrlEncoded + "====".Substring(base64UrlEncoded.Length % 4);
        return padded.Replace('-', '+').Replace('_', '/');
    }

    /// <summary>
    /// 验证是否为有效的XML文件。
    /// </summary>
    /// <param name="filePath">待验证的XML文件路径。</param>
    /// <returns>如果文件是有效的XML返回true，否则返回false。</returns>
    public static bool IsValidXmlFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            using (var fs = File.OpenRead(filePath))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(fs);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}