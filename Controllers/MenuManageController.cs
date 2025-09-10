using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class MenuManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();
        ReturnJsonListData treeListDate = new ReturnJsonListData();
        ReturnJsonData jsonData = new ReturnJsonData();
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
        /// <returns></returns>
        public ActionResult GetList(int page, int limit, string name)
        {
            var newTreelist = new List<TreeListDate>();
            var list = db.SystemResourceModule.Where(a => a.ParentID == Guid.Empty).ToList();
            foreach (var item in list)
            {
                TreeListDate treeDate = new TreeListDate();

                treeDate.id = item.ID;
                treeDate.name = item.ModuleName;
                treeDate.code = item.ModuleCode;
                treeDate.type = item.Type;
                treeDate.url = item.Url;
                treeDate.parentId = item.ParentID;
                treeDate.IsParent = db.SystemResourceModule.Where(a => a.ParentID == item.ID).ToList().Count > 0 ? true : false;
                treeDate.children = BindTree(list, item.ID);//递归调用
                newTreelist.Add(treeDate);
            }
            treeListDate.code = 0;
            treeListDate.count = newTreelist.Count;
            treeListDate.data = newTreelist;

            return Json(treeListDate, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 递归访问
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<object> BindTree(List<SystemResourceModule> list, Guid ID)
        {
            List<object> treeList = new List<object>();

            var childList = db.SystemResourceModule.Where(a => a.ParentID == ID).ToList();
            foreach (var item in childList)
            {
                TreeListDate treeDate = new TreeListDate();
                treeDate.id = item.ID;
                treeDate.name = item.ModuleName;
                treeDate.code = item.ModuleCode;
                treeDate.type = item.Type;
                treeDate.url = item.Url;
                treeDate.parentId = item.ParentID;
                treeDate.IsParent = db.SystemResourceModule.Where(a => a.ParentID == item.ID).ToList().Count > 0 ? true : false;
                treeDate.children = BindTree(childList, item.ID);//递归调用
                treeList.Add(treeDate);
            }
            return treeList;
        }
        /// <summary>
        /// 返回数据实体类型
        /// </summary>
        public class TreeListDate
        {
            public Guid id { get; set; }
            public string name { get; set; }
            public string code { get; set; }
            public int? type { get; set; }
            public string url { get; set; }
            public Guid? parentId { get; set; }
            public IEnumerable<object> children { get; set; }
            public bool IsParent { get; set; }
        }
        /// <summary>
        /// 添加主菜单
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult AddPar(string ModuleCode, string ModuleName)
        {
            SystemResourceModule model = new SystemResourceModule();
            model.ID = Guid.NewGuid();
            model.ModuleCode = ModuleCode;
            model.ModuleName = ModuleName;
            model.Type = 0;
            model.ParentID = Guid.Empty;
            model.CreateDate = DateTime.Now;
            model.ModifyDate = DateTime.Now;
            db.SystemResourceModule.Add(model);
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
        /// <summary>
        /// 添加子菜单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(SystemResourceModule model)
        {
            model.ParentID = model.ID;
            model.ID = Guid.NewGuid();
            model.ModifyDate = DateTime.Now;
            model.CreateDate = DateTime.Now;
            db.SystemResourceModule.Add(model);
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
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Update(SystemResourceModule model)
        {
            var entity = db.SystemResourceModule.Find(model.ID);
            if (entity != null)
            {
                entity.ModuleName = model.ModuleName;
                entity.ModuleCode = model.ModuleCode;
                entity.Url = model.Url;
                entity.ModifyDate = DateTime.Now;

                db.Entry(entity).State = System.Data.Entity.EntityState.Modified;

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
            else
            {
                jsonData.code = 1;
                jsonData.msg = "数据已删除，请联系管理员，请刷新！";
            }
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 编辑子节点对父节点赋值
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GetInfo(Guid? ID)
        {
            if (ID == Guid.Empty || ID == null)
            {
                jsonData.code = 1;
                jsonData.msg = "未接收到参数，请联系管理员！";
            }
            else
            {
                var entity = db.SystemResourceModule.Find(ID);
                if (entity != null)
                {
                    jsonData.data = entity;
                }
                else
                {
                    jsonData.code = 1;
                    jsonData.msg = "未找到当前参数，请联系管理员！";
                }
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult Delete(Guid ID)
        {
            var model = db.SystemResourceModule.Find(ID);

            if (model != null)
            {
                db.SystemResourceModule.Remove(model);
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
            else
            {
                jsonData.code = 1;
                jsonData.msg = "用户已删除，请刷新！";
            }

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    
    }
}