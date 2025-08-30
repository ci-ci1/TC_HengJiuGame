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
        TC_HengJiuGame_DBEntities db = new TC_HengJiuGame_DBEntities();

        //返回实例
        ReturnJsonData returnJsonData = new ReturnJsonData();

        #region 视图
        //首页视图
        public ActionResult Index()
        {
            if (Session["T_Users"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            //返回页面赋值
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }

        //修改信息视图
        public ActionResult Edit()
        {
            //返回页面赋值
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }




        //修改密码视图
        public ActionResult EditPassWord()
        {
            ViewBag.UserModel = LoginFitterController._adminUsers;
            return View();
        }
        #endregion




        //修改功能实现
        public ActionResult Revise(T_Users t_Users)
        {
            if (t_Users.ID==Guid.Empty)
            {
                returnJsonData.code = 1;
                returnJsonData.msg = "未查找到用户信息！";
            }
            else
            {
                var user = db.T_Users.Find(t_Users.ID);
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
                        Session["T_Users"] = user;
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

        //退出登录
        public ActionResult Logout()
        {
            var sessionInfo = System.Web.HttpContext.Current.Session["T_Users"];
            if (sessionInfo!=null)
            {
                Session.Remove("T_Users");
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


        //修改密码功能
        public ActionResult ChangePassWord(T_Users t_Users,string oldPassWord,string newPassWord)
        {
            var user = db.T_Users.Find(t_Users.ID);
            if (oldPassWord==user.PassWord)
            {
                if (oldPassWord== newPassWord)
                {
                    returnJsonData.code = 1;
                    returnJsonData.msg = "新密码不能与原密码相同！";
                }
                else
                {
                    user.PassWord = newPassWord;
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    if (db.SaveChanges() > 0)
                    {
                        returnJsonData.code = 0;
                        //更新缓存
                        Session["T_Users"] = user;
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