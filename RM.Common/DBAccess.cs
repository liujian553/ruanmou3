using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RM.Common
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public class DBConn
    {
        /*
         
        1   查询 - 返回IEnumerable<T>
        2   查询 - 返回第一个T
        3   查询 - 返回IEnumerable<T> - sql方式
        4   查询 - 返回数据表行数
        5   单条插入 - 实体对象就是表结构
        6   批量插入 - 存储过程 - 单表
        7   单条更新 - 实体对象就是表结构
        8   批量更新 - 存储过程 - 单表
        9   单条删除 - 实体对象就是表结构
        10 批量删除 - 带条件
        11 批量删除 - 存储过程 - 单表
        12 存储过程 - 返回IEnumerable<T>
        13 存储过程 - 无返回值
        14 存储过程 - 返回受影响的行数
        15 返回第一行第一列

        */

        /// <summary>
        /// 数据库写连接
        /// </summary>
        public static IDbConnection WriteConn
        {
            get
            {
                var config = IocContainer.Resolve<IConfigurations>();
                var connStr = config.DBConnectionStringWrite;
                return new SqlConnection(connStr);
            }
        }


        /// <summary>
        /// 数据库读连接
        /// </summary>
        public static IDbConnection ReadConn
        {
            get
            {
                var config = IocContainer.Resolve<IConfigurations>();
                var connStr = config.DBConnectionStringRead;
                return new SqlConnection(connStr);
            }
        }

        #region 查询 - 返回IEnumerable<T>

        /// <summary>
        /// 查询 - 返回IEnumerable T
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="tableName">表名</param>
        /// <param name="conditions">查询条件</param>
        /// <param name="param">查询参数</param>
        /// <param name="columns">查询要展示的字段</param>
        /// <returns></returns>
        public static IEnumerable<T> Retrieves<T>(string tableName, string conditions, object param, string columns = "*")
        {
            if (string.IsNullOrEmpty(columns))
            {
                columns = "*";
            }

            var query = string.Format("select {2} from {0} WHERE {1}", tableName, conditions, columns);
            return DBConn.ReadConn.Query<T>(query, param, null, true);
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="conditions"></param>
        /// <param name="param"></param>
        /// <param name="columns"></param>
        /// <param name="sort"></param>
        /// <param name="pageNum">页码，从1开始</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> RetrieveMultiple<T>(string tableName, string conditions, object param, string columns = "*", string sort = null, int pageNum = -1, int pageSize = -1)
        {
            if (string.IsNullOrEmpty(columns)) columns = "*";
            var query = "";
            if (pageNum == -1 || pageSize == -1 || string.IsNullOrEmpty(sort))
            {
                if (string.IsNullOrEmpty(conditions))
                {
                    query = string.Format("select {1} from {0}", tableName, columns);
                }
                else
                {
                    query = string.Format("select {2} from {0} WHERE {1}", tableName, conditions, columns);
                }
                if (!string.IsNullOrEmpty(sort))
                {
                    query += " order by " + sort;
                }
            }
            else
            {
                query = string.Format("select * from (select row_number() over (order by {3}) n ,{2} from {0} WHERE {1}) a where n > {4} and n <= {5}"
                    , tableName, conditions, columns, sort, pageSize * (pageNum - 1), pageSize * pageNum);
            }
            return DBConn.ReadConn.Query<T>(query, param, null, true);
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="conditions"></param>
        /// <param name="param"></param>
        /// <param name="count"></param>
        /// <param name="columns"></param>
        /// <param name="sort"></param>
        /// <param name="pageNum">页码，从1开始</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> RetrieveMultipleWithCount<T>(string tableName, string conditions, object param, out int count, string columns = "*", string sort = null, int pageNum = -1, int pageSize = -1)
        {
            //count
            count = RetrieveCount(tableName, conditions, param);

            //result
            return RetrieveMultiple<T>(tableName, conditions, param, columns, sort, pageNum, pageSize);
        }

        #endregion

        #region 查询 - 返回第一个T
        //scott_miracle: Retrieve重命名RetrieveFirst
        /// <summary>
        /// 查询 - 返回第一个T
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="tableName">表名</param>
        /// <param name="conditions">查询条件</param>
        /// <param name="param">查询参数</param>
        /// <param name="columns">查询要展示的字段</param>
        /// <returns></returns>
        public static T RetrieveFirst<T>(string tableName, string conditions, object dynamicParams, string columns = "*")
        {
            if (string.IsNullOrEmpty(columns))
            {
                columns = "*";
            }
            if (dynamicParams != null)
            {
                var type = dynamicParams.GetType();
                if (type == typeof(DynamicFields))
                {
                    var param = (DynamicFields)dynamicParams;
                    dynamicParams = param.Values;
                }
            }
            var query = string.Format("select {2} from {0} WHERE {1}", tableName, conditions, columns);
            return DBConn.ReadConn.Query<T>(query, dynamicParams, null, true).FirstOrDefault();
        }
        #endregion

        #region 查询 - 返回IEnumerable<T> - sql方式
        /// <summary>
        /// 查询 - 返回IEnumerable T  - sql方式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static IEnumerable<T> RetrieveBySql<T>(string sql, object param)
        {
            return DBConn.ReadConn.Query<T>(sql, param);
        }
        #endregion

        #region 查询 - 返回数据表行数
        /// <summary>
        /// 查询 - 返回数据表行数
        /// </summary>
        /// <param name="tableName">表明</param>
        /// <param name="conditions">查询条件</param>
        /// <param name="param">查询参数</param>
        /// <returns></returns>
        public static int RetrieveCount(string tableName, string conditions, object param)
        {
            var query = "";
            //count
            if (string.IsNullOrEmpty(conditions))
            {
                query = string.Format("select count(0) count from {0}", tableName);
            }
            else
            {
                query = string.Format("select count(0) count from {0} WHERE {1}", tableName, conditions);
            }
            return DBConn.ReadConn.Query<int>(query, param).FirstOrDefault();
        }
        #endregion

        #region 查询 - 返回第一个值
        /// <summary>
        /// 查询 - 返回第一个值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public static string RetrieveStr(string sql, object param)
        {
            return DBConn.ReadConn.Query<string>(sql, param).FirstOrDefault();
        }
        #endregion

        #region 单条插入 - 实体对象就是表结构
        /// <summary>
        /// 单条插入 - 实体对象就是表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public static int Create<T>(T entity, IDbTransaction transaction = null)
        {
            var type = entity.GetType();
            if (type.IsDefined(typeof(DataTableAttrbute), true))
            {
                var tableAtt = (DataTableAttrbute)type.GetCustomAttributes(typeof(DataTableAttrbute), true).FirstOrDefault();

                string columnName = "";
                string values = "";
                string output = "";
                foreach (var property in type.GetProperties())
                {
                    if (property.IsDefined(typeof(NonDataFieldAttrbute), true))
                    {
                        continue;
                    }

                    if (property.IsDefined(typeof(PrimaryKeyAttrbute), true))
                    {
                        var att = (PrimaryKeyAttrbute)property.GetCustomAttributes(typeof(PrimaryKeyAttrbute), true).FirstOrDefault();
                        if (att.IsIncrement)
                        {
                            output = property.Name;
                            continue;
                        }
                    }
                    var value = property.GetValue(entity);
                    if (value == null)
                    {
                        continue;
                    }

                    columnName += '[' + property.Name + "],";
                    values += '@' + property.Name + ',';
                }

                if (!string.IsNullOrEmpty(output))
                {
                    string query = string.Format("INSERT INTO {0} ({1}) OUTPUT INSERTED.{3} VALUES ({2})"
                        , tableAtt.TableName, columnName.TrimEnd(','), values.TrimEnd(','), output);
                    //对对象进行操作
                    //   var id = DBConn.WriteConn.Query<int>(query, entity, null, true).FirstOrDefault();
                    var id = DBConn.WriteConn.Query<int>(query, entity, transaction, true).FirstOrDefault();
                    type.GetProperty(output).SetValue(entity, id);
                    return 1;
                }
                else
                {
                    string query = string.Format("INSERT INTO {0} ({1}) VALUES ({2})"
                        , tableAtt.TableName, columnName.TrimEnd(','), values.TrimEnd(','));
                    //对对象进行操作
                    return DBConn.WriteConn.Execute(query, entity);
                }
            }

            return -1;
        }
        #endregion

        #region 批量插入
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        //public static int Create<T>(string tableName, string[] updatedFields, IEnumerable<T> param)
        //{
        //    string createSql = string.Format("update {0} set {1} where {1}", tableName, condition);

        //    return DBConn.WriteConn.Execute(createSql, param);        
        //}
        #endregion

        #region 单条更新 - 实体对象就是表结构
        /// <summary>
        /// 单条更新 - 实体对象就是表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="updatedFields">被更新字段名列表</param>
        /// <param name="locktype">锁类型</param>
        /// <returns></returns>
        public static int Update<T>(T entity, string[] updatedFields, HintType locktype = HintType.Default)
        {
            var type = entity.GetType();
            if (type.IsDefined(typeof(DataTableAttrbute), true))
            {
                var tableAtt = (DataTableAttrbute)type.GetCustomAttributes(typeof(DataTableAttrbute), true).FirstOrDefault();

                string hintStr = "";
                switch (locktype)
                {
                    case HintType.With_Nolock:
                        hintStr = " with (nolock)";
                        break;
                    case HintType.With_Readpast:
                        hintStr = " with (readpast)";
                        break;
                }
                string set = "";
                string where = null;

                foreach (var property in type.GetProperties())
                {
                    if (property.IsDefined(typeof(NonDataFieldAttrbute), true))
                    {
                        continue;
                    }

                    if (property.IsDefined(typeof(PrimaryKeyAttrbute), true))
                    {
                        var att = (PrimaryKeyAttrbute)property.GetCustomAttributes(typeof(PrimaryKeyAttrbute), true).FirstOrDefault();
                        where = string.Format("[{0}] = @{0}", property.Name);
                        continue;
                    }
                    if (updatedFields == null || updatedFields.Any(i => i.ToLower() == property.Name.ToLower()))
                    {
                        var value = property.GetValue(entity);
                        if (value == null)
                        {
                            continue;
                        }

                        set += string.Format("[{0}] = @{0}, ", property.Name);
                    }
                }

                string query = string.Format("update {0}{3} set {1} where {2}"
                    , tableAtt.TableName, set.TrimEnd(',', ' '), where, hintStr);
                //对对象进行操作
                return DBConn.WriteConn.Execute(query, entity);
            }

            return -1;
        }
        #endregion 


        #region 单条更新 - 实体对象就是表结构ByTian
        /// <summary>
        /// 单条更新 - 实体对象就是表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="updatedFields">被更新字段名列表</param>
        /// <param name="locktype">锁类型</param>
        /// <returns></returns>
        public static int UpdateByTian<T>(T entity, string[] updatedFields, HintType locktype = HintType.Default)
        {
            var type = entity.GetType();
            if (type.IsDefined(typeof(DataTableAttrbute), true))
            {
                var tableAtt = (DataTableAttrbute)type.GetCustomAttributes(typeof(DataTableAttrbute), true).FirstOrDefault();

                string hintStr = "";
                switch (locktype)
                {
                    case HintType.With_Nolock:
                        hintStr = " with (nolock)";
                        break;
                    case HintType.With_Readpast:
                        hintStr = " with (readpast)";
                        break;
                }
                string set = "";
                string where = null;

                foreach (var property in type.GetProperties())
                {
                    if (property.IsDefined(typeof(NonDataFieldAttrbute), true))
                    {
                        continue;
                    }

                    if (property.IsDefined(typeof(PrimaryKeyAttrbute), true))
                    {
                        var att = (PrimaryKeyAttrbute)property.GetCustomAttributes(typeof(PrimaryKeyAttrbute), true).FirstOrDefault();
                        where = string.Format("[{0}] = @{0}", property.Name);
                        continue;
                    }
                    if (updatedFields == null || updatedFields.Any(i => i.ToLower() == property.Name.ToLower()))
                    {
                        var value = property.GetValue(entity);
                        if (value == null)
                        {
                            //continue;
                        }

                        set += string.Format("[{0}] = @{0}, ", property.Name);
                    }
                }

                string query = string.Format("update {0}{3} set {1} where {2}"
                    , tableAtt.TableName, set.TrimEnd(',', ' '), where, hintStr);
                //对对象进行操作
                return DBConn.WriteConn.Execute(query, entity);
            }

            return -1;
        }
        #endregion 

        #region  批量更新-带条件
        /// <summary>
        /// 批量更新-带条件
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="updatedFields">更新字段</param>
        /// <param name="condition">更新条件</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static int UpdateBySql(string tableName, List<string> updatedFields, string condition, object param)
        {
            string set = "";
            if (updatedFields != null)
            {
                for (int i = 0; i < updatedFields.Count(); i++)
                {
                    set += string.Format("[{0}] = @{0}, ", updatedFields[i]);
                }
            }
            string updateSql = string.Format("update {0} set {1} where {2}", tableName, set.TrimEnd(',', ' '), condition);

            return WriteConn.Execute(updateSql, param);
        }

		#endregion

		#region 新增 - 返回int - sql方式
		/// <summary>
		/// 批量更新-带条件
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="updatedFields">更新字段</param>
		/// <param name="condition">更新条件</param>
		/// <param name="param">参数</param>
		/// <returns></returns>
		public static int BatchBySql(string sql, object param)
        {
            return WriteConn.Execute(sql, param);
        }

        #endregion

        #region 单条删除 - 实体对象就是表结构
        /// <summary>
        /// 单条删除 - 实体对象就是表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public static int Delete<T>(T entity)
        {
            var type = entity.GetType();
            if (type.IsDefined(typeof(DataTableAttrbute), true))
            {
                var tableAtt = (DataTableAttrbute)type.GetCustomAttributes(typeof(DataTableAttrbute), true).FirstOrDefault();
                foreach (var property in type.GetProperties())
                {
                    if (property.IsDefined(typeof(PrimaryKeyAttrbute), true))
                    {
                        var att = (PrimaryKeyAttrbute)property.GetCustomAttributes(typeof(PrimaryKeyAttrbute), true).FirstOrDefault();

                        string query = string.Format("delete from {0} where {1} = @ID"
                            , tableAtt.TableName, property.Name);
                        //对对象进行操作
                        return DBConn.WriteConn.Execute(query, new { ID = property.GetValue(entity) });

                    }
                }
            }
            return -1;
        }
        #endregion

        #region 批量删除 - 带条件
        /// <summary>
        /// 批量删除 - 带条件
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="condition">条件</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static int DeleteBySql(string tableName, string condition, object param)
        {
            string deleteSql = string.Format(" delete from {0}  where {1}", tableName, condition);

            return WriteConn.Execute(deleteSql, param);
        }
        #endregion

        #region 存储过程 - 返回IEnumerable<T>
        /// <summary>
        /// 存储过程 - 返回IEnumerable T
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="storedProcedureName">存储过程名</param>
        /// <param name="dynamicParams">动态字段</param>
        /// <returns></returns>
        public static IEnumerable<T> RetrieveBySP<T>(string storedProcedureName, object dynamicParams)
        {
            if (dynamicParams != null)
            {
                var type = dynamicParams.GetType();
                if (type == typeof(DynamicFields))
                {
                    var param = (DynamicFields)dynamicParams;
                    dynamicParams = param.Values;
                }
            }
            return DBConn.ReadConn.Query<T>(storedProcedureName, dynamicParams, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region 存储过程 - 无返回值
        /// <summary>
        /// 存储过程 - 无返回值
        /// </summary>
        /// <param name="storedProcedureName">存储过程名</param>
        /// <param name="dynamicParams">动态字段</param>
        public static void RetrieveBySP(string storedProcedureName, object dynamicParams)
        {
            if (dynamicParams != null)
            {
                var type = dynamicParams.GetType();
                if (type == typeof(DynamicFields))
                {
                    var param = (DynamicFields)dynamicParams;
                    dynamicParams = param.Values;
                }
            }

            DBConn.ReadConn.Execute(storedProcedureName, dynamicParams, commandType: CommandType.StoredProcedure);
        }
        #endregion

        #region 存储过程 - 返回受影响的行数
        /// <summary>
        /// 存储过程 - 返回受影响的行数
        /// </summary>
        /// <param name="storedProcedureName">存储过程名</param>
        /// <param name="dynamicParams"></param>
        /// <returns></returns>
        public static int ExecuteAffectedSP(string storedProcedureName, object dynamicParams, IDbTransaction transaction = null)
        {
            if (dynamicParams != null)
            {
                var type = dynamicParams.GetType();
                if (type == typeof(DynamicFields))
                {
                    var param = (DynamicFields)dynamicParams;
                    dynamicParams = param.Values;
                }
            }

            return DBConn.WriteConn.Execute(storedProcedureName, dynamicParams, commandType: CommandType.StoredProcedure, transaction: transaction);
        }
        #endregion

        #region 返回第一行第一列
        /// <summary>
        /// 返回第一行第一列
        /// </summary>
        /// <param name="storedProcedureName">存储过程名</param>
        /// <param name="dynamicParams"></param>
        /// <param name="dynamicParams"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string storedProcedureName, object dynamicParams, IDbTransaction transaction = null, CommandType? commandType = null)
        {
            if (dynamicParams != null)
            {
                var type = dynamicParams.GetType();
                if (type == typeof(DynamicFields))
                {
                    var param = (DynamicFields)dynamicParams;
                    dynamicParams = param.Values;
                }
            }

            return DBConn.WriteConn.ExecuteScalar(storedProcedureName, dynamicParams, commandType: commandType, transaction: transaction);
        }
        #endregion

        #region 批量更新 - 存储过程 - 单表

        /// <summary>
        /// 批量更新 - 存储过程 - 单表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">对象集合</param>
        /// <param name="tableName">表名</param>
        /// <param name="updateFieldswhere">字段作为更新条件</param>    
        /// <param name="where">更新条件</param>
        /// <param name="updateFields">updateFields是 null 时 更新所有字段。updateFields有值时 noupdateFields 不起作用</param>
        /// <param name="noupdateFields">updateFields是 null 时，不更新的字段起作用 抱出不需要更新的字段</param>
        /// <returns></returns>
        public static BatchOperationReturnInfo BatchUpdate<T>(List<T> list, string tableName, List<string> updateFieldswhere, string where = null, List<string> updateFields = null, List<string> noupdateFields = null)
        {
            /**
            * 1.sql注入过滤，记录正确数据
            * 2.根据正确数据拼接 部分sql
            * 3.拼接完整参数存入DataTable参数中
            * 4.执行存储过程--单挑执行拼接字符串 返回结果集合 包含正确 错误ID
            * 5.根据返回ID 找到实体对象
            * 6.返回批量处理集合
            * **/
            if (updateFieldswhere == null || updateFieldswhere.Count == 0)
            {
                throw (new Exception("updateFieldswhere 不许为空，如果为空请使用Updatesql方法"));
            }

            List<BatchOperationInfo> batchlist = new List<BatchOperationInfo>();
            List<BatchOperationInfo> errorlist = new List<BatchOperationInfo>();//sql 注入错误信息
            var type = typeof(T);
            //--------------拼接字符串 ---------------------------------------------------------  
            int sqlcount = -1;
            for (int i = 0; i < list.Count; i++)
            {
                string updatewherestr = "";
                string updatestr = "";
                foreach (var property in type.GetProperties())
                {

                    //updateFields is not null 时，更新指定字段 eg [ID]='1'
                    if (updateFields != null && updateFields.Contains(property.Name) && ((noupdateFields != null && !noupdateFields.Contains(property.Name)) || noupdateFields == null))
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            updatestr += string.Format("[{0}] = NULL,", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                updatestr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                updatestr += string.Format("[{0}] = '{1}',", property.Name, value);
                            }
                        }
                    }

                    //updateFields is  null 时，更新所有字段，抛出指定字段
                    if (updateFields == null && ((noupdateFields != null && !noupdateFields.Contains(property.Name)) || noupdateFields == null))
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            updatestr += string.Format("[{0}] = NULL,", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                updatestr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                updatestr += string.Format("[{0}] = '{1}',", property.Name, value);
                            }
                        }
                    }

                    //字段作为更新条件 常见ID做更新条件 where ID='1'
                    if (updateFieldswhere != null && updateFieldswhere.Contains(property.Name))
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            updatewherestr += string.Format(" AND [{0}] is NULL", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                updatestr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                updatewherestr += string.Format(" AND [{0}] = '{1}'", property.Name, value);
                            }
                        }
                    }


                }
                if (!string.IsNullOrEmpty(updatestr))
                {
                    sqlcount++;
                    batchlist.Add(new BatchOperationInfo()
                    {
                        ID = sqlcount,
                        Operationset = String.Format("{0}", updatestr.TrimEnd(',')),
                        Operationwhere = String.Format("{0}", updatewherestr),
                        Entity = list[i]
                    });
                }
            }
            //-----------------------------------------------------------------------------------------------  
            //拼接执行字符串  执行批量操作
            DataTable BatchOperationTable = new DataTable();
            BatchOperationTable.Columns.Add("ID");
            BatchOperationTable.Columns.Add("OperationString");
            foreach (var item in batchlist)
            {
                string operationstr = string.Format("update {0} set {1} where 1=1", tableName, item.Operationset);
                if (!string.IsNullOrEmpty(where))
                {
                    operationstr = operationstr + string.Format(" And {0}", where);
                }

                if (!string.IsNullOrEmpty(item.Operationwhere))
                {
                    operationstr = operationstr + string.Format("{0}", item.Operationwhere);
                }

                item.Operationstr = operationstr;
                BatchOperationTable.Rows.Add(item.ID, item.Operationstr);
            }

            DynamicParameters d = new DynamicParameters();
            d.Add("@OperationTable", BatchOperationTable, DbType.Object);
            List<BatchOperationInfo> returnlist = DBConn.WriteConn.Query<BatchOperationInfo>("USP_BatchOperation", d, commandType: CommandType.StoredProcedure).ToList();
            //为返回对象赋予 处理结果
            foreach (BatchOperationInfo returninfo in returnlist)
            {
                IEnumerable<BatchOperationInfo> batchOperationInfos = batchlist.Where(b => b.ID == returninfo.ID);
                if (batchOperationInfos.Count() > 0)
                {
                    var info = batchOperationInfos.First();
                    info.Success = returninfo.Success;
                    info.Errormsg = returninfo.Errormsg;
                }

            }
            //附加sql注入信息

            batchlist.AddRange(errorlist);
            return new BatchOperationReturnInfo(batchlist);
        }
        #endregion

        #region 批量删除 - 存储过程 - 单表
        /// <summary>
        /// 批量删除 - 存储过程 - 单表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">对象集合</param>
        /// <param name="tableName">表名</param>
        /// <param name="deleteFieldswhere">字段作为删除条件</param>
        /// <param name="where">删除条件</param>
        /// <returns></returns>
        public static BatchOperationReturnInfo BatchDelete<T>(List<T> list, string tableName, List<string> deleteFieldswhere, string where = null)
        { /**
             * 1.sql注入过滤，记录正确数据
             * 2.根据正确数据拼接 部分sql
             * 3.拼接完整参数存入DataTable参数中
             * 4.执行存储过程--单挑执行拼接字符串 返回结果集合 包含正确 错误ID
             * 5.根据返回ID 找到实体对象
             * 6.返回批量处理集合
             * **/
            if (deleteFieldswhere == null || deleteFieldswhere.Count == 0)
            {
                throw (new Exception("deleteFieldswhere 不许为空，如果为空请使用Deletesql方法"));
            }


            List<BatchOperationInfo> batchlist = new List<BatchOperationInfo>(); //批量处理对象 
            List<BatchOperationInfo> errorlist = new List<BatchOperationInfo>();//sql 注入错误信息
            var type = typeof(T);
            int sqlcount = -1;
            //--------------------------------拼接字符串 --where 1=1 and A='哈哈'--------------------------------------------------  
            for (int i = 0; i < list.Count; i++)
            {
                string deletestr = "";
                foreach (var property in type.GetProperties())
                {
                    if (deleteFieldswhere.Contains(property.Name)) //拼接删除条件 eg: [ID]='1'
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            deletestr += string.Format(" And [{0}] = NULL", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //判断sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                deletestr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                deletestr += string.Format(" And  [{0}] = '{1}'", property.Name, value);
                            }
                        }
                    }


                }
                if (!string.IsNullOrEmpty(deletestr))
                {
                    sqlcount++;
                    batchlist.Add(new BatchOperationInfo()
                    {
                        ID = sqlcount,
                        Operationwhere = String.Format("{0}", deletestr),
                        Entity = list[i]
                    });
                }
            }
            //------------------------------------------------------------------------------------------------  
            //拼接执行字符串  执行批量操作
            DataTable BatchOperationTable = new DataTable();
            BatchOperationTable.Columns.Add("ID");
            BatchOperationTable.Columns.Add("OperationString");
            foreach (var item in batchlist)
            {
                string deletestr = string.Format("delete {0} where 1=1 ", tableName);
                if (!string.IsNullOrEmpty(where))
                {
                    deletestr = deletestr + string.Format("AND {0}", where);
                }

                if (!string.IsNullOrEmpty(item.Operationwhere))
                {
                    deletestr = deletestr + string.Format("{0}", item.Operationwhere);
                }

                item.Operationstr = deletestr;
                BatchOperationTable.Rows.Add(item.ID, item.Operationstr);
            }

            DynamicParameters d = new DynamicParameters();
            d.Add("@OperationTable", BatchOperationTable, DbType.Object);
            List<BatchOperationInfo> returnlist = DBConn.WriteConn.Query<BatchOperationInfo>("USP_BatchOperation", d, commandType: CommandType.StoredProcedure).ToList();
            //为返回对象赋予 处理结果
            foreach (BatchOperationInfo returninfo in returnlist)
            {
                IEnumerable<BatchOperationInfo> batchOperationInfos = batchlist.Where(b => b.ID == returninfo.ID);
                if (batchOperationInfos.Count() > 0)
                {
                    var info = batchOperationInfos.First();
                    info.Success = returninfo.Success;
                    info.Errormsg = returninfo.Errormsg;
                }
            }

            //附加sql注入信息
            batchlist.AddRange(errorlist);
            return new BatchOperationReturnInfo(batchlist);
        }
        #endregion

        #region 批量插入 - 存储过程 - 单表
        /// <summary>
        /// 批量插入 - 存储过程 - 单表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">对象集合</param>
        /// <param name="tableName">表名</param>
        /// <param name="insertFields">插入字段是 null时 插入对象的所有字段. insertFields 有值时noinsertFields 不起作用</param>
        /// <param name="noinsertFields">insertFields 是null时 不插入字段起作用 抱出不需要插入的字段</param>
        /// <returns></returns>
        public static BatchOperationReturnInfo BatchInsert<T>(List<T> list, string tableName, List<string> insertFields = null, List<string> noinsertFields = null, IDbTransaction transaction = null)
        { /**
             * 1.sql注入过滤，记录正确数据
             * 2.根据正确数据拼接 部分sql
             * 3.拼接完整参数存入DataTable参数中
             * 4.执行存储过程--单挑执行拼接字符串 返回结果集合 包含正确 错误ID
             * 5.根据返回ID 找到实体对象
             * 6.返回批量处理集合
             * **/
            var type = typeof(T);
            var properties = type.GetProperties();
            List<BatchOperationInfo> batchlist = new List<BatchOperationInfo>();   //批量处理对象 
            List<BatchOperationInfo> errorlist = new List<BatchOperationInfo>();//sql 注入错误信息
            //--------------拼接字符串insert into values(c1,c2) select c1,c2其中的c1，c2---------------------------------  
            string set = "";
            if (insertFields != null)  //insertFields==null?"插入指定列":"插入所有列"
            {

                foreach (var property in properties)//为了保证 insert values(c1,c2)与select c1,c2 一直
                {
                    if (insertFields.Contains(property.Name) && ((noinsertFields != null && !noinsertFields.Contains(property.Name)) || noinsertFields == null))
                    {
                        set += string.Format("{0},", property.Name);
                    }
                }
            }
            else
            {
                foreach (var property in properties)
                {
                    if ((noinsertFields != null && !noinsertFields.Contains(property.Name)) || noinsertFields == null)
                    {
                        set += string.Format("{0},", property.Name);
                    }
                }

            }
            set = set.TrimEnd(',');
            //-----------------------------------------------------------------------------------------------------------------

            //--------------------------------拼接值与字段关系----select A AS field1, B AS field2-----------------------
            int sqlcount = -1;//保证拼接字符串ID连续
            for (int i = 0; i < list.Count; i++)
            {
                string selectstr = "";
                foreach (var property in properties)
                {
                    //insertFields is not  null 时插入指定字段 eg A AS field1, B AS field2
                    if (insertFields != null && insertFields.Contains(property.Name) && ((noinsertFields != null && !noinsertFields.Contains(property.Name)) || noinsertFields == null))
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            selectstr += string.Format("NULL as {0},", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                selectstr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                selectstr += string.Format("'{0}' as {1},", value, property.Name);
                            }
                        }
                    }

                    // insertFields is  null 时插入所有字段 ，抛出指定字段 
                    if (insertFields == null && ((noinsertFields != null && !noinsertFields.Contains(property.Name)) || noinsertFields == null))
                    {
                        if (property.GetValue(list[i]) == null)
                        {
                            selectstr += string.Format("NULL as {0},", property.Name);
                        }
                        else
                        {
                            string value = property.GetValue(list[i]).ToString();
                            if (Utility.CheckIllegalString(value)) //sql注入
                            {
                                errorlist.Add(new BatchOperationInfo()
                                {
                                    ID = -1,
                                    Entity = list[i],
                                    Errormsg = "sql注入",
                                    Success = false
                                });
                                selectstr = "";
                                break;
                            }
                            else
                            {
                                value = Utility.RecoverIllegalString(value);
                                selectstr += string.Format("'{0}' as {1},", value, property.Name);
                            }
                        }
                    }


                }
                if (!string.IsNullOrEmpty(selectstr))
                {
                    sqlcount++;
                    batchlist.Add(new BatchOperationInfo()
                    {
                        ID = sqlcount,
                        Operationset = String.Format("{0}", selectstr.TrimEnd(',')),
                        Entity = list[i]
                    });
                }
            }
            //----------------------------------------------------------------------------------------------
            //拼接执行字符串  执行批量操作
            DataTable BatchOperationTable = new DataTable();
            BatchOperationTable.TableName = "BatchOperationTable";
            BatchOperationTable.Columns.Add("ID");
            BatchOperationTable.Columns.Add("OperationString");
            foreach (var item in batchlist)
            {
                item.Operationstr = string.Format("insert into {0}({1}) select {2}", tableName, set, item.Operationset);
                BatchOperationTable.Rows.Add(item.ID, item.Operationstr);
            }
            DynamicParameters d = new DynamicParameters();
            d.Add("@OperationTable", BatchOperationTable, DbType.Object);
            List<BatchOperationInfo> returnlist = DBConn.WriteConn.Query<BatchOperationInfo>("USP_BatchOperation", d, commandType: CommandType.StoredProcedure, transaction: transaction).ToList();

            //为返回对象赋予 处理结果
            foreach (BatchOperationInfo returninfo in returnlist)
            {
                IEnumerable<BatchOperationInfo> batchOperationInfos = batchlist.Where(b => b.ID == returninfo.ID);
                if (batchOperationInfos.Count() > 0)
                {
                    var info = batchOperationInfos.First();
                    info.IDENTITY1 = returninfo.IDENTITY1;
                    info.Success = returninfo.Success;
                    info.Errormsg = returninfo.Errormsg;
                }
            }
            //附加sql注入信息
            batchlist.AddRange(errorlist);
            return new BatchOperationReturnInfo(batchlist);
        }
        #endregion

    }


    /// <summary>
    /// 动态字段集合类型
    /// </summary>
    public class DynamicFields
    {
        private DynamicParameters fields;
        public DynamicParameters Values
        {
            get
            {
                return fields;
            }
        }
        public DynamicFields()
        {
            fields = new DynamicParameters();
        }


        /// <summary>
        /// 添加动态字段
        /// </summary>
        /// <param name="name">@字段名</param>
        /// <param name="value">字段值</param>
        /// <param name="dbType">字段类型</param>
        /// <param name="direction">输入字段还是输出字段</param>
        public void Add(string name, object value, DbType? dbType = null, ParameterDirection? direction = null)
        {
            fields.Add(name, value, dbType, direction);
        }


        /// <summary>
        /// 得到字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            return fields.Get<T>(name);
        }

    }


    /// <summary>
    /// 处理死锁的方式
    /// </summary>
    public enum HintType
    {
        /// <summary>
        /// 死锁状态查询不出任何数据
        /// </summary>
        Default,
        /// <summary>
        /// 不管是否被锁住，都查询出数据
        /// </summary>
        With_Nolock,
        /// <summary>
        /// 死锁状态查询不出任何数据
        /// </summary>
        With_Readpast
    }


    #region 批量操作类
    /// <summary>
    /// 批量处理的操作信息
    /// </summary>
    public class BatchOperationInfo
    {
        /// <summary>
        /// 自动生成 从1开始的主键
        /// </summary>
        public int ID
        {
            set; get;
        }

        /// <summary>
        /// 自增主键ID 只对插入方法有效
        /// </summary>
        public long? IDENTITY1
        {
            set; get;
        }
        /// <summary>
        /// 拼接的操作语句（条件-部分） eg  delete ... where field1=A And field2=A  update ... where ield1=A And field2=A
        /// </summary>
        public string Operationwhere
        {
            set; get;
        }

        /// <summary>
        ///  拼接的操作语句（set-部分） eg: insert into values(field1,field2)  update table set field1=A,field2=A
        /// </summary>
        public string Operationset
        {
            set; get;
        }
        /// <summary>
        ///  拼接的操作语句（全部）
        /// </summary>
        public string Operationstr
        {
            set; get;
        }


        /// <summary>
        /// 实体对象
        /// </summary>
        public object Entity
        {
            set; get;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Errormsg
        {
            set; get;
        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success
        {
            set; get;
        }

    }


    /// <summary>
    /// 批量操作返回结果
    /// </summary>
    public class BatchOperationReturnInfo
    {
        public BatchOperationReturnInfo(List<BatchOperationInfo> _batchOperationInfo)
        {
            identity1 = _batchOperationInfo.Where(b => b.Success == true).Select(b => b.IDENTITY1).ToList();
            aLLInfo = _batchOperationInfo;
            successInfo = _batchOperationInfo.Where(b => b.Success == true).ToList();
            failureInfo = _batchOperationInfo.Where(b => b.Success == false).ToList();
        }
        /// <summary>
        /// 自增主键ID
        /// </summary>
        public List<long?> IDENTITY1
        {
            get => identity1; set => identity1 = value;
        }
        private List<long?> identity1;
        private List<BatchOperationInfo> successInfo;
        private List<BatchOperationInfo> failureInfo;
        private List<BatchOperationInfo> aLLInfo;
        /// <summary>
        /// 成功对象信息
        /// </summary>
        public List<BatchOperationInfo> SuccessInfo
        {
            get => successInfo; set => successInfo = value;
        }
        /// <summary>
        /// 失败对象信息
        /// </summary>
        public List<BatchOperationInfo> FailureInfo
        {
            get => failureInfo; set => failureInfo = value;
        }
        /// <summary>
        /// 所有对象信息
        /// </summary>
        public List<BatchOperationInfo> ALLInfo
        {
            get => aLLInfo; set => aLLInfo = value;
        }

    }

    #endregion

    #region Attributes

    /// <summary>
    /// 数据表属性类
    /// </summary>
    public class DataTableAttrbute : Attribute
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string TableName
        {
            get; set;
        }


        public DataTableAttrbute(string _tableName)
        {
            TableName = _tableName;
        }

    }


    /// <summary>
    /// 主键属性类
    /// </summary>
    public class PrimaryKeyAttrbute : Attribute
    {
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIncrement
        {
            get; set;
        }


        public PrimaryKeyAttrbute(bool _isIncrement = false)
        {
            IsIncrement = _isIncrement;
        }

    }


    /// <summary>
    /// 非数据字段属性类
    /// </summary>
    public class NonDataFieldAttrbute : Attribute
    {
    }

    #endregion

}
