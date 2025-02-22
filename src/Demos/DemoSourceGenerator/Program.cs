// See https://aka.ms/new-console-template for more information
using MFToolkit.Utility;

// 使用默认格式生成订单号，包含日期和雪花ID
string defaultOrderNumber = SnowflakeId.GeneratePrefixedSnowflakeId();
Console.WriteLine(defaultOrderNumber); // 输出类似 "ORD20250102123456789012345678"

// 使用自定义日期格式生成订单号
string customDateFormatOrderNumber = SnowflakeId.GeneratePrefixedSnowflakeId(dateFormat: "yyMMddHHmmss", format: "{prefix}{date:yyMMddHHmmss}{id}");
Console.WriteLine(customDateFormatOrderNumber); // 输出类似 "ORD250102153045123456789012345678"

// 使用自定义前缀、日期格式和格式模板生成订单号
string prefixedOrderNumber = SnowflakeId.GeneratePrefixedSnowflakeId(format: "{date:yyyyMMdd}-{id}");
Console.WriteLine(prefixedOrderNumber); // 输出类似 "INV-250102-123456789012345678"

Console.ReadLine();