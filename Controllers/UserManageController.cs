using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TC_HengJiuGame.Controllers.Utilities;
using TC_HengJiuGame.Models;

namespace TC_HengJiuGame.Controllers
{
    public class UserManageController : Controller
    {
        HengJiuGameEntities db = new HengJiuGameEntities();

        ReturnJsonListData listData = new ReturnJsonListData();

        ReturnJsonData jsonData = new ReturnJsonData();


        // GET: UserManage
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadInfo()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public ActionResult GetUserList(int page, int limit, string name, string startTime, string endTime)
        {
            var list = db.Users.ToList();

            if (!string.IsNullOrEmpty(name))
            {
                list = list.Where(a => a.UserCode.Contains(name)).ToList();
            }

            // 时间区间查询
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                list = list.Where(a => a.CreateDate >= DateTime.Parse(startTime) && a.CreateDate <= DateTime.Parse(endTime).AddDays(1).AddSeconds(-1)).ToList();
            }

            var offset = (page - 1) * limit;
            var newList = list.OrderByDescending(a => a.CreateDate).Skip(offset).Take(limit).ToList();


            listData.code = 0;
            listData.msg = "";
            listData.count = list.Count;
            listData.data = newList;

            return Json(listData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult Delete(Guid ID)
        {
            try
            {
                var user = db.Users.Find(ID);
                if (user != null)
                {
                    db.Users.Remove(user);

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
                else
                {
                    jsonData.code = 1;
                    jsonData.msg = "未查找到用户信息！";
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
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult DeleteAll(string ids)
        {
            try
            {
                //将逗号分隔的ID字符串拆分为字符串数组var idsList = ids.Split(',’);
                var idsList = ids.Split(',');

                //1.从数据库查询所有用户并转换为列表
                //2.筛选出ID在待删除ID列表中的用户
                //注意:此写法会先加载所有用户到内存再选，数据量大时效率较低var list = db.T_Users.ToList()
                var list = db.Users.ToList().Where(a => idsList.Contains(a.ID.ToString())).ToList();
                //从数据库上下文中批量移除筛选出的用户
                db.Users.RemoveRange(list);

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
            catch (Exception ex)
            {
                jsonData.code = 1;
                jsonData.msg = "删除失败！" + ex.Message;
            }
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        //添加修改
        public ActionResult Save(Users users)
        {
            try
            {
                if (users.ID != Guid.Empty)
                {
                    var list = db.Users.Find(users.ID);
                    if (list != null)
                    {
                        list.UserCode = users.UserCode;
                        list.UserName = users.UserName;
                        list.Sex = users.Sex;
                        list.NativePlace = users.NativePlace;
                        list.Email = users.Email;
                        list.Tel = users.Tel;
                        list.Address = users.Address;
                        list.BirthDay = users.BirthDay;
                        db.Entry(list).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        jsonData.code = 1;
                        jsonData.msg = "未查找到用户信息！";
                    }
                }
                else
                {
                    users.ID = Guid.NewGuid();
                    users.ModifyDate = DateTime.Now;
                    users.CreateDate = DateTime.Now;
                    users.Status = true;
                    db.Users.Add(users);
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



        //导出
        /// <summary>
        /// 导出用户数据到Excel文件
        /// 支持两种导出场景：按查询条件导出全部数据 或 导出用户勾选的特定数据
        /// </summary>
        /// <param name="name">查询条件：用户账号或姓名（用于模糊搜索）</param>
        /// <param name="startTime">查询条件：创建时间的起始值</param>
        /// <param name="endTime">查询条件：创建时间的结束值</param>
        /// <param name="selectedData">选中数据的ID集合（JSON格式字符串，为空则导出查询结果）</param>
        /// <returns>EmptyResult（导出成功时）或包含错误信息的JSON（导出失败时）</returns>
        public ActionResult ExportUserList(string name, string startTime, string endTime, string selectedData = null)
        {
            try
            {
                // 1. 获取用户数据查询对象
                // 使用IQueryable实现延迟加载，避免过早执行SQL查询，提高数据处理效率
                IQueryable<Users> list = db.Users.AsQueryable();

                // 2. 根据查询条件过滤数据
                // 处理姓名/账号筛选条件
                if (!string.IsNullOrEmpty(name))
                {
                    // 模糊匹配用户账号或用户姓名，扩大搜索范围
                    list = list.Where(a => a.UserCode.Contains(name) || a.UserName.Contains(name));
                }

                // 处理时间范围筛选条件
                if (!string.IsNullOrEmpty(startTime) && DateTime.TryParse(startTime, out DateTime startDate) &&
                    !string.IsNullOrEmpty(endTime) && DateTime.TryParse(endTime, out DateTime endDate))
                {
                    // 调整结束时间为当天23:59:59，确保包含结束日期当天的所有数据
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                    // 根据创建时间筛选数据
                    list = list.Where(a => a.CreateDate >= startDate && a.CreateDate <= endDate);
                }

                // 3. 处理选中数据导出（优先级高于查询条件）
                if (!string.IsNullOrEmpty(selectedData))
                {
                    // 将前端传递的JSON格式ID数组反序列化为字符串列表
                    // 前端需将选中行的ID转换为JSON字符串传递（如：["id1","id2"...]）
                    var selectedUsers = JsonConvert.DeserializeObject<List<string>>(selectedData);

                    // 根据选中的ID筛选数据
                    // 注意：数据库中ID为Guid类型，需转换为字符串才能与前端传递的ID匹配
                    list = list.Where(a => selectedUsers.Contains(a.ID.ToString()));
                }

                // 4. 定义需要导出的字段列表
                // 仅包含前端表格中展示的字段，与页面显示保持一致
                var tableFields = new List<string>
        {
            "UserCode", "UserName", "Sex", "BirthDay", "Age",
            "Status", "Email", "Tel", "CreateDate", "Address"
        };

                // 5. 配置字段的显示名称和格式化规则
                var fieldConfigs = new List<FieldConfig>
        {
            // 员工账号字段配置
            new FieldConfig { FieldName = "UserCode", DisplayName = "员工账号" },
            
            // 员工名称字段配置
            new FieldConfig { FieldName = "UserName", DisplayName = "员工名称" },

            // 性别字段配置：将布尔值转换为中文显示
            new FieldConfig
            {
                FieldName = "Sex",
                DisplayName = "性别",
                Formatter = value => value is bool sexBool ? (sexBool ? "男" : "女") : "未知"
            },
            
            // 出生日期字段配置：格式化日期显示
            new FieldConfig
            {
                FieldName = "BirthDay",
                DisplayName = "出生日期",
                Formatter = value => value is DateTime birthDate ? birthDate.ToString("yyyy-MM-dd") : ""
            },

            // 年龄字段配置
            new FieldConfig { FieldName = "Age", DisplayName = "年龄" },

            // 在职状态字段配置：将布尔值转换为中文显示
            new FieldConfig
            {
                FieldName = "Status",
                DisplayName = "是否在职",
                Formatter = value => value is bool statusBool ? (statusBool ? "在职" : "离职") : "未知"
            },
            
            // 邮箱字段配置
            new FieldConfig { FieldName = "Email", DisplayName = "邮箱" },

            // 电话字段配置
            new FieldConfig { FieldName = "Tel", DisplayName = "电话" },

            // 创建时间字段配置：格式化时间显示
            new FieldConfig
            {
                FieldName = "CreateDate",
                DisplayName = "创建时间",
                Formatter = value => value is DateTime createDate ? createDate.ToString("yyyy-MM-dd HH:mm:ss") : ""
            },

            // 员工住址字段配置
            new FieldConfig { FieldName = "Address", DisplayName = "员工住址" }
        };

                // 6. 调用通用导出工具类，执行Excel导出
                // 将筛选后的数据、文件名、字段配置和导出字段列表传入
                ExportToExcel.Export(list.ToList(), "用户列表", fieldConfigs, tableFields);
            }
            catch (Exception ex)
            {
                // 7. 异常处理
                // 记录错误日志到调试输出窗口，便于开发阶段排查问题
                System.Diagnostics.Debug.WriteLine($"导出失败: {ex.Message}");

                // 向前端返回错误信息，包含错误代码和描述
                return Json(new { code = 1, msg = "导出失败: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }

            // 8. 导出成功时返回空结果
            // 因为文件内容已通过响应流直接发送到客户端，无需额外返回数据
            return new EmptyResult();
        }

        //导入
        public ActionResult UploadEmployeeData(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    // 存储所有员工数据的列表
                    List<Users> employeeList = new List<Users>();
                    // 存储上传结果的列表
                    List<UploadResult> resultList = new List<UploadResult>();

                    // 获取文件流
                    using (Stream stream = file.InputStream)
                    {
                        // 根据文件扩展名判断文件类型
                        IWorkbook workbook;
                        if (file.FileName.EndsWith(".xlsx"))
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                        else if (file.FileName.EndsWith(".xls"))
                        {
                            // 如果你需要支持旧版Excel格式
                            workbook = new HSSFWorkbook(stream);
                        }
                        else
                        {
                            return Json(new { success = false, message = "请上传.xls或.xlsx格式的文件" });
                        }

                        // 获取第一个工作表
                        ISheet sheet = workbook.GetSheetAt(0);

                        // 遍历工作表的行 - 从第2行开始（索引1），跳过表头
                        for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
                        {
                            IRow row = sheet.GetRow(rowIndex);
                            if (row != null)
                            {
                                // 获取关键单元格的值，用于判断是否为空行
                                string userCode = GetCellValue(row.GetCell(1));
                                string userName = GetCellValue(row.GetCell(2));

                                // 如果员工账号和员工姓名都为空，认为是无效空行，跳过处理
                                if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(userName))
                                {
                                    continue;
                                }

                                // 创建用户对象
                                Users user = new Users();
                                UploadResult result = new UploadResult();

                                try
                                {
                                    // 读取单元格数据（根据你的Excel列顺序调整索引）
                                    // 假设列顺序：员工账号(1)、姓名(2)、性别(3)、出生日期(4)、邮箱(5)、电话(6)、住址(7)
                                    user.UserCode = userCode;
                                    user.UserName = userName;
                                    user.Sex = GetCellValue(row.GetCell(3)) == "男" ? true : false;

                                    // 处理日期
                                    if (row.GetCell(4) != null)
                                    {
                                        if (row.GetCell(4).CellType == CellType.Numeric)
                                        {
                                            user.BirthDay = row.GetCell(4).DateCellValue;
                                        }
                                        else
                                        {
                                            DateTime.TryParse(GetCellValue(row.GetCell(4)), out DateTime birthDate);
                                            user.BirthDay = birthDate;
                                        }
                                    }

                                    user.Email = GetCellValue(row.GetCell(5));
                                    user.Tel = GetCellValue(row.GetCell(6));
                                    user.Address = GetCellValue(row.GetCell(7));

                                    user.ID = Guid.NewGuid();
                                    user.Status = true;
                                    user.CreateDate = DateTime.Now;
                                    user.ModifyDate = DateTime.Now;
                                    user.Password = "123456";


                                    // 添加到列表
                                    employeeList.Add(user);

                                    // 记录成功结果
                                    result.EmployeeId = userCode;  // 与前端保持一致
                                    result.Success = true;
                                    result.Message = "解析成功";
                                }
                                catch (Exception ex)
                                {
                                    // 记录失败结果
                                    result.EmployeeId = userCode ?? $"第{rowIndex + 1}行";
                                    result.Success = false;
                                    result.Message = $"解析错误: {ex.Message}";
                                }

                                resultList.Add(result);
                            }
                        }
                    }

                    // 将数据保存到Session
                    Session["UploadedEmployees"] = employeeList;

                    // 将数据添加到数据库
                    try
                    {
                        foreach (var user in employeeList)
                        {
                            // 可以在这里添加重复检查等逻辑
                            db.Users.Add(user);
                        }
                        db.SaveChanges();

                        // 更新结果为数据库插入成功
                        foreach (var result in resultList)
                        {
                            if (result.Success)
                            {
                                result.Message = "已成功导入数据库";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 数据库操作失败
                        return Json(new
                        {
                            success = false,
                            message = $"数据库操作失败: {ex.Message}",
                            data = resultList
                        });
                    }

                    // 返回成功消息和结果列表
                    return Json(new
                    {
                        success = true,
                        message = $"文件上传成功，共处理 {employeeList.Count} 条数据",
                        data = resultList
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "文件处理出错: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "未选择文件" });
        }

        // 辅助方法：获取单元格值
        private string GetCellValue(ICell cell)
        {
            if (cell == null)
                return string.Empty;

            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        // 修正：处理可空DateTime的格式化问题
                        return cell.DateCellValue?.ToString("yyyy-MM-dd") ?? string.Empty;
                    }
                    else
                    {
                        return cell.NumericCellValue.ToString();
                    }
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Formula:
                    return cell.CellFormula;
                default:
                    return string.Empty;
            }
        }

        public class UploadResult
        {
            [JsonProperty("employeeId")]
            public string EmployeeId { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }





    }
}