using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class PageController : Controller
    {
        [HttpGet]
        public ContentResult main()
        {
            string role = RoleManager.CheckRole(Request.Cookies["id"]);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            var html = System.IO.File.ReadAllText($"./assets/{role}/mainpage.html");
            return base.Content(html, "text/html");
        }
        [HttpGet]
        public ContentResult timetable()
        {
            string role = RoleManager.CheckRole(Request.Cookies["id"]);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            var html = System.IO.File.ReadAllText($"./assets/{role}/timetable.html");
            return base.Content(html, "text/html");
        }
        [HttpGet]
        public ContentResult lunchmenupage()
        {
            string role = RoleManager.CheckRole(Request.Cookies["id"]);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            var html = System.IO.File.ReadAllText($"./assets/{role}/lunchmenupage.html");
            return base.Content(html, "text/html");
        }
        [HttpGet]
        public ContentResult courses()
        {
            string role = RoleManager.CheckRole(Request.Cookies["id"]);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            var html = System.IO.File.ReadAllText($"./assets/{role}/courses.html");
            return base.Content(html, "text/html");
        }
    }
}