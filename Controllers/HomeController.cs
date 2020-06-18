using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WeddingPlanning.Models;

namespace WeddingPlanning.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [Route("")]
        [HttpGet]
        public IActionResult Registration()
        {
            return View("Registration");
        }

        [HttpPost("users/register")]
        public IActionResult Registering(LogRegWrapper user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Register.Email))
                {
                    ModelState.AddModelError("Register.Email", "Already Registered? Please Log In.");
                    return Registration();
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Register.Password = Hasher.HashPassword(user.Register, user.Register.Password);
                dbContext.Users.Add(user.Register);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId", user.Register.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return Registration();
            }
        }

        [HttpPost("users/login")]
        public IActionResult Logging(LogRegWrapper user)
        {
            if(ModelState.IsValid)
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == user.Login.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Login.Email", "Invalid Email/Password");
                    return Registration();
                }
                PasswordHasher<LoggedUser> Hasher = new PasswordHasher<LoggedUser>();
                PasswordVerificationResult Result = Hasher.VerifyHashedPassword(user.Login, userInDb.Password, user.Login.Password);
                if(Result == 0)
                {
                    ModelState.AddModelError("Login.Email", "Invalid Email/Password");
                    return Registration();
                }
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return Registration();
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            DashboardWrapper WMod = new DashboardWrapper()
            {
                AllWeddings = dbContext.Weddings
                    .Include(w => w.Planner)
                    .Include(w => w.GuestsAttending)
                    .ThenInclude(r => r.Guest)
                    .Where(w => w.Date > DateTime.Today)
                    .ToList(),
                LoggedUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)LoggedId)
            };

            return View("Dashboard", WMod);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            int? LoggedUser = HttpContext.Session.GetInt32("UserId");
            if(LoggedUser == null)
            {
                return RedirectToAction("Registration");
            }
            HttpContext.Session.Clear();
            return RedirectToAction("LoggedOut");
        }

        [HttpGet("loggedout")]
        public IActionResult LoggedOut()
        {
            return View("LoggedOut");
        }

        [Route("weddings/new")]
        [HttpGet]
        public IActionResult NewWedding()
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }
            return View("NewWedding");
        }

        [HttpPost("weddings/create")]
        public IActionResult CreateWedding(Wedding wedding)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if (LoggedId == null) 
            {
                return RedirectToAction("Registration");
            }
            // Linking user in session to object
            wedding.UserId = (int)LoggedId;

            if(ModelState.IsValid)
            {
                if(wedding.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "Choose a future date.");
                    return NewWedding();
                }
                
                dbContext.Add(wedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return NewWedding();
            }
        }

        [HttpGet("weddings/{WeddingId}")]
        public IActionResult WeddingDetail(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if (LoggedId == null)
            {
                return RedirectToAction("Registration");
            }
            Wedding ToPage = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .ThenInclude(r => r.Guest)
                .FirstOrDefault(w => w.WeddingId == WeddingId);
            
            if(ToPage == null)
            {
                return RedirectToAction("Dashboard");
            }

            return View("WeddingDetail", ToPage);
        }

        // Edit
        [HttpGet("weddings/{WeddingId}/edit")]
        public IActionResult EditWedding(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            Wedding ToEdit = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToEdit == null || ToEdit.UserId != (int)LoggedId)
            {
                return RedirectToAction("Dashboard");
            }

            return View("EditWedding", ToEdit);
        }

        // Update
        [HttpPost("weddings/{WeddingId}/update")]
        public IActionResult UpdateWedding(int WeddingId, Wedding FromForm)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            if(!dbContext.Weddings.Any(w => w.WeddingId == WeddingId && w.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            FromForm.UserId = (int)LoggedId;
            if(ModelState.IsValid)
            {
                FromForm.WeddingId = WeddingId;
                dbContext.Update(FromForm);
                dbContext.Entry(FromForm).Property("CreatedAt").IsModified = false;
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return EditWedding(WeddingId);
            }
        }

        // RSVP
        [HttpGet("weddings/{WeddingId}/rsvp")]
        public RedirectToActionResult RSVP(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            Wedding ToJoin = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .FirstOrDefault(w => w.WeddingId == WeddingId);
            if(ToJoin == null || ToJoin.UserId == (int)LoggedId || ToJoin.GuestsAttending.Any(r => r.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                RSVP NewRsvp = new RSVP()
                {
                    UserId = (int)LoggedId,
                    WeddingId = WeddingId
                };
                dbContext.Add(NewRsvp);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }

        // unRSVP
        [HttpGet("weddings/{WeddingId}/unrsvp")]
        public RedirectToActionResult unRSVP(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            Wedding ToLeave = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToLeave == null || !ToLeave.GuestsAttending.Any(r => r.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                RSVP ToRemove = dbContext.RSVPS.FirstOrDefault(r => 
                r.UserId == (int)LoggedId && r.WeddingId == WeddingId);
                dbContext.Remove(ToRemove);
                dbContext.SaveChanges();

                return RedirectToAction("Dashboard");
            }
        }

        // Delete
        [HttpGet("weddings/{WeddingId}/delete")]
        public RedirectToActionResult DeleteWedding(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("Registration");
            }

            Wedding ToDelete = dbContext.Weddings
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToDelete == null || ToDelete.UserId != (int)LoggedId)
            {
                return RedirectToAction("Dashboard");
            }

            dbContext.Remove(ToDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
    
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
