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
        // For development/demo purposes, if template doesn't exist, we create a dummy one.
        // In production, the user will place the actual Template_Report.xlsx here.
        if (!File.Exists(TemplateFileName))
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Report");
            ws.Cell("A2").Value = "BATCH CODE:";
            ws.Cell("C2").Value = "NART CODE:";
            
            ws.Cell("A4").Value = "Date / Time";
            ws.Cell("B4").Value = "Drop Time (ms)";
            
            workbook.SaveAs(TemplateFileName);
        }
    }

    public bool ExportReport(string batchCode, string nartCode, IEnumerable<TestRecord> records, out string savedFilePath)
    {
        savedFilePath = $"Report_{batchCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        try
        {
            EnsureTemplateExists();

            // Mở file template thay vì tạo mới
            using var workbook = new XLWorkbook(TemplateFileName);
            var ws = workbook.Worksheet(1);

            // Ghi Header
            ws.Cell("B2").Value = batchCode;
            ws.Cell("D2").Value = nartCode;

            // Đổ dữ liệu bắt đầu từ dòng 5
            int currentRow = 5;
            foreach (var record in records)
            {
                ws.Cell(currentRow, 1).Value = record.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cell(currentRow, 2).Value = record.DropTime;
                currentRow++;
            }

            // Căn chỉnh tự động
            ws.Columns().AdjustToContents();

            workbook.SaveAs(savedFilePath);
            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Excel Export Error] Lỗi file đang mở hoặc cấp quyền: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Excel Export Error] Lỗi không xác định: {ex.Message}");
            return false;
        }
    }
}
