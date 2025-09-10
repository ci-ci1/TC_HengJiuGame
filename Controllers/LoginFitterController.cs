using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class LoginFitterController : ActionFilterAttribute
    {

        public static Users _adminUsers = null;
        /// <summary>
        /// 重写过滤器，判断用户是否登陆
        /// </summary>
        /// <returns></returns>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //获取登陆用户信息
            var sessionInfo = System.Web.HttpContext.Current.Session["Users"];
            if (sessionInfo != null)
            {
                //拆箱
                Users entity = sessionInfo as Users;
                //返回页面赋值
                _adminUsers = entity;
            }
            else
            {
                filterContext.Result = new RedirectResult("/Login/Index");
            }
        }


    }
}