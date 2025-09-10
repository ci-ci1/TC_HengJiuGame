using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
  //  [LoginFitterController]
    public class JobManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();

        ReturnJsonListData listData = new ReturnJsonListData();

        ReturnJsonData jsonData = new ReturnJsonData();

        // GET: JobManage

        #region 职位管理

        //职位管理
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult GetList(int page, int limit, string name, string code)
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


            return Json(listData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改，添加
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public ActionResult Save(Job job)
        {
            try
            {
                //判断是添加还是修改
                if (job.ID != Guid.Empty)
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
            }
            catch (Exception ex)
            {
                jsonData.code = 1;
                jsonData.msg = "提交失败！" + ex.Message;
            }


            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public ActionResult Delete(Guid ID)
        {
            try
            {
                var job = db.Job.Find(ID);
                if (job == null)
                {
                    jsonData.code = 1;
                    jsonData.msg = "未查找到用户信息！";
                }
                else
                {
                    job.IsDel = true;
                    job.ModifyDate = DateTime.Now;
                    db.Entry(job).State = System.Data.Entity.EntityState.Modified;

                    if (db.SaveChanges() > 0)
                    {
                        jsonData.code = 0;
                        jsonData.msg = "删除成功！";
                    }
                    else
                    {
                        jsonData.code = 1;
                        jsonData.msg = "删除失败！";
                    }
                }
            }
            catch (Exception ex)
            {
                jsonData.code = 1;
                jsonData.msg = "删除失败！" + ex.Message;
            }
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion


       
    }
}