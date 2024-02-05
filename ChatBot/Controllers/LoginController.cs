using ChatBot.Models;
using ChatBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatBot.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index() => View();

        [HttpPost]
        public ActionResult Index(LoginData loginData)
        {
            LoginRepository repo = new LoginRepository();

            try
            {
                if (repo.Login(loginData.UserId))
                {
                    Session["user_id"] = loginData.UserId;
                    repo.MakeLoginLog(loginData.UserId, "success");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "로그인 실패하였습니다. 다시 시도하시기 바랍니다.";
                    repo.MakeLoginLog(loginData.UserId, "fail", ViewBag.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                repo.MakeLoginLog(loginData.UserId, "fail", ex.Message.ToString()); 
            }

            return View();
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));

            return RedirectToAction("Index", "Login");
        }
    }
}
