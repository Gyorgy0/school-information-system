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
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            string role = RoleManager.CheckRole(sessionId);
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
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            string role = RoleManager.CheckRole(sessionId);
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
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            string role = RoleManager.CheckRole(sessionId);
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
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            string role = RoleManager.CheckRole(sessionId);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }
            var html = System.IO.File.ReadAllText($"./assets/{role}/courses.html");
            return base.Content(html, "text/html");
        }

        [HttpGet]
        public ContentResult gradebook()
        {
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }

            string role = RoleManager.CheckRole(sessionId);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }

            var html = System.IO.File.ReadAllText($"./assets/{role}/gradebook.html");
            return base.Content(html, "text/html");
        }

        [HttpGet]
        public ContentResult gallery()
        {
            string? sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }

            string role = RoleManager.CheckRole(sessionId);
            if (role == "")
            {
                return base.Content("<script>window.location.href = '/';</script>", "text/html");
            }

            var html = System.IO.File.ReadAllText($"./assets/{role}/gallery.html");
            return base.Content(html, "text/html");
        }
    }
}
