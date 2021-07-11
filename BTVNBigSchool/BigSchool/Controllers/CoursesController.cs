using BigSchool.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BigSchool.Controllers
{
    public class CoursesController : Controller
    {
        // GET: Courses
        public ActionResult Create()
        {
            //get list category
            BigSchoolContext context = new BigSchoolContext();
            Course objCourse = new Course();
            objCourse.ListCategory = context.Categories.ToList();
            return View(objCourse);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course objCourse)
        {
            
            BigSchoolContext context = new BigSchoolContext();
            //Không xét valid LecturedId vì bằng user đăng nhập
            ModelState.Remove("LecturerId");
            if(!ModelState.IsValid)
            {
                objCourse.ListCategory = context.Categories.ToList();
                return View("Create", objCourse);
            }
            //Lấy login user ID
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            objCourse.LecturerId = user.Id;
            objCourse.Status = 1;
            //add vào CSDL
            context.Courses.Add(objCourse);
            context.SaveChanges();
            
            //Trở về Home, Action Index
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Attending()
        {
            BigSchoolContext context = new BigSchoolContext();
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
                .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            var listAttendances = context.Attendances.Where(p => p.Attendee == currentUser.Id && p.Course.Status == 1).ToList();
            var courses = new List<Course>();
            foreach(Attendance temp in listAttendances)
            {
                Course objCourse = temp.Course;
                objCourse.LectureName = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
                    .FindById(objCourse.LecturerId).Name;
                courses.Add(objCourse);
            }
            return View(courses);
        }

        public ActionResult Mine()
        {
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
                .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            BigSchoolContext context = new BigSchoolContext();
            var courses = context.Courses.Where(c => c.LecturerId == currentUser.Id && c.Datetime > DateTime.Now).ToList();
            foreach(Course i in courses)
            {
                i.LectureName = currentUser.Name;
            }
            return View(courses);
        }
        public ActionResult Edit(int id)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course course = context.Courses.SingleOrDefault(p => p.Id == id);
            course.ListCategory = context.Categories.ToList();
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }
        [HttpPost]
        [Authorize]
        public ActionResult Edit(Course course)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course courseUpdate = context.Courses.SingleOrDefault(p => p.Id == course.Id);
            if (courseUpdate != null)
            {
                context.Courses.AddOrUpdate(course);
                context.SaveChanges();
            }
            return RedirectToAction("Mine");
        }
        [Authorize]
        public ActionResult Delete(int id)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course course = context.Courses.SingleOrDefault(p => p.Id == id);
            if (course == null)
            {
                return HttpNotFound();
            }
            course.Status = 0;
            context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteMine(int id)
        {
            BigSchoolContext context = new BigSchoolContext();
            Course course = context.Courses.SingleOrDefault(p => p.Id == id);
            if (course != null)
            {
                context.Courses.Remove(course);
                context.SaveChanges();
            }

            return RedirectToAction("Mine");
        }
    }
}