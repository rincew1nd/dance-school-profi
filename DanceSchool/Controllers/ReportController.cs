using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using DanceSchool.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DanceSchool.Controllers
{
    public class ReportController : Controller
    {
        private readonly DanceSchoolEntities db = new DanceSchoolEntities();

        public async Task<ActionResult> DownloadLessonReport()
        {
            var lessons = await db.Lessons.Include(l => l.Registrations).ToListAsync();
            var pdfBytes = GeneratePdf(lessons);
            return File(pdfBytes, "application/pdf", $"LessonReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public byte[] GeneratePdf(List<Lesson> data)
        {
            using (var ms = new MemoryStream())
            {
                // Создание документа
                var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(doc, ms);

                doc.Open();

                // Заголовок
                var titleFont = FontFactory.GetFont("Arial", 18, BaseColor.DARK_GRAY);
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
                var headerFont = FontFactory.GetFont("Arial", 12, BaseColor.WHITE);
                AddHeaderCell(table, "Дата", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Время", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Длительность", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Стиль", headerFont, BaseColor.GRAY);
                AddHeaderCell(table, "Название", headerFont, BaseColor.GRAY);

                // Данные
                var cellFont = FontFactory.GetFont("Arial", 10);
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
                    FontFactory.GetFont("Arial", 8, BaseColor.LIGHT_GRAY))
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

        /*
        private byte[] GeneratePdf(IEnumerable<Lesson> lessons)
        {
            var document = Document.Create(container =>
            {
                foreach (var lesson in lessons)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .AlignCenter()
                            .Text($"Отчет по занятию: {lesson.Name}")
                            .FontSize(20).Bold();

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Text($"Дата: {lesson.Date:dd.MM.yyyy}");
                                column.Item().Text($"Время: {lesson.Date:HH:mm:ss}");
                                column.Item().Text($"Длительность: {lesson.Duration.ToString()}");
                                column.Item().Text($"Стиль: {lesson.Style.Name}");

                                column.Item().PaddingTop(15).LineHorizontal(1);

                                column.Item().Text("Участники:").FontSize(14).Bold();
                                foreach (var reg in lesson.Registrations)
                                {
                                    column.Item().Text($"{reg.AspUser.FirstName} {reg.AspUser.LastName}");
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Страница ");
                                x.CurrentPageNumber();
                            });
                    });
                }
            });

            using (var stream = new MemoryStream())
            {
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
        }
        */


    }
}