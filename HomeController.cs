using E_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace E_Project.Controllers
{
    public class HomeController : Controller
    {
        DBQUIZEntities db = new DBQUIZEntities();
        [HttpGet]
        public ActionResult sregister()
        {


            return View();
        }

        [HttpPost]
        public ActionResult sregister(TBL_STUDENT svw, HttpPostedFileBase imgfile)
        {
            TBL_STUDENT s = new TBL_STUDENT();
            try
            {
                s.S_NAME = svw.S_NAME;
                s.S_PASSWORD = svw.S_PASSWORD;
                s.S_IMAGE = uploadimgfile(imgfile);
                db.TBL_STUDENT.Add(s);
                db.SaveChanges();
                return RedirectToAction("slogin");
            }
            catch (Exception)
            {

                ViewBag.msg = "Data Could not be inserted......";
            }

            return View();
        }

        public string uploadimgfile(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/img"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/img/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }
            }

            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";
            }



            return path;
        }


        [HttpGet]
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index");


        }
        public ActionResult tlogin()
        {


            return View();
        }



        [HttpPost]
        public ActionResult tlogin(TBL_ADMIN a)
        {
            TBL_ADMIN ad = db.TBL_ADMIN.Where(x => x.AD_NAME == a.AD_NAME && x.AD_PASSWORD == a.AD_PASSWORD).SingleOrDefault();
            if (ad != null)
            {
                Session["ad_id"] = ad.AD_ID;
                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.msg = "Invalid usename or password";
            }

            return View();
        }
        public ActionResult slogin()
        {
            return View();
        }


        [HttpPost]
        public ActionResult slogin(TBL_STUDENT s)
        {

            TBL_STUDENT std = db.TBL_STUDENT.Where(x => x.S_NAME == s.S_NAME && x.S_PASSWORD == s.S_PASSWORD).SingleOrDefault();
            if (std == null)
            {
                ViewBag.msg = "Invalid Email or Password";
            }
            else
            {
                Session["std_id"] = std.S_ID;
                return RedirectToAction("StudentExam");
            }
            return View();
        }

        public ActionResult StudentExam()
        {

            if (Session["std_id"] == null)
            {
                return RedirectToAction("slogin");
            }

            return View();
        }

        [HttpPost]
        public ActionResult StudentExam(string room)
        {
            List<tbl_category> list = db.tbl_category.ToList();

            foreach (var item in list)
            {
                if (item.cat_encyptedstring == room)
                {
                    List<TBL_QUESTIONS> li = db.TBL_QUESTIONS.Where(x => x.q_fk_catid == item.cat_id).ToList();
                    Queue<TBL_QUESTIONS> queue = new Queue<TBL_QUESTIONS>();
                    foreach (TBL_QUESTIONS a in li)
                    {
                        queue.Enqueue(a);
                    }
                    TempData["examid"] = item.cat_id;
                    TempData["questions"] = queue;
                    TempData["score"] = 0;
                    TempData.Keep();
                    return RedirectToAction("QuizStart");

                }
                else
                {
                    ViewBag.error = "No Room Found......";
                }
            }


            return View();
        }

        public ActionResult QuizStart()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("slogin");
            }

            TBL_QUESTIONS q = null;
            if (TempData["questions"] != null)
            {
                Queue<TBL_QUESTIONS> qlist = (Queue<TBL_QUESTIONS>)TempData["questions"];
                if (qlist.Count > 0)
                {
                    q = qlist.Peek();
                    qlist.Dequeue();
                    TempData["questions"] = qlist;
                    TempData.Keep();
                }
            }
            else
            {
                return RedirectToAction("EndExam");
            }
            return View(db.TBL_QUESTIONS.ToList());
        }

        [HttpPost]
        public ActionResult QuizStart(TBL_QUESTIONS q)
        {
            string correctans = "";
            if (q.OPA != null)
            {
                correctans = "A";
            }
            else if (q.OPB != null)
            {
                correctans = "B";
            }
            else if (q.OPC != null)
            {
                correctans = "C";
            }
            else if (q.OPD != null)
            {
                correctans = "D";
            }

            if (correctans.Equals(q.COP))
            {
                TempData["score"] = Convert.ToInt32(TempData["score"]) + 1;
            }

            TempData.Keep();
            return RedirectToAction("QuizStart");
        }

        public ActionResult EndExam(TBL_QUESTIONS q)
        {

           
            return View();
        }


        public ActionResult Dashboard()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Addcategory()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Index");
            }
            int adid = Convert.ToInt32(Session["ad_id"].ToString());
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_fk_adid == adid).OrderByDescending(x => x.cat_id).ToList();
            ViewData["list"] = li;

            return View();
        }


        [HttpPost]
        public ActionResult Addcategory(tbl_category cat)
        {

            List<tbl_category> li = db.tbl_category.OrderByDescending(x => x.cat_id).ToList();
            ViewData["list"] = li;

            Random r = new Random();
            tbl_category c = new tbl_category();
            c.cat_name = cat.cat_name;
            c.cat_encyptedstring = cyptop.Encrypt(cat.cat_name.Trim() + r.Next().ToString(), true);
            c.cat_fk_adid = Convert.ToInt32(Session["ad_id"].ToString());
            db.tbl_category.Add(c);
            db.SaveChanges();




            return RedirectToAction("Addcategory");
        }


        [HttpGet]

        public ActionResult Addquestion()
        {
            int sid = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_fk_adid == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");

            return View();
        }


        [HttpPost]

        public ActionResult Addquestion(TBL_QUESTIONS q)
        {
            int sid = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_fk_adid == sid).ToList();
            ViewBag.list = new SelectList(li, "cat_id", "cat_name");

            TBL_QUESTIONS QA = new TBL_QUESTIONS();
            QA.Q_TEXT = q.Q_TEXT;
            QA.OPA = q.OPA;
            QA.OPB = q.OPB;
            QA.OPC = q.OPC;
            QA.OPD = q.OPD;
            QA.COP = q.COP;
            QA.q_fk_catid = q.q_fk_catid;
            db.TBL_QUESTIONS.Add(QA);
            db.SaveChanges();
            TempData["msg"] = "Question added successfully....";
            TempData.Keep();
            return RedirectToAction("Addquestion");


        }
        public ActionResult Index()
        {
            if (Session["ad_id"] != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
