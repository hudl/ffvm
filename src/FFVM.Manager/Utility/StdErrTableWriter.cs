using FFVM.Base.IO.BaseTypes;
using FFVM.Manager.Models;
using System.Text;

namespace FFVM.Manager.Utility;

public class StdErrTableWriter
{
    private readonly List<TableColumn> _columns = [];
    private readonly List<string[]> _tableDatas = [];

    public void AddHeader(string header) => _columns.Add(new TableColumn(header));
    public void AddHeader(string header, int headerWidth) => _columns.Add(new TableColumn(header, headerWidth));
    public void AddRow(params string[] columnData) => _tableDatas.Add(columnData);
    public void Write(ILogger logger)
    {
        var columnHeading = string.Join("  ", _columns.Select(c => c.ColumnHeading.PadRight(c.ColumnSize))); 
        logger.WriteStdErr(columnHeading);

        foreach (var tableData in _tableDatas)
        {
            var tableContent = new StringBuilder();
            for (var columnIndex = 0; columnIndex < tableData.Length && columnIndex < _columns.Count; columnIndex++)
            {
                tableContent.Append($"{tableData[columnIndex].PadRight(_columns[columnIndex].ColumnSize)}  ");
            }
            logger.WriteStdErr(tableContent.ToString());
        }
    }

}
