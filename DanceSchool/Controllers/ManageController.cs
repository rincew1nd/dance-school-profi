using System;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using DanceSchool.Models;

namespace DanceSchool.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly DanceSchoolEntities _db;
        public ManageController(DanceSchoolEntities db)
        {
            _db = db;
        }
        
        public async Task<ActionResult> Index()
        {
            try
            {
                var userId = User.Identity.GetUserId<int>();

                var user = await _db.AspUsers.Include(u => u.Registrations).FirstAsync(x => x.Id == userId);
                var model = new IndexViewModel
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    IsAdmin = user.RoleId == 1,
                    Picture = user.Picture,
                    Registrations = user.Registrations
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка сервера - {ex.Message}");
            }
            
            return View(new IndexViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePicture(HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId<int>();
                var user = await _db.AspUsers.FindAsync(userId);
                
                if (imageFile.ContentType != "image/jpeg" && imageFile.ContentType != "image/png")
                {
                    ModelState.AddModelError("", "Допустимы только JPEG и PNG.");
                    return RedirectToAction("Index");
                }
                
                if (imageFile.ContentLength > 0)
                {
                    using (var reader = new BinaryReader(imageFile.InputStream))
                    {
                        user.Picture = reader.ReadBytes(imageFile.ContentLength);
                    }
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Пароли не совпадают");
                    return View(model);
                }
                
                var userId = User.Identity.GetUserId<int>();
                var user = await _db.AspUsers.FindAsync(userId);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }

                if (user.Password.TrimEnd() != model.OldPassword)
                {
                    ModelState.AddModelError(string.Empty, "Не совпадает старый пароль");
                }
                
                if (ModelState.IsValid)
                {
                    user.Password = model.NewPassword;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка сервера - {ex.Message}");
            }
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeRole()
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _db.AspUsers.FindAsync(userId);
            user.RoleId = user.RoleId == 1 ? 2 : 1;
            await _db.SaveChangesAsync();
            Request.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}