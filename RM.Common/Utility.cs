using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace RM.Common
{



    public class Utility
    {



       /// <summary>
       /// 文件名中非法字符过滤
       /// </summary>
       /// <param name="str"></param>
       /// <returns></returns>
        public static string FilenameFilter(string str)
        {

            if (string.IsNullOrEmpty(str)) {
                return "1";
            }
            var b=  Regex.Replace(str, "[\\\\/:*?\"<>|]", "").Trim();

            return string.IsNullOrEmpty(b) ? "1" : b;


        }


        /// <summary>
        /// 分页类型
        /// </summary>
        public class Paging
        {
            /// <summary>
            /// 得到当前页码
            /// </summary>
            /// <param name="inputParams"></param>
            /// <returns></returns>
            public static int GetPageNumber(JObject inputParams)
            {
                int limit = int.Parse(inputParams["limit"].ToString());  //bootstrapTable的每页条目数
                int offset = int.Parse(inputParams["offset"].ToString());  ///bootstrapTable的偏移量

                return (offset / limit) + 1;
            }


            /// <summary>
            /// 得到每页条目数
            /// </summary>
            /// <param name="inputParams"></param>
            /// <returns></returns>
            public static int GetPageSize(JObject inputParams)
            {
                int limit = int.Parse(inputParams["limit"].ToString());  //bootstrapTable的每页条目数
                return limit;
            }


            /// <summary>
            /// 得到排序规则
            /// </summary>
            /// <param name="inputParams"></param>
            /// <returns></returns>
            public static string GetSort(JObject inputParams)
            {
                string order = inputParams["order"].ToString();

                string sort = "";
                if (inputParams["sort"] == null)
                {
                    sort = "ID";
                }
                else
                {
                    sort = inputParams["sort"].ToString();
                }

                return sort + " " + order;
            }


        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <typeparam name="T">方法所在的类</typeparam>
        /// <param name="t">方法所在的类的实例</param>
        /// <param name="method">要执行的方法</param>
        /// <param name="parms">传递的参数数组，类型与顺序必须与方法接受的类型一致</param>
        /// <param name="modelType">方法需要的泛型，默认为null</param>
        /// <param name="isStatic">方法是否为静态方法，默认为false</param>
        /// <returns></returns>
        public static object InvokeMethod<T>(T t, string method, object[] parms, Type modelType = null, bool isStatic = false)
        {
            object res = null;
            //object aci = Activator.CreateInstance(modelType);//按类型实例化
            MethodInfo mInfo = typeof(T).GetMethod(method);//查找要执行的方法
            if (modelType != null)
            {
                mInfo = mInfo.MakeGenericMethod(new[] { modelType });//如果方法为泛型，用类型数组代替
            }
            if (isStatic)
            {
                res = mInfo.Invoke(null, parms);//静态类执行方法
            }
            else
            {
                res = mInfo.Invoke(t, parms);////非静态类执行方法
            }

            return res;
        }

        /// <summary>
        /// 将IEnumerable集合转换为DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">IEnumerable集合</param>
        /// <param name="RealyNameList">中文名集合</param>
        /// <param name="NoFieldList">原始集合</param>
        /// <returns></returns>
        public static DataTable AsDataTable<T>(IEnumerable<T> data, Dictionary<string, string> RealyNameList = null, List<string> NoFieldList = null)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                if (NoFieldList != null && !NoFieldList.Contains(prop.Name))//不包含字段
                    if (RealyNameList != null && RealyNameList.ContainsKey(prop.Name))//重命名字段
                    {
                        table.Columns.Add(RealyNameList[prop.Name], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
                    else
                    {
                        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    if (NoFieldList != null && !NoFieldList.Contains(prop.Name))//不包含字段
                        if (RealyNameList != null && RealyNameList.ContainsKey(prop.Name))//重命名字段
                        {
                            row[RealyNameList[prop.Name]] = prop.GetValue(item) ?? DBNull.Value;
                        }
                        else
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                table.Rows.Add(row);
            }
            return table;
        }
        /// <summary>
        /// 检查非法字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns>存在非法字符返回true</returns>
        public static bool CheckIllegalString(string str)
        {
            //去除执行存储过程的命令关键字
            string str1 = @"Exec|Execute";

            //去除系统存储过程或扩展存储过程关键字
            string str2 = @"xp_|sp_";

            //防止16进制注入
            string str3 = @"0x";

            //删除与数据库相关的词
          //  string str4 = @"select|insert|delete|count|drop table|truncate|asc|mid|char|xp_cmdshell|exec master|net localgroup administrators|and|net user|or|net|from|drop|script|declare";
			string str4 = @"\s+select\s+|\s+insert\s+|\s+delete\s+|\s+count\s+|drop table|truncate|\s+asc\s+|\s+mid\s+|\s+char\s+|xp_cmdshell|exec master|net localgroup administrators|\s+and\s+|net user|\s+or\s+|\s+net\s+|\s+from\s+|\s+drop\s+|\s+script\s+|\s+declare\s+";
			//特殊的字符
			string str5 = @"\<|\>|\*|\-{2}|\?|\'|\/|\;|\*/|\\r|\\n|\,|\)|\(|\@|\=|\+|\&|\#|\%|\$";//废弃
            str5 = @"\<|\>|\*|\-{2}|\?|\'|\;|\*/|\\r|\\n|\,|\)|\(|\@|\=|\+|\&|\#|\%|\$";//新的
            str5 = @"\-{2}|\=";//去掉特殊符号判断 by zx
            string pattern = str1 + "|" + str2 + "|" + str3 + "|" + str4 + "|" + str5;
            pattern = pattern.Trim('|');
            if (Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 恢复被限制的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns>恢复被限制的字符串</returns>
        public static string RecoverIllegalString(string str)
        {
            Dictionary<string, string> dicRecover = new Dictionary<string, string>();
            dicRecover["[|p|]"] = "<sup>";
            dicRecover["[||p|]"] = "</sup>";
            dicRecover["[|b|]"] = "<sub>";
            dicRecover["[||b|]"] = "</sub>";

            foreach (var item in dicRecover)
            {
                if (str.Contains(item.Key))
                {
                    str = str.Replace(item.Key, item.Value);
                }
            }

            return str;
        }

        /// <summary>
        /// 将model转换为Dynamic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static DynamicFields ModelToDynamicFields<T>(T t)
        {
            var parms = new DynamicFields();

            foreach (var item in t.GetType().GetProperties())
            {
                parms.Add(item.Name, item.GetValue(t) == null ? null : item.GetValue(t).ToString());
            }

            return parms;
        }

        /// <summary>
        /// 判断用户是否是超级管理员
        /// </summary>
        /// <param name="LoginID">登陆用户的用户名</param>
        /// <returns></returns>
        public static bool IsAdmin(string LoginID)
        {
            var config = IocContainer.Resolve<IConfigurations>();
            var segs = config.SuperAdministratorName.Split(',');
            if (segs.Contains(LoginID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 日志发生位置
    /// </summary>
    public enum LogLocation
    {
        ApiController,
        WinService,
        BLL,
        DAL
    }
}
