using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Data.Export;

public class ExcelReportService
{
    private const string TemplateFileName = "Template_Report.xlsx";

    public void EnsureTemplateExists()
    {
        // For multi-sheet logic, we might not need a static template file anymore, 
        // as we create sheets dynamically based on data. 
        // But let's just create a basic one so it doesn't fail.
        if (!File.Exists(TemplateFileName))
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Template");
            ws.Cell("A1").Value = "TAPE ADHESION TEST REPORT";
            workbook.SaveAs(TemplateFileName);
        }
    }

    public bool ExportReport(IEnumerable<TestRecord> records, out string savedFilePath)
    {
        savedFilePath = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        try
        {
            using var workbook = new XLWorkbook();
            
            // Group records by RackId
            var groupedRecords = System.Linq.Enumerable.GroupBy(records, r => string.IsNullOrEmpty(r.RackId) ? "Unknown" : r.RackId);

            foreach (var group in groupedRecords)
            {
                var wsName = group.Key.Length > 31 ? group.Key.Substring(0, 31) : group.Key;
                var ws = workbook.Worksheets.Add(wsName);

                ws.Cell("A1").Value = $"TAPE ADHESION TEST REPORT - {group.Key}";
                
                ws.Cell("A3").Value = "Date / Time";
                ws.Cell("B3").Value = "Rack";
                ws.Cell("C3").Value = "Floor";
                ws.Cell("D3").Value = "Hook";
                ws.Cell("E3").Value = "Location";
                ws.Cell("F3").Value = "Vị trí mẫu";
                ws.Cell("G3").Value = "Tester";
                ws.Cell("H3").Value = "Batch Code";
                ws.Cell("I3").Value = "Nart Code";
                ws.Cell("J3").Value = "Thời gian (Phút)";
                ws.Cell("K3").Value = "PLC Value (Raw)";

                int currentRow = 4;
                foreach (var record in group)
                {
                    ws.Cell(currentRow, 1).Value = record.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cell(currentRow, 2).Value = record.RackId;
                    ws.Cell(currentRow, 3).Value = record.Floor;
                    ws.Cell(currentRow, 4).Value = record.HookIndex;
                    ws.Cell(currentRow, 5).Value = record.Location;
                    ws.Cell(currentRow, 6).Value = record.SamplePosition;
                    ws.Cell(currentRow, 7).Value = record.Tester;
                    ws.Cell(currentRow, 8).Value = record.BatchCode;
                    ws.Cell(currentRow, 9).Value = record.NartCode;
                    ws.Cell(currentRow, 10).Value = Math.Round(record.DropTime / 600.0, 2); // PLC timer là 100ms -> 1 phút = 600 đơn vị
                    ws.Cell(currentRow, 11).Value = record.PlcValue;
                    currentRow++;
                }

                ws.Columns().AdjustToContents();
            }

            if (workbook.Worksheets.Count == 0)
            {
                workbook.Worksheets.Add("Empty Report");
            }

            workbook.SaveAs(savedFilePath);
            return true;
        }
        catch (IOException ex)
        {
            TapeAdhesionApp.Core.Utils.SimpleLogger.LogError($"[Excel Export Error] Lỗi file đang mở hoặc cấp quyền: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            TapeAdhesionApp.Core.Utils.SimpleLogger.LogError($"[Excel Export Error] Lỗi không xác định: {ex.Message}");
            return false;
        }
    }
}
