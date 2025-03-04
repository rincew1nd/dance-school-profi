using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using DanceSchool.Models;
using Microsoft.AspNet.Identity;

namespace DanceSchool.Controllers
{
    public class LessonsController : Controller
    {
        private DanceSchoolEntities db = new DanceSchoolEntities();

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var lessons = await db.Lessons.Include(l => l.Style).OrderBy(x => x.Date).ToListAsync();
            return View(lessons.Select(x => x.ToViewModel() as LessonModel));
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var lesson = await db.Lessons
                .Include(l => l.Style)
                .Include(l => l.Registrations)
                .FirstOrDefaultAsync(l => l.Id == id);
            
            if (lesson == null)
            {
                return HttpNotFound();
            }

            return View(lesson.ToViewModel());
        }

        // GET: Lessons/Create
        public ActionResult Create()
        {
            ViewBag.StyleId = new SelectList(db.Styles, "Id", "Name");
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,StyleId,Date,Duration")] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                db.Lessons.Add(lesson);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StyleId = new SelectList(db.Styles, "Id", "Name", lesson.StyleId);
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            ViewBag.StyleId = new SelectList(db.Styles, "Id", "Name", lesson.StyleId);
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,StyleId,Date,Duration")] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { Id = lesson.Id });
            }
            ViewBag.StyleId = new SelectList(db.Styles, "Id", "Name", lesson.StyleId);
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson lesson = db.Lessons.Find(id);
            db.Lessons.Remove(lesson);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enroll(int lessonId)
        {
            var lesson = await db.Lessons
                .Include(l => l.Style)
                .Include(l => l.Registrations)
                .FirstAsync(l => l.Id == lessonId);
            
            try
            {
                var userId = User.Identity.GetUserId<int>();
                var user = await db.AspUsers.FindAsync(userId);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }

                var registration = new Registration()
                {
                    UserId = userId,
                    LessonId = lesson.Id,
                    CreateDate = DateTime.Now
                };
                db.Registrations.Add(registration);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при записи на занятие - {ex.Message}");
            }

            return RedirectToAction("Details", lesson.ToViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Withdraw(int lessonId, int registrationId)
        {
            var lesson = await db.Lessons
                .Include(l => l.Style)
                .Include(l => l.Registrations)
                .FirstAsync(l => l.Id == lessonId);
            
            try
            {
                var userId = User.Identity.GetUserId<int>();
                var user = await db.AspUsers.FindAsync(userId);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }

                var registration = await db.Registrations.FindAsync(registrationId);
                db.Registrations.Remove(registration);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при отмене записи на занятие - {ex.Message}");
            }
            
            return RedirectToAction("Details", lesson.ToViewModel());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
