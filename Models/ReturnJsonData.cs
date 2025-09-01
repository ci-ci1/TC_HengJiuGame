using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TC_HengJiuGame.Models
{
    //返回实体类型的Json格式数据
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
    //返回集合类型的Json格式数据
    public class ReturnJsonListData
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
        /// 总条数
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public IEnumerable<object> data { get; set; }
    }
}