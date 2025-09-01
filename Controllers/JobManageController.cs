using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class JobManageController : Controller
    {
        TC_HengJiuGame_DBEntities db = new TC_HengJiuGame_DBEntities();

        ReturnJsonListData listData = new ReturnJsonListData();

        // GET: JobManage
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetList(int page,int limit)
        {
            var list = db.Job.Where(a => a.IsDel == false).ToList();
            var offset = (page - 1) * limit;
            var newList = list.OrderBy(a => a.CreateDate).Skip(offset).Take(limit).ToList();

            listData.code = 0;
            listData.msg = "";
            listData.count = list.Count;
            listData.data = newList;


            return Json(listData,JsonRequestBehavior.AllowGet);
        }
    }
}