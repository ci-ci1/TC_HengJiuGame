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

        ReturnJsonData jsonData = new ReturnJsonData();

        // GET: JobManage
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetList(int page,int limit, string name , string code )
        {
            var list = db.Job.Where(a => a.IsDel == false).ToList();
            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.JobName.Contains(name)).ToList();
            }
            if (!string.IsNullOrEmpty(code))
            {
                list = list.Where(a => a.JobCode.Contains(code)).ToList();
            }

            var offset = (page - 1) * limit;
            var newList = list.OrderByDescending(a => a.ModifyDate).Skip(offset).Take(limit).ToList();

            listData.code = 0;
            listData.msg = "";
            listData.count = list.Count;
            listData.data = newList;


            return Json(listData,JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Job job)
        {

            //判断是添加还是修改
            if (job.ID!=Guid.Empty)
            {
                var jobInfo = db.Job.Find(job.ID);

                // 修改字段
                jobInfo.JobName = job.JobName;
                jobInfo.JobCode = job.JobCode;
                jobInfo.ModifyDate = DateTime.Now;

                db.Entry(jobInfo).State = System.Data.Entity.EntityState.Modified;

            }
            else
            {
                job.ID = Guid.NewGuid();
                job.CreateDate = DateTime.Now;
                job.ModifyDate = DateTime.Now;
                job.IsDel = false;
                db.Job.Add(job);
            }
            if (db.SaveChanges() > 0)
            {
                jsonData.code = 0;
                jsonData.msg = "提交成功！";
            }
            else
            {
                jsonData.code = 1;
                jsonData.msg = "提交失败！";
            }



            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    }
}