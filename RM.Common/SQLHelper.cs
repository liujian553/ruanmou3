using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace Microsoft.WorkflowServicesPlatform.Utility
{
    public class SQLHelper
    {
        private static Database _db = DatabaseFactory.CreateDatabase("DBConnectionStringWrite");

        public static string DBName
        {
            get {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_db.ConnectionString);
                return builder["database"] as string;
                //return "WorkflowServicePlatformDB"; 
            }
        }

        public static string CONN_STRING
        {
            get { return _db.ConnectionString;}
        }

        public static SqlConnection CreateConnection(string connStr)
        {
            return new SqlDatabase(connStr).CreateConnection() as SqlConnection;
        }

        public static object ExecuteScalarSQL(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetSqlStringCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteScalar(dbCommand);
        }

        public static object ExecuteScalarSP(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetStoredProcCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteScalar(dbCommand);
        }

        public static int ExecuteNonQuerySQL(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetSqlStringCommand(sql);
           
            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteNonQuery(dbCommand);
        }

        public static object ExecuteNonQuerySP(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetStoredProcCommand(sql);
           
            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            object obj = _db.ExecuteNonQuery(dbCommand);
            dbCommand.Parameters.Clear();
            return obj;
        }

        public static DataTable ExecuteDataTableSQL(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetSqlStringCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            DataTable dt = _db.ExecuteDataSet(dbCommand).Tables[0];
            dbCommand.Parameters.Clear();
            return dt;
        }

        public static DataTable ExecuteDataTableSP(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetStoredProcCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteDataSet(dbCommand).Tables[0];
        }

        public static DataSet ExecuteDataSetSQL(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetSqlStringCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteDataSet(dbCommand);
        }

        public static DataSet ExecuteDataSetSP(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetStoredProcCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteDataSet(dbCommand);
        }



        public static IDataReader ExecuteReaderSQL(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetSqlStringCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteReader(dbCommand);
        }


        public static IDataReader ExecuteReaderSP(string sql, SqlParameter[] sqlParams)
        {
            DbCommand dbCommand = _db.GetStoredProcCommand(sql);

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                    dbCommand.Parameters.Add(sqlParam);
            }

            return _db.ExecuteReader(dbCommand);


            //return _db.ExecuteReader(dbCommand) as SqlDataReader;
        }
    }
}
