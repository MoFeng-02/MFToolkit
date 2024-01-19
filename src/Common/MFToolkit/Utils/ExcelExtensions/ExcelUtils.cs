using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MFToolkit.Utils.ExcelExtensions;
public class ExcelUtils
{
    private static readonly object lockObject = new();
    /// <summary>
    /// 数据转换成Excel --> DataTable
    /// </summary>
    /// <param name="stream">数据</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DataTable FileToExcel(Stream stream, string fileExtension)
    {
        // 根据文件扩展名选择合适的工作簿
        IWorkbook workbook;
        using DataTable dataTable = new();
        if (fileExtension == ".xls")
        {
            workbook = new HSSFWorkbook(stream); // 旧版的Excel格式（.xls）
        }
        else if (fileExtension == ".xlsx")
        {
            workbook = new XSSFWorkbook(stream); // 较新版的Excel格式（.xlsx）
        }
        else
        {
            throw new Exception("不支持的文件格式");
        } // 获取第一个工作表
        ISheet sheet = workbook.GetSheetAt(0);

        // 读取工作表数据到 DataTable
        IRow headerRow = sheet.GetRow(0);
        int cellCount = headerRow.LastCellNum;

        for (int i = 0; i < cellCount; i++)
        {
            DataColumn column = new(headerRow.GetCell(i).StringCellValue);
            dataTable.Columns.Add(column);
        }

        int rowCount = sheet.LastRowNum;

        for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)
        {
            IRow row = sheet.GetRow(i);

            if (row == null)
                continue;

            DataRow dataRow = dataTable.NewRow();

            for (int j = row.FirstCellNum; j < cellCount; j++)
            {
                if (row.GetCell(j) != null)
                    dataRow[j] = row.GetCell(j).ToString();
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }
    /// <summary>
    /// 数据转换成Excel --> DataTable，优化内存
    /// </summary>
    /// <param name="stream">数据</param>
    /// <param name="fileExtension">文件尾缀</param>
    /// <param name="batchSize">每次处理的行数</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DataTable FileToExcel(Stream stream, string fileExtension, int batchSize = 10000)
    {
        // 根据文件扩展名选择合适的工作簿
        IWorkbook workbook;
        using DataTable dataTable = new();
        if (fileExtension == ".xls")
        {
            workbook = new HSSFWorkbook(stream); // 旧版的Excel格式（.xls）
        }
        else if (fileExtension == ".xlsx")
        {
            workbook = new XSSFWorkbook(stream); // 较新版的Excel格式（.xlsx）
        }
        else
        {
            throw new Exception("不支持的文件格式");
        }

        ISheet sheet = workbook.GetSheetAt(0); // 获取第一个工作表

        IRow headerRow = sheet.GetRow(0);
        int cellCount = headerRow.LastCellNum;

        // 创建表格的列
        for (int i = 0; i < cellCount; i++)
        {
            DataColumn column = new(headerRow.GetCell(i).StringCellValue);
            dataTable.Columns.Add(column);
        }

        int rowCount = sheet.LastRowNum;
        int processedRows = 0;

        List<Task> tasks = new();

        // 按批次读取数据并使用多线程处理
        for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i += batchSize)
        {
            int startRow = i;
            int endRow = Math.Min(i + batchSize - 1, rowCount);

            Task task = Task.Run(() =>
            {
                for (int j = startRow; j <= endRow; j++)
                {
                    IRow row = sheet.GetRow(j);

                    if (row == null)
                        continue;

                    DataRow dataRow = dataTable.NewRow();

                    for (int k = row.FirstCellNum; k < cellCount; k++)
                    {
                        if (row.GetCell(k) != null)
                            dataRow[k] = row.GetCell(k).ToString();
                    }

                    dataTable.Rows.Add(dataRow);
                }
            });

            tasks.Add(task);
            processedRows += (endRow - startRow + 1);

            // 控制同时运行的任务数量，避免过多的内存消耗
            if (tasks.Count >= Environment.ProcessorCount || processedRows >= batchSize)
            {
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
                processedRows = 0;
            }
        }

        Task.WaitAll(tasks.ToArray());

        return dataTable;
    }

    #region 导出
    public class ExportExcelTo
    {
        /// <summary>
        /// 导出名称
        /// </summary>
        public string ExportName { get; set; }
        /// <summary>
        /// 导出列头名称
        /// </summary>
        public List<ExportExcelKey> Keys { get; set; }
        public DataTable Values { get; set; }
    }
    public class ExportExcelKey
    {
        public string Key { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// 是否设置颜色
        /// </summary>
        public bool IsSetColor { get; set; }
        public short FontColor { get; set; } = IndexedColors.Red.Index;
    }

    /// <summary>
    /// 设置单元格字颜色
    /// </summary>
    /// <param name="workbook"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static ICellStyle SetHanderStyle(IWorkbook workbook, ExportExcelKey t)
    {
        ICellStyle headerStyle = workbook.CreateCellStyle();
        //headerStyle.FillForegroundColor = t.Color;
        IFont font = workbook.CreateFont();
        font.Color = t.FontColor;   // 设置字体颜色
        headerStyle.SetFont(font);
        return headerStyle;
    }
    // 创建 DataTable 对象并设置列
    private static DataTable CreateDataTable(List<ExportExcelKey> keys)
    {
        DataTable data = new DataTable();

        foreach (var key in keys)
        {
            DataColumn column = new(key.Key);
            data.Columns.Add(column);
        }

        return data;
    }
    /// <summary>
    /// 快捷导出
    /// </summary>
    /// <param name="savePath">保存路径文件夹</param>
    /// <param name="fileName">保存文件基本名称</param>
    /// <param name="e">待操作的数据，只需要传入Keys即可</param>
    /// <param name="dataTable">数据</param>
    /// <param name="maxSize">最大每页数据量</param>
    /// <param name="isAutoDataTableDispose">是否自动释放DataTable，默认不自动释放</param>
    /// <returns>返回导出的数量</returns>
    public static int ExportExcelUtils(string savePath, string fileName, ExportExcelTo e, DataTable dataTable, int maxSize = 10000, bool isAutoDataTableDispose = false)
    {
        int saveCount = 0;
        lock (lockObject)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            var pages = dataTable.Rows.Count % maxSize == 0 ? dataTable.Rows.Count / maxSize : dataTable.Rows.Count / maxSize + 1;
            List<int> sizes = new();
            for (int i = 0; i < pages; i++)
            {
                sizes.Add(i == pages - 1 ? dataTable.Rows.Count - sizes.Sum() : 10000);
            }
            int skip = 0;

            for (int i = 0; i < pages; i++)
            {
                using DataTable data = CreateDataTable(e.Keys);
                for (int j = 0; j < sizes[i]; j++)
                {
                    var row = dataTable.Rows[skip];
                    DataRow dataRow = data.NewRow();
                    foreach (var item in e.Keys)
                    {
                        if (string.IsNullOrEmpty(item.Key)) continue;
                        if (row.Table.Columns.Contains(item.Key))
                            dataRow[item.Key] = row[item.Key];
                    }
                    data.Rows.Add(dataRow);
                    skip++;
                }
                e.ExportName = fileName + (i + 1);
                e.Values = data;
                using var aa = ExportExcel(e);
                using FileStream fileStream = new(savePath + e.ExportName + ".xlsx", FileMode.OpenOrCreate, FileAccess.Write);

                aa.Write(fileStream, false);
                saveCount++;
            }
            if (isAutoDataTableDispose) dataTable.Dispose();
        }
        return saveCount;
    }
    /// <summary>
    /// 导出Excel
    /// </summary>
    /// <param name="e">导出Excel的实体类</param>
    /// <param name="t"></param>
    /// <param name="isAutoDisposeValues">是否自动释放e的Values，默认不自动释放</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IWorkbook ExportExcel(ExportExcelTo e, string t = "xlsx", bool isAutoDisposeValues = false)
    {
        if (t != "xlsx" && t != "xls") throw new Exception("错误，不支持的导出格式：" + t);
        IWorkbook workbook = t == "xlsx" ? new XSSFWorkbook() : new HSSFWorkbook();
        e.ExportName ??= DateTime.Now.ToString("yyyyMMddHHmmssfff");
        ISheet sheet = workbook.CreateSheet(e.ExportName);
        IRow headerRow = sheet.CreateRow(0);

        // 设置表头
        for (int i = 0; i < e.Keys.Count; i++)
        {
            var a = e.Keys[i];
            ICell headerCell = headerRow.CreateCell(i);
            headerCell.SetCellValue(a.Title);

            if (a.IsSetColor)
            {
                var headerStyle = SetHanderStyle(workbook, a);
                headerCell.CellStyle = headerStyle;
            }
        }
        // 设置内容
        for (int i = 0; i < e.Values.Rows.Count; i++)
        {
            var row = e.Values.Rows[i];
            IRow cells = sheet.CreateRow(i + 1);
            int index = 0;
            foreach (var key in e.Keys)
            {
                var value = row[key.Key];
                cells.CreateCell(index).SetCellValue(value?.ToString());
                index++;
            }
        }
        // 释放DataTable内存
        if (isAutoDisposeValues) e.Values.Dispose();
        return workbook;
    }
    #endregion
}