using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TC_HengJiuGame.Models
{
    public class ReturnJsonData
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回描述
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public object data { get; set; }
    }
}