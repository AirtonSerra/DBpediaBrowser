using DBPediaNetwork.Biz;
using DBPediaNetwork.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using MySqlConnector;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DBPediaNetwork.Controllers
{
    public class AuthenticationController : Controller
    {


        public const string SESSION_KEY_USER = "user";
        public const string VIEWDATA_FIELD_ERROR = "errorMsg";

        private MySqlConnection db;

        public AuthenticationController(MySqlConnection _db)
        {
            db = _db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            if(!String.IsNullOrEmpty(user.email) && !String.IsNullOrEmpty(user.password))
            {
                //var tewr = ConfigurationManager.AppSettings["ConnectionStrings:Default"];
                AuthenticationBiz authenticationBiz = new AuthenticationBiz(db);
                User authenticationUser = authenticationBiz.GetUserData(user.email);

                if (authenticationUser != null && authenticationUser.password.Equals(user.password))
                {
                    HttpContext.Session.SetString(SESSION_KEY_USER, JsonConvert.SerializeObject(authenticationUser));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData[VIEWDATA_FIELD_ERROR] = "Falha no login! Usuário e/ou senha não conferem.";
                }
            }
            else
            {
                ViewData[VIEWDATA_FIELD_ERROR] = "O usuário e senha não podem ser vazios.";
            }
            
            return View("index");
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (!String.IsNullOrEmpty(user.email) && !String.IsNullOrEmpty(user.password))
            {
                //var tewr = ConfigurationManager.AppSettings["ConnectionStrings:Default"];
                AuthenticationBiz authenticationBiz = new AuthenticationBiz(db);
                User authenticationUser = authenticationBiz.RegisterUser(user);

                if (authenticationUser != null)
                {
                    HttpContext.Session.SetString(SESSION_KEY_USER, JsonConvert.SerializeObject(authenticationUser));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData[VIEWDATA_FIELD_ERROR] = "Falha no cadastrar usuário.";
                }
            }
            else
            {
                ViewData[VIEWDATA_FIELD_ERROR] = "O usuário e senha não podem ser vazios.";
            }

            return View();
        }



        public ActionResult Logout()
        {
            HttpContext.Session.Remove(SESSION_KEY_USER);
            return RedirectToAction("Index", "Home");
        }
    }
}
