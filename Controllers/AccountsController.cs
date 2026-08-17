using OrientalApplication.Models;
using OrientalApplication.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OrientalApplication.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountsController() : this(new UserRepository())
        {
        }

        public AccountsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: Accounts
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserModel model)
        {
            bool IsValidUser = false;
            //string username = ConfigurationManager.AppSettings.Get("UserName");
            //string password = ConfigurationManager.AppSettings.Get("Password");

            if (_userRepository.ValidateUser(model.UserName,model.UserPassword))
            {
                IsValidUser = true;
            }
            if (IsValidUser)
            {
                FormsAuthentication.SetAuthCookie(model.UserName, false);
                if (model.UserName == "Engr")
                {
                    return RedirectToAction("Index", "PurchaseRequisition");
                }
                else if(model.UserName == "Accounts")
                {
                    return RedirectToAction("Index", "BillPayment");
                }
                else if(model.UserName == "Admin")
                {
                    return RedirectToAction("Index", "PurchaseRequisition");
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();

        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    
    }
}

