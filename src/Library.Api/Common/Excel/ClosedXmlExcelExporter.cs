using ClosedXML.Excel;
using System.Reflection;

namespace Library.Api.Common.Excel;

public interface IExcelExporter
{
    byte[] ExportRawToXlsx<T>(
        IReadOnlyList<T> rows,
        Dictionary<string, string>? headerMap,
        string sheetName = "Sheet1");
}

public class ClosedXmlExcelExporter : IExcelExporter
{
    public byte[] ExportRawToXlsx<T>(
        IReadOnlyList<T> rows,
        Dictionary<string, string>? headerMap,
        string sheetName = "Sheet1")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName);

        var allProps = typeof(T).GetProperties()
    .Where(p => p.CanRead)
    .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] props;

        if (headerMap is not null && headerMap.Count > 0)
        {
            // pilih kolom + urutan dari headerMap.Keys
            props = headerMap.Keys
                .Where(k => allProps.ContainsKey(k))
                .Select(k => allProps[k])
                .ToArray();
        }
        else
        {
            // fallback: semua kolom
            props = allProps.Values.ToArray();
        }

        var headerLookup = headerMap is null
    ? null
    : new Dictionary<string, string>(headerMap, StringComparer.OrdinalIgnoreCase);

        // header
        for (int c = 0; c < props.Length; c++)
        {
            var p = props[c];
            var header = p.Name;

            if (headerLookup is not null &&
                headerLookup.TryGetValue(p.Name, out var mapped) &&
                !string.IsNullOrWhiteSpace(mapped))
            {
                header = mapped;
            }

            ws.Cell(1, c + 1).SetValue(header);
            ws.Cell(1, c + 1).Style.Font.Bold = true;
        }


        // rows
        for (int r = 0; r < rows.Count; r++)
        {
            var item = rows[r];
            for (int c = 0; c < props.Length; c++)
            {
                var val = props[c].GetValue(item);
                SetCellValue(ws.Cell(r + 2, c + 1), val);
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        if (value is null)
        {
            cell.SetValue(string.Empty);
            return;
        }

        // Preserve common scalar types
        switch (value)
        {
            case string s:
                const int max = 32767;
                if (s.Length > max) s = s.Substring(0, max - 20) + " ...[TRUNCATED]";
                cell.SetValue(s);
                return;

            case bool b:
                cell.SetValue(b);
                return;

            case DateOnly d:
                cell.SetValue(d.ToDateTime(TimeOnly.MinValue));
                cell.Style.DateFormat.Format = "yyyy-MM-dd";
                return;

            case DateTime dt:
                cell.SetValue(dt);
                // Optional format: uncomment if you want
                // cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                return;

            case DateTimeOffset dto:
                cell.SetValue(dto.DateTime);
                // cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                return;

            case TimeOnly to:
                cell.SetValue(to.ToTimeSpan());
                cell.Style.DateFormat.Format = "HH:mm:ss";
                return;

            case TimeSpan ts:
                cell.SetValue(ts);
                cell.Style.DateFormat.Format = "HH:mm:ss";
                return;

            // numeric types
            case byte or sbyte or short or ushort or int or uint or long or ulong:
                cell.SetValue(Convert.ToDouble(value));
                return;

            case float or double:
                cell.SetValue(Convert.ToDouble(value));
                return;

            case decimal dec:
                cell.SetValue(dec);
                return;

            case Guid g:
                cell.SetValue(g.ToString());
                return;

            default:
                // fallback: stringify
                cell.SetValue(value.ToString() ?? string.Empty);
                return;
        }
    }
}
