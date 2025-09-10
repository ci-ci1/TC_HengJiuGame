using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Linq;

namespace TC_HengJiuGame.Controllers.Utilities
{
    /// <summary>
    /// 字段配置信息类，用于定义导出时字段的显示名称和格式化规则
    /// </summary>
    public class FieldConfig
    {
        /// <summary>
        /// 实体类中的字段原名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Excel表头中显示的名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 字段值的格式化函数，用于将原始值转换为需要显示的格式
        /// </summary>
        public Func<object, string> Formatter { get; set; }
    }

    /// <summary>
    /// Excel导出工具类，支持泛型数据导出，可配置导出字段和格式化规则
    /// </summary>
    public static class ExportToExcel
    {
        /// <summary>
        /// 导出数据到Excel文件
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="data">要导出的数据集合</param>
        /// <param name="fileName">导出的Excel文件名（不含扩展名）</param>
        /// <param name="fieldConfigs">字段配置列表，用于定义字段显示名称和格式化方式</param>
        /// <param name="fieldsToExport">指定要导出的字段列表，为null则导出所有字段</param>
        public static void Export<T>(IEnumerable<T> data, string fileName,
                                    List<FieldConfig> fieldConfigs = null,
                                    List<string> fieldsToExport = null)
        {
            // 验证输入数据是否为空
            if (data == null)
                throw new ArgumentNullException(nameof(data), "导出数据不能为空");

            // 创建Excel工作簿（XSSFWorkbook用于处理.xlsx格式）
            using (var workbook = new XSSFWorkbook())
            {
                // 创建工作表，命名为"数据"
                var sheet = workbook.CreateSheet("数据");

                // 获取实体类的所有公共属性
                var allProperties = typeof(T).GetProperties();

                // 筛选出需要导出的属性
                // 如果指定了fieldsToExport，则只保留在列表中的属性；否则导出所有属性
                var propertiesToExport = allProperties
                    .Where(p => fieldsToExport == null || fieldsToExport.Contains(p.Name))
                    .ToList();

                // 创建表头行（第一行）
                var headerRow = sheet.CreateRow(0);
                for (int i = 0; i < propertiesToExport.Count; i++)
                {
                    // 根据配置获取列的显示名称
                    string columnName = GetColumnName(propertiesToExport[i].Name, fieldConfigs);
                    // 在表头行的第i列设置列名
                    headerRow.CreateCell(i).SetCellValue(columnName);
                }

                // 填充数据行（从第二行开始）
                int rowIndex = 1; // 行索引，从1开始（0是表头）
                foreach (var item in data)
                {
                    // 创建新行
                    var row = sheet.CreateRow(rowIndex++);
                    // 为每个需要导出的属性设置单元格值
                    for (int colIndex = 0; colIndex < propertiesToExport.Count; colIndex++)
                    {
                        // 获取当前属性
                        var property = propertiesToExport[colIndex];
                        // 获取属性值
                        var value = property.GetValue(item);
                        // 根据配置格式化属性值
                        string cellValue = GetFormattedValue(property.Name, value, fieldConfigs);
                        // 设置单元格值
                        row.CreateCell(colIndex).SetCellValue(cellValue);
                    }
                }

                // 自动调整所有列的宽度，以适应内容
                for (int i = 0; i < propertiesToExport.Count; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                // 准备HTTP响应，用于下载文件
                var response = HttpContext.Current.Response;
                // 清除响应中的现有内容
                response.Clear();
                // 设置响应内容类型为Excel文件
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                // 设置下载文件名，包含编码处理以支持中文
                response.AddHeader("Content-Disposition",
                    $"attachment; filename={HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8)}.xlsx");

                // 将工作簿内容写入响应流
                workbook.Write(response.OutputStream);
                // 确保所有数据都被发送到客户端
                response.Flush();
                // 完成请求处理，避免后续操作干扰
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// 根据字段配置获取列的显示名称
        /// </summary>
        /// <param name="fieldName">字段原名</param>
        /// <param name="fieldConfigs">字段配置列表</param>
        /// <returns>列的显示名称</returns>
        private static string GetColumnName(string fieldName, List<FieldConfig> fieldConfigs)
        {
            // 如果有配置，优先使用配置的显示名称
            if (fieldConfigs != null)
            {
                var config = fieldConfigs.Find(c => c.FieldName == fieldName);
                if (config != null && !string.IsNullOrEmpty(config.DisplayName))
                {
                    return config.DisplayName;
                }
            }

            // 没有配置则使用字段原名
            return fieldName;
        }

        /// <summary>
        /// 根据字段配置格式化字段值
        /// </summary>
        /// <param name="fieldName">字段原名</param>
        /// <param name="value">字段原始值</param>
        /// <param name="fieldConfigs">字段配置列表</param>
        /// <returns>格式化后的字段值</returns>
        private static string GetFormattedValue(string fieldName, object value, List<FieldConfig> fieldConfigs)
        {
            // 处理空值
            if (value == null)
                return "";

            // 如果有配置的格式化函数，优先使用
            if (fieldConfigs != null)
            {
                var config = fieldConfigs.Find(c => c.FieldName == fieldName);
                if (config != null && config.Formatter != null)
                {
                    return config.Formatter(value);
                }
            }

            // 默认格式化处理：对日期类型进行特殊处理
            if (value is DateTime dateValue)
            {
                return dateValue.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 其他类型直接转换为字符串
            return value.ToString();
        }
    }
}
