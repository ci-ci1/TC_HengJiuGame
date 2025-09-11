using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;


namespace TC_HengJiuGame.Controllers
{
    [LoginFitterController]
    public class HomeController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();

        /// <summary>
        /// 返回实例
        /// </summary>
        ReturnJsonData returnJsonData = new ReturnJsonData();

        #region 视图
        /// <summary>
        /// 首页视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (Session["Users"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            //返回页面赋值
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }

        /// <summary>
        /// 修改信息视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            //返回页面赋值
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }
        /// <summary>
        /// 修改密码视图
        /// </summary>
        /// <returns></returns>
        public ActionResult EditPassWord()
        {
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }
        #endregion
        /// <summary>
        /// 修改功能实现
        /// </summary>
        /// <param name="t_Users"></param>
        /// <returns></returns>
        public ActionResult Revise(Users t_Users)
        {
            if (t_Users.ID==Guid.Empty)
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "未查找到用户信息！";
            }
            else
            {
                var user = db.Users.Find(t_Users.ID);
                if (user!=null)
                {
                    user.CreateDate = DateTime.Now;

                    user.UserCode = t_Users.UserCode;
                    user.UserName = t_Users.UserName;
                    user.Sex = t_Users.Sex;
                    user.BirthDay = t_Users.BirthDay;
                    user.NativePlace = t_Users.NativePlace;
                    user.Address = t_Users.Address;
                    user.Email = t_Users.Email;
                    user.Tel = t_Users.Tel;

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    if (db.SaveChanges() > 0)
                    {
                        returnJsonData.code = 0;
                        //更新缓存
                        Session["Users"] = user;
                        returnJsonData.msg = "修改成功！";
                    }
                    else
                    {
                        returnJsonData.code = 1;
                        returnJsonData.msg = "修改失败！";
                    }
                }
                else
                {
                    returnJsonData.code = 1;
                    returnJsonData.msg = "未查找到用户信息！";
                }
            }
            
            return Json(returnJsonData, JsonRequestBehavior.AllowGet);
        
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            var sessionInfo = System.Web.HttpContext.Current.Session["Users"];
            if (sessionInfo!=null)
            {
                Session.Remove("Users");
                Session.Clear();      // 清除所有Session
                Session.Abandon();
                returnJsonData.code = 0;
                returnJsonData.msg = "缓存已清理，正在退出！";
            }
            else
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "请刷新重试！";
            }
            //returnJsonData.code = 0;
            return Json(returnJsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改密码功能
        /// </summary>
        /// <param name="t_Users"></param>
        /// <param name="oldPassWord"></param>
        /// <param name="newPassWord"></param>
        /// <returns></returns>
        public ActionResult ChangePassWord(Users t_Users,string oldPassWord,string newPassWord)
        {
            var user = db.Users.Find(t_Users.ID);
            if (oldPassWord==user.Password)
            {
                if (oldPassWord== newPassWord)
                {
                    returnJsonData.code = 1;
                    returnJsonData.msg = "新密码不能与原密码相同！";
                }
                else
                {
                    user.Password = newPassWord;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    if (db.SaveChanges() > 0)
                    {
                        returnJsonData.code = 0;
                        //更新缓存
                        Session["Users"] = user;
                        returnJsonData.msg = "修改成功！";
                    }
                    else
                    {
                        returnJsonData.code = 1;
                        returnJsonData.msg = "修改失败！";
                    }
                }
            }
            else
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "原密码输入不正确！";
            }
            return Json(returnJsonData, JsonRequestBehavior.AllowGet);
        }

    }
}