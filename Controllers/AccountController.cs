using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using User_Management.Models;
using User_Management.ViewModels;


namespace User_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public AccountController()
        {
            dbContext = new ApplicationDbContext(); // Initialize your DbContext
        }

        // GET: Action
        public ActionResult Index()
        {
            var users = dbContext.Users.ToList();
            if(users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Users.Include("PasswordDetails")
                        .FirstOrDefault(u => u.EmailAddress == model.EmailAddress);

                    // Check if user exists and validate password
                    if (user != null)
                    {
                        // For security reasons, compare hashed password instead of plain text
                        if (user.PasswordDetails.Password == model.Password) // Ideally, use a hashing function
                        {
                            FormsAuthentication.SetAuthCookie(user.EmailAddress, false); // Auth cookie
                            return RedirectToAction("Index", "Account");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid credentials."); // Add error if password is wrong
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid credentials."); // Add error if user not found
                    }
                }
            }

            // If we got this far, something failed; redisplay form with errors
            return View(model);
        }



        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = new UserDetails
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DOB = model.DateOfBirth,
                        Gender = model.Gender,
                        EmailAddress = model.Email
                    };

                    db.Users.Add(user);
                    db.SaveChanges();
                    var activationLink = Url.Action("CreatePassword", "Account", new { email = model.Email }, protocol: Request.Url.Scheme);
                    SendEmail(model.Email, activationLink);

                    return View("Register");
                }
            }

            return View(model);
        }

        private void SendEmail(string email, string activationLink)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.Subject = "Create Password";
                mail.Body = $"Please click this link to create Password: <a href='{activationLink}'>Create Password</a>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("shobhitunion20@gmail.com", "mtfcmhkkmlskgvhg")
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                // Log the exception or display it for debugging
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }



        // GET: Account/CreatePassword
        public ActionResult CreatePassword(string email)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.EmailAddress == email);
                if (user != null)
                {
                    return View(new CreatePasswordViewModel { Email = email });
                }
                return HttpNotFound();
            }
        }

        // POST: Account/CreatePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePassword(CreatePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user based on the email
                var user = dbContext.Users.FirstOrDefault(u => u.EmailAddress == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User does not exist.");
                    return View(model);
                }

                // Check if the password entry already exists
                var passwordDetail = dbContext.Passwords.FirstOrDefault(p => p.UserID == user.UserID);
                if (passwordDetail == null)
                {
                    // Create a new PasswordDetails entry
                    passwordDetail = new PasswordDetails
                    {
                        UserID = user.UserID,
                        Password = model.Password // Remember to hash this password before saving
                    };
                    dbContext.Passwords.Add(passwordDetail);
                }
                else
                {
                    // Update the existing PasswordDetails entry
                    passwordDetail.Password = model.Password; // Remember to hash this password before saving
                    dbContext.Entry(passwordDetail).State = EntityState.Modified;
                }

                // Save changes to the database
                dbContext.SaveChanges();

                // Redirect to login
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }


        // GET: Account/RetrievePassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/RetrievePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email exists in the database
                var user = dbContext.Users.FirstOrDefault(u => u.EmailAddress == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "This email address does not match any records.");
                    return View(model);
                }

                var activationLink = Url.Action("CreatePassword", "Account", new { email = model.Email }, protocol: Request.Url.Scheme);
                SendEmail(model.Email , activationLink);

                ViewBag.Message = "A reset password link has been sent to your email.";
                return View("ForgotPassword");
            }

            return View(model);
        }

        public ActionResult Details(int id)
        {
            var user = dbContext.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Return a partial view with user details
            return PartialView("_UserDetails", user);
        }


        public ActionResult Edit(int id)
        {
            var user = dbContext.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DOB = user.DOB.ToString("yyyy-MM-dd"), 
                Gender = user.Gender,
                EmailAddress = user.EmailAddress
            }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult Edit([Bind(Include = "UserID,FirstName,LastName,DOB,Gender,EmailAddress")] UserDetails user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = dbContext.Users.Find(user.UserID);
                if (existingUser != null)
                {
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.DOB = user.DOB;
                    existingUser.Gender = user.Gender;
                    existingUser.EmailAddress = user.EmailAddress;

                    dbContext.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "User not found." });
            }
            return Json(new { success = false, message = "Invalid data." });
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }


    }
}
