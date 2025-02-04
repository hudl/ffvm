namespace FFVM.Manager.Models;

public class TableColumn(string columnHeading, int columnSize)
{
    public TableColumn(string columnHeading)
        : this(columnHeading, columnHeading.Length)
    {
    }

    public int ColumnSize { get; set; } = columnSize;
    public string ColumnHeading { get; set; } = columnHeading;
}
