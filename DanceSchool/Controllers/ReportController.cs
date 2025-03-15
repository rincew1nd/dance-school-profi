using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DanceSchool.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DanceSchool.Controllers
{
    public class ReportController : Controller
    {
        private readonly DanceSchoolEntities _db;
        public ReportController(DanceSchoolEntities db)
        {
            _db = db;
        }

        public async Task<ActionResult> DownloadLessonReport()
        {
            var lessons = await _db.Lessons.Include(l => l.Registrations).ToListAsync();
            var pdfBytes = GeneratePdf(lessons);
            return File(pdfBytes, "application/pdf", $"LessonReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public byte[] GeneratePdf(List<Lesson> data)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var ms = new MemoryStream())
            {
                // Создание документа
                var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(doc, ms);

                doc.Open();
                var filepath = ControllerContext.HttpContext.Server.MapPath("..\\arial.ttf");
                BaseFont bf = BaseFont.CreateFont(filepath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                
                // Заголовок
                var titleFont = new Font(bf, 18, Font.NORMAL);
                var title = new Paragraph("Отчет по занятиям", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                doc.Add(title);

                // Таблица данных
                var table = new PdfPTable(5)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };

                // Заголовки таблицы
                var headerFont = new Font(bf, 12, Font.NORMAL);
                AddHeaderCell(table, "Дата", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Время", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Длительность", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Стиль", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Название", headerFont, BaseColor.GRAY);

                // Данные
                var cellFont = new Font(bf, 10, Font.NORMAL);
                foreach (var item in data)
                {
                    AddCell(table, item.Date.ToString("dd.MM.yyyy"), cellFont);
                    AddCell(table, item.Date.ToString("HH:mm:ss"), cellFont);
                    AddCell(table, item.Duration.ToString(), cellFont);
                    AddCell(table, item.Style.Name, cellFont);
                    AddCell(table, item.Name, cellFont);
                }

                doc.Add(table);

                // Подвал
                var footer = new Paragraph($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}",
                    new Font(bf, 8, Font.NORMAL))
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                doc.Add(footer);

                doc.Close();
                return ms.ToArray();
            }
        }

        private void AddHeaderCell(PdfPTable table, string text, Font font, BaseColor bgColor)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table.AddCell(cell);
        }

        private void AddCell(PdfPTable table, string text, Font font)
        {
            table.AddCell(new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            });
        }
    }
}