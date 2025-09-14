using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class RoleMenuManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();

        ReturnJsonListData listData = new ReturnJsonListData();

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
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult GetList(int page, int limit, string name, string code)
        {
            var query = from r in db.Role
                        join rm in db.RoleResourceModule on r.ID equals rm.RoleID into roleModules
                        select new
                        {
                            r.ID,
                            r.RoleName,
                            r.RoleCode,
                            Modules = from rm in roleModules
                                      join m in db.SystemResourceModule on rm.ResourceModuleId equals m.ID
                                      select m.ModuleName
                        };

            // 过滤条件
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.RoleName.Contains(name));
            }
            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(x => x.RoleCode.Contains(code));
            }

            // 总数
            var total = query.Count();

            // 分页 + 拼接 ModuleName
            var list = query.ToList() .Select(x => new
                {
                    x.ID,
                    x.RoleName,
                    x.RoleCode,
                    ModuleName = string.Join(",", x.Modules)  // 权限列表合并成一行
        })
                .ToList();

            // layui 格式
            listData.code = 0;
            listData.msg = "";
            listData.count = total;
            listData.data = list;

            return Json(listData, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改，添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Role model)
        {
            try
            {
                //判断是添加还是修改
                if (model.ID != Guid.Empty)
                {
                    var roleInfo = db.Role.Find(model.ID);
                    roleInfo.RoleName = model.RoleName;
                    roleInfo.RoleCode = model.RoleCode;
                    roleInfo.ModifyDate = DateTime.Now;
                    db.Entry(roleInfo).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    model.ID = Guid.NewGuid();
                    model.CreateDate = DateTime.Now;
                    model.ModifyDate = DateTime.Now;
                    db.Role.Add(model);
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
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult Delete(Guid ID)
        {
            try
            {
                var role = db.Role.Find(ID);
                if (role == null)
                {
                    jsonData.code = 1;
                    jsonData.msg = "未查找到用户信息！";
                }
                else
                {
                    db.Role.Remove(role);
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
        /// <summary>
        /// 设置权限视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Set(Guid roleId)
        {
            ViewBag.roleID = roleId;
            return View();
        }
        /// <summary>
        /// 保存权限
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePermission(Guid roleId,string ids)
        {
            if (roleId==null||roleId==Guid.Empty)
            {
                jsonData.code = 1;
                jsonData.msg = "请刷新页面，重新选择角色！";
            }
            if (!string.IsNullOrEmpty(ids))
            {
                List<RoleResourceModule> list = new List<RoleResourceModule>();
                var idList = ids.Split(',');
                foreach (var item in idList)
                {
                    RoleResourceModule entity = new RoleResourceModule();
                    entity.ID = Guid.NewGuid();
                    entity.RoleID = roleId;
                    entity.ResourceModuleId = Guid.Parse(item);
                    list.Add(entity);
                }
                db.RoleResourceModule.AddRange(list);
                if (db.SaveChanges() > 0)
                {
                    jsonData.code = 0;
                    jsonData.msg = "设置成功！";
                }
                else
                {
                    jsonData.code = 1;
                    jsonData.msg = "设置失败！";
                }
            }
            else
            {
                jsonData.code = 1;
                jsonData.msg = "请先选择菜单！";
            }
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    }
}