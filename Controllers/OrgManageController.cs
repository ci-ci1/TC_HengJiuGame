using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class OrgManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();
        ReturnJsonListData treeListDate = new ReturnJsonListData();
        ReturnJsonData jsonData = new ReturnJsonData();


        // GET: OrgManage
        public ActionResult Index()
        {
            return View();
        }
        //查询
        public ActionResult GetList(int page, int limit, string name, string code)
        {
            var newTreelist = new List<TreeListDate>();

            var list = db.OrganizationStructure.Where(a => a.ParentID == Guid.Empty).ToList();
            foreach (var item in list)
            {
                TreeListDate treeDate = new TreeListDate();

                treeDate.id = item.ID;
                treeDate.name = item.OrgName;
                treeDate.code = item.OrgCode;
                treeDate.leve = item.Leve;
                treeDate.parentId = item.ParentID;
                treeDate.IsParent = db.OrganizationStructure.Where(a => a.ParentID == item.ID).ToList().Count > 0 ? true : false;
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
        public List<object> BindTree(List<OrganizationStructure> list,Guid ID)
        {
            List<object> treeList = new List<object>();

            var childList = db.OrganizationStructure.Where(a => a.ParentID == ID).ToList();
            foreach (var item in childList)
            {
                TreeListDate treeDate = new TreeListDate();

                treeDate.id = item.ID;
                treeDate.name = item.OrgName;
                treeDate.code = item.OrgCode;
                treeDate.leve = item.Leve;
                treeDate.parentId = item.ParentID;
                treeDate.IsParent = db.OrganizationStructure.Where(a => a.ParentID == item.ID).ToList().Count > 0 ? true : false;
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
            public int? leve { get; set; }
            public Guid? parentId { get; set; }
            public IEnumerable<object> children { get; set; }
            public bool IsParent { get; set; }
        }
        [HttpPost]
        /// <summary>
        /// 添加父节点
        /// </summary>
        /// <param name="org">实体模型</param>
        /// <returns></returns>
        public ActionResult AddPar(OrganizationStructure org)
        {
            org.ID = Guid.NewGuid();
            org.Leve = 0;
            org.ParentID = Guid.Empty;
            org.CreateDate = DateTime.Now;
            org.ModifyDate = DateTime.Now;
            db.OrganizationStructure.Add(org);
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

        //添加子节点
        [HttpPost]
        public ActionResult Save(OrganizationStructure org)
        {

                
                org.ParentID = org.ID;
                org.ID = Guid.NewGuid();
                org.ModifyDate = DateTime.Now;
                org.CreateDate = DateTime.Now;

            db.OrganizationStructure.Add(org);

            
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

        //修改
        public ActionResult Update(OrganizationStructure model)
        {
            var entity = db.OrganizationStructure.Find(model.ID);
            if (entity != null)
            {
                entity.OrgName = model.OrgName;
                entity.OrgCode = model.OrgCode;
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

        public ActionResult GetInfo(Guid? ID)
        {
            if (ID==Guid.Empty||ID==null)
            {
                jsonData.code = 1;
                jsonData.msg = "未接收到参数，请联系管理员！";
            }
            else
            {
                var entity = db.OrganizationStructure.Find(ID);
                if (entity!=null)
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

        //删除
        public ActionResult Delete(Guid ID)
        {
            var model = db.OrganizationStructure.Find(ID);

            if (model != null)
            {
                db.OrganizationStructure.Remove(model);
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