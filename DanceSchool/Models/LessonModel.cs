using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DanceSchool.Models
{
    public class LessonModel
    {
        [Display(Name = "ID")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        public string Name { get; set; }
        
        [Display(Name = "Стиль")]
        public string Style { get; set; }
        
        [Display(Name = "Дата")]
        public string Date { get; set; }
        
        [Display(Name = "Время")]
        public string Time { get; set; }
        
        [Display(Name = "Длительность")]
        public System.TimeSpan Duration { get; set; }
    }
    
    public class LessonDetailsModel : LessonModel
    {
        public IEnumerable<Registration> Registrations { get; set; }
    }

    public static class LessonExtensions
    {
        public static LessonDetailsModel ToViewModel(this Lesson lesson)
        {
            return new LessonDetailsModel()
            {
                Id = lesson.Id,
                Name = lesson.Name,
                Style = lesson.Style.Name,
                Date = lesson.Date.ToString("dd-MM-yyyy"),
                Time = lesson.Date.ToString("HH:mm:ss"),
                Duration = lesson.Duration,
                Registrations = lesson.Registrations
            };
        }
    }
}