using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{


    /// <summary>
    /// 登陆，注册
    /// </summary>
    public class LoginController : Controller
    {
        #region 公共基层方法
        /// <summary>
        /// 数据库连接
        /// </summary>
        TC_HengJiuGame_DBEntities db = new TC_HengJiuGame_DBEntities();
        /// <summary>
        /// 返回参数
        /// </summary>
        ReturnJsonData returnJsonData = new ReturnJsonData();
        #endregion

        #region 视图
        // GET: 登陆视图
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 注册视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Registered()
        {
            return View();
        }
        #endregion

        #region 方法

        #region 登陆验证
        public ActionResult VeryifyLogin(T_Users model)
        {
            try
            {
                var entity = db.T_Users.Where(a => a.UserCode == model.UserCode && a.PassWord == model.PassWord).FirstOrDefault();
                if (entity != null)
                {
                    Session["T_Users"] = entity;

                    returnJsonData.code = 0;
                    returnJsonData.msg = "登陆成功！";
                }
                else
                {
                    returnJsonData.code = 1;
                    returnJsonData.msg = "登陆失败！请输入正确的用户名或密码";
                }
            }
            catch (Exception ex)
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "登陆失败！" + ex.Message;
            }
            return Json(returnJsonData, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 注册
        public ActionResult VeryifyAdd(T_Users model)
        {

            try
            {
                model.ID = Guid.NewGuid();
                model.ModifyDate = DateTime.Now;
                model.CreateDate = DateTime.Now;
                model.Status = false;
                db.T_Users.Add(model);
                if (db.SaveChanges() > 0)
                {
                    returnJsonData.code = 0;
                    returnJsonData.msg = "注册成功！";
                }
                else
                {
                    returnJsonData.code = 1;
                    returnJsonData.msg = "注册失败！";
                }
            }
            catch (Exception ex)
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "注册失败！" + ex.Message;
            }
            return Json(returnJsonData, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #endregion







    }
}