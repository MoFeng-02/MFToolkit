using System.Data;
using MFToolkit.Json.Extensions;
using SqlSugar;

namespace MFToolkit.SqlSugarExtensions.Converter;
public class UlidToStringConverter : ISugarDataConverter
{
    public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
    {
        var name = "@Ulid" + columnIndex;
        if (columnValue == null) return new SugarParameter(name, null);
        return new SugarParameter(name, columnValue.ToString());
    }

    public T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex)
    {
        var str = dataRecord.GetValue(dataRecordIndex) + "";
        return str.JsonToDeserialize<T>() ?? throw new("错误，转换Ulid出错");
    }
}
