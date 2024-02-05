using ChatBot.Models;
using ChatBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatBot.Controllers
{
    public class HomeController : Controller
    {
        readonly ChatRepository repo = new ChatRepository();

        public ActionResult Index()
        {
            string UserId = Convert.ToString(HttpContext.Session["user_id"]);

            if (UserId == "") return RedirectToAction("Index", "Login");

            UserData userData = new UserData();
            userData.UserId = UserId;

            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            return View(userData);
        }

        public ActionResult Detail(string id)
        {
            string UserId = Convert.ToString(HttpContext.Session["user_id"]);

            if (UserId == "") return RedirectToAction("Index", "Login");

            UserData userData = new UserData();
            userData.UserId = UserId;
            userData.RoomId = id;

            if (repo.ListCheck(UserId, id))
            {
                return View(userData);
            }
            else
            {
                TempData["ErrorMessage"] = "해당 대화 내역을 찾을 수 없습니다.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
