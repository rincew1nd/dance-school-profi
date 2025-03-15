using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DanceSchool.Controllers;
using DanceSchool.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace DanceSchool.Tests.Controllers
{
    [TestClass]
    public class ReportControllerTests : BaseControllerTest
    {
        private ReportController Controller;
        private Mock<DbSet<Lesson>> LessonsMock;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            
            LessonsMock = new Mock<DbSet<Lesson>>();
            
            var data = new List<Lesson>
            {
                new Lesson
                {
                    Id = 1,
                    Date = new DateTime(2023, 10, 15, 18, 0, 0),
                    Duration = TimeSpan.FromMinutes(90),
                    Style = new Style { Name = "Сальса" },
                    Name = "Базовые шаги",
                    Registrations = new List<Registration>()
                },
                new Lesson
                {
                    Id = 2,
                    Date = new DateTime(2023, 10, 16, 19, 30, 0),
                    Duration = TimeSpan.FromMinutes(60),
                    Style = new Style { Name = "Танго" },
                    Name = "Продвинутая техника",
                    Registrations = new List<Registration>()
                }
            }.AsQueryable();

            LessonsMock.As<IQueryable<Lesson>>().Setup(m => m.Provider).Returns(data.Provider);
            LessonsMock.As<IQueryable<Lesson>>().Setup(m => m.Expression).Returns(data.Expression);
            LessonsMock.As<IQueryable<Lesson>>().Setup(m => m.ElementType).Returns(data.ElementType);
            LessonsMock.As<IQueryable<Lesson>>().Setup(m => m.GetEnumerator())
                .Returns(data.GetEnumerator());

            DbMock.Setup(db => db.Lessons).Returns(LessonsMock.Object);
            
            HttpServerUtilityBaseMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(Directory.GetCurrentDirectory() + "\\..\\..\\..\\DanceSchool\\arial.ttf");
            
            Controller = new ReportController(DbMock.Object)
            {
                ControllerContext = ControllerContextMock.Object,
            };
        }

        [TestMethod]
        public void GeneratePdf_WithValidData_CreatesCorrectDocument()
        {
            // Arrange
            var lessons = new List<Lesson>
            {
                new Lesson
                {
                    Date = new DateTime(2023, 10, 15),
                    Duration = TimeSpan.FromMinutes(90),
                    Style = new Style { Name = "Сальса" },
                    Name = "Базовые шаги"
                }
            };

            // Act
            var pdfBytes = Controller.GeneratePdf(lessons);
            var text = ExtractTextFromPdf(pdfBytes);

            // Assert
            Assert.IsTrue(text.Contains("Отчет по занятиям"));
            Assert.IsTrue(text.Contains("15.10.2023"));
            Assert.IsTrue(text.Contains("Сальса"));
            Assert.IsTrue(text.Contains("Базовые шаги"));
            Assert.IsTrue(text.Contains("Сгенерировано:"));
        }

        [TestMethod]
        public void GeneratePdf_WithEmptyData_CreatesDocument()
        {
            // Act
            var pdfBytes = Controller.GeneratePdf(new List<Lesson>());
            var text = ExtractTextFromPdf(pdfBytes);

            // Assert
            Assert.IsTrue(text.Contains("Отчет по занятиям"));
            Assert.IsFalse(text.Contains("15.10.2023"));
        }

        private string ExtractTextFromPdf(byte[] pdfBytes)
        {
            using (var reader = new PdfReader(pdfBytes))
            {
                var text = new System.Text.StringBuilder();
                
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                    text.Append(currentText);
                }
                
                return text.ToString();
            }
        }

        [TestMethod]
        public void GeneratePdf_TableStructure_IsCorrect()
        {
            // Arrange
            var lessons = new List<Lesson>
            {
                new Lesson
                {
                    Date = new DateTime(2023, 10, 15, 18, 0, 0),
                    Duration = TimeSpan.FromMinutes(90),
                    Style = new Style { Name = "Сальса" },
                    Name = "Базовые шаги"
                }
            };

            // Act
            var pdfBytes = Controller.GeneratePdf(lessons);
            var text = ExtractTextFromPdf(pdfBytes);

            // Assert
            var lines = text.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            Assert.AreEqual("Отчет по занятиям", lines[0]);
            Assert.AreEqual("Дата Время Длительность Стиль Название", lines[2]);
            Assert.AreEqual("15.10.2023 18:00:00 01:30:00 Сальса Базовые шаги", lines[3]);
        }
    }
}