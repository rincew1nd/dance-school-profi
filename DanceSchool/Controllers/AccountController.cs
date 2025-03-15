using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using DanceSchool.Models;

namespace DanceSchool.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly DanceSchoolEntities _db;
        public AccountController(DanceSchoolEntities db)
        {
            _db = db;
        }
        
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                if (Request.IsAuthenticated)
                {
                    return RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка сервера - {ex.Message}");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userInfo = _db.User_Login(model.Email, model.Password).FirstOrDefault();
                    if (userInfo != null)
                    {
                        SignInUser(userInfo.Id, $"{userInfo.FirstName} {userInfo.LastName}", userInfo.RoleId, false);
                        return RedirectToLocal(returnUrl);
                    }
                    ModelState.AddModelError(string.Empty, "Неправильный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка сервера - {ex.Message}");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var regInfo =
                        _db.Insert_User(model.Email, model.Password, model.FirstName, model.LastName, 2).First();
                    if (Convert.ToInt32(regInfo) != -1)
                    {
                        await _db.SaveChangesAsync();

                        return RedirectToAction("Login", "Account");
                    }

                    ModelState.AddModelError(string.Empty, "Пользователь с такой почтой уже существует.");
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
        public ActionResult LogOff()
        {
            try
            {
                Request.GetOwinContext().Authentication.SignOut();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка сервера - {ex.Message}");
            }
            return RedirectToAction("Login", "Account");
        }

        #region Private methods

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Manage");
        }

        private void SignInUser(int id, string name, int roleId, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, roleId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            
            Request.GetOwinContext().Authentication.SignIn(
                new AuthenticationProperties() { IsPersistent = isPersistent },
                claimsIdentity
            );
        }

        #endregion
    }
}