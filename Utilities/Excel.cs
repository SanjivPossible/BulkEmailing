using OfficeOpenXml;
using System.Data;
using System.IO;
using System.Linq;

namespace bEmailing
{
    public class Excel
    {
        public DataTable GetDataTableFromExcel(string fileName)
        {
            DataTable tbl = new DataTable();
            var existingFile = new FileInfo(fileName);
            using (var package = new ExcelPackage(existingFile))
            {

                ExcelWorkbook workBook = package.Workbook;
                if (workBook != null)
                {
                    if (workBook.Worksheets.Count > 0)
                    {
                        ExcelWorksheet ws = workBook.Worksheets.First();

                        bool hasHeader = true;
                        foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                        {
                            tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                        }
                        var startRow = hasHeader ? 2 : 1;
                        for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                        {
                            var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                            var row = tbl.NewRow();
                            foreach (var cell in wsRow)
                            {
                                row[cell.Start.Column - 1] = cell.Text;
                            }
                            tbl.Rows.Add(row);
                        }
                    }
                }

                return tbl;
            }
        }

        public void ExportDataTableToExcel(DataTable dtExport, FileInfo fileName)
        {
            using (ExcelPackage pck = new ExcelPackage(fileName))
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");
                ws.Cells["A1"].LoadFromDataTable(dtExport, true);
                ws.Cells.AutoFitColumns();
                pck.Save();
            }

        }

    }
}
