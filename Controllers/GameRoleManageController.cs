using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class GameRoleManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();

        ReturnJsonListData listData = new ReturnJsonListData();

        ReturnJsonData jsonData = new ReturnJsonData();

        // GET: GameRoleManage
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetList(int page, int limit,string name,string type)
        {
            var list = db.GameRole.ToList();

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.RoleName.Contains(name)).ToList();
            }
            if (!string.IsNullOrEmpty(type))
            {
                list = list.Where(a => a.RoleType.Contains(type)).ToList();
            }

            var offset = (page - 1) * limit;
            var newList = list.OrderByDescending(a => a.ShelfTime).Skip(offset).Take(limit).ToList();


            listData.code = 0;
            listData.msg = "";
            listData.count = list.Count;
            listData.data = newList;

            return Json(listData, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Delete(int ID)
        {
            try
            {
                var gameRole = db.GameRole.Find(ID);
                if (gameRole == null)
                {
                    jsonData.code = 1;
                    jsonData.msg = "未查找到用户信息！";
                }
                else
                {

                    db.GameRole.Remove(gameRole);

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


        public ActionResult Save(GameRole gameRole)
        {
            try
            {
                var list = db.GameRole.Find(gameRole.ID);

                //判断是添加还是修改
                if (list!=null  )
                {
                    list.GameID = gameRole.GameID;
                    list.RoleName = gameRole.RoleName;
                    list.Lines = gameRole.Lines;
                    list.RoleType = gameRole.RoleType;
                    list.Sex = gameRole.Sex;
                    list.Remark = gameRole.Remark;

                    db.Entry(list).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    gameRole.ShelfTime = DateTime.Now;
                    
                    db.GameRole.Add(gameRole);
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
    }
}