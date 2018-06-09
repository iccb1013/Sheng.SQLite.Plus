
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheng.SQLite.Plus
{
    /*
     * 配合 Sheng.SQLite.Plus 对数据库进行操作
     * 
     */

    public class DatabaseWrapper
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SQLiteConnection _connection;

        public DatabaseWrapper(string connectionString)
        {
            _log.Info(connectionString);
            _connection = new SQLiteConnection(connectionString);
        }

        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, null);
        }

        public int ExecuteNonQuery(string commandText, List<CommandParameter> parameterList)
        {
            try
            {
                _connection.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(commandText, _connection))
                {
                    if (parameterList != null && parameterList.Count > 0)
                    {
                        foreach (CommandParameter item in parameterList)
                        {
                            DbParameter parameter = cmd.CreateParameter();
                            parameter.ParameterName = item.ParameterName;
                            parameter.Value = item.Value;
                            cmd.Parameters.Add(parameter);
                        }
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                string logMessage = commandText;
                if (parameterList != null)
                {
                    logMessage += Environment.NewLine + JsonConvert.SerializeObject(parameterList);
                }
                _log.Error(logMessage, exception);

                throw exception;
            }
            finally
            {
                _connection.Close();
            }
        }

        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(commandText, null);
        }

        public bool ExecuteScalar<T>(string commandText, Action<T> callback)
        {
            return ExecuteScalar<T>(commandText, null, callback);
        }

        public object ExecuteScalar(string commandText, List<CommandParameter> parameterList)
        {
            try
            {
                _connection.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(commandText, _connection))
                {
                    foreach (CommandParameter item in parameterList)
                    {
                        DbParameter parameter = cmd.CreateParameter();
                        parameter.ParameterName = item.ParameterName;
                        parameter.Value = item.Value;
                        cmd.Parameters.Add(parameter);
                    }

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception exception)
            {
                string logMessage = commandText;
                if (parameterList != null)
                {
                    logMessage += Environment.NewLine + JsonConvert.SerializeObject(parameterList);
                }
                _log.Error(logMessage, exception);

                throw exception;
            }
            finally
            {
                _connection.Close();
            }
        }

        public bool ExecuteScalar<T>(string commandText, List<CommandParameter> parameterList,Action<T> callback)
        {
            object scalarValue = ExecuteScalar(commandText, parameterList);
            if (scalarValue == null || scalarValue == DBNull.Value)
                return false;
            else
            {
                if (scalarValue.GetType() == typeof(byte) && typeof(T) == typeof(int))
                {
                    scalarValue = Convert.ToInt32(scalarValue);
                }
                else if (typeof(T) == typeof(int))
                {
                    scalarValue = Convert.ToInt32(scalarValue);
                }

                callback((T)scalarValue);
                return true;
            }
        }

        public DataSet ExecuteDataSet(string commandText)
        {
            return ExecuteDataSet(commandText, new[] { "Table" });
        }

        public DataSet ExecuteDataSet(string commandText, string[] tableNameArray)
        {
            return ExecuteDataSet(commandText, null, tableNameArray);
        }

        public DataSet ExecuteDataSet(string commandText,
            List<CommandParameter> parameterList, string[] tableNameArray)
        {
            SQLiteCommand cmd = null;
            SQLiteDataAdapter adapter = null;

            try
            {
                _connection.Open();

                cmd = new SQLiteCommand(commandText, _connection);

                foreach (CommandParameter item in parameterList)
                {
                    DbParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = item.ParameterName;
                    parameter.Value = item.Value;
                    cmd.Parameters.Add(parameter);
                }

                adapter = new SQLiteDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds != null && ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        if (tableNameArray.Length <= i)
                            break;

                        ds.Tables[i].TableName = tableNameArray[i];
                    }
                }
                return ds;

            }
            catch (Exception exception)
            {
                string logMessage = commandText;
                if (parameterList != null)
                {
                    logMessage += Environment.NewLine + JsonConvert.SerializeObject(parameterList);
                }
                _log.Error(logMessage, exception);

                throw exception;
            }
            finally
            {
                _connection.Close();

                if (cmd != null)
                    cmd.Dispose();

                if (adapter != null)
                    adapter.Dispose();
            }
        }

        /// <summary>
        /// 返回受影响的行数
        /// </summary>
        /// <param name="sqlExpression"></param>
        public int ExcuteSqlExpression(SqlExpression sqlExpression)
        {
            try
            {
                _connection.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(sqlExpression.Sql, _connection))
                {
                    cmd.Parameters.AddRange(sqlExpression.ParameterList.ToArray());
                    return cmd.ExecuteNonQuery(); 
                }
            }
            catch (Exception exception)
            {
                string logMessage = sqlExpression.Sql;
                if (sqlExpression.ParameterList != null)
                {
                    logMessage += Environment.NewLine + JsonConvert.SerializeObject(sqlExpression.ParameterList);
                }
                _log.Error(logMessage, exception);

                throw exception;
            }
            finally
            {
                _connection.Close();
            }
        }

        public void ExcuteSqlExpression(List<SqlExpression> sqlExpressionList)
        {
            SQLiteTransaction transaction = null;
            try
            {
                _connection.Open();
                transaction = _connection.BeginTransaction();

                foreach (var item in sqlExpressionList)
                {
                    try
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(item.Sql, _connection))
                        {
                            cmd.Parameters.AddRange(item.ParameterList.ToArray());
                            cmd.Transaction = transaction;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (transaction != null)
                            transaction.Rollback();

                        string logMessage = item.Sql;
                        if (item.ParameterList != null)
                        {
                            logMessage += Environment.NewLine + JsonConvert.SerializeObject(item.ParameterList);
                        }
                        _log.Error(logMessage, exception);

                        throw exception;
                    }
                }

                transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
            }
            finally
            {
                _connection.Close();
            }
        }

        private DataSet ExcuteDataSetSqlExpression(SqlExpression sqlExpression)
        {
            return ExecuteDataSet(sqlExpression.Sql,
                GetCommandParameterList(sqlExpression.ParameterList), new string[] { "Table" });
        }

        private List<CommandParameter> GetCommandParameterList(List<SQLiteParameter> list)
        {
            List<CommandParameter> resultList = new List<CommandParameter>();

            if (list != null)
            {
                foreach (var item in list)
                {
                    CommandParameter cmd = new CommandParameter();
                    cmd.ParameterName = item.ParameterName;
                    cmd.Value = item.Value;

                    resultList.Add(cmd);
                }
            }

            return resultList;
        }

        /// <summary>
        /// 填充一个对象，根据对象上有keyAttribute的属性生成where
        /// 如果取出的数据集不是唯一的一条记录，则无法填充
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public bool Fill<T>(object obj) where T : class,new()
        {
            return Fill<T>(obj, null, null);
        }

        public bool Fill<T>(object obj, Dictionary<string, object> attachedWhere) where T : class,new()
        {
            return Fill<T>(obj, null, null);
        }

        public bool Fill<T>(object obj, string table) where T : class,new()
        {
            return Fill<T>(obj, table, null);
        }

        public bool Fill<T>(object obj, string table, Dictionary<string, object> attachedWhere) where T : class,new()
        {
            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Table = table;
            args.Type = SqlExpressionType.Select;
            if (attachedWhere == null)
            {
                args.GenerateWhere = true;
            }
            else
            {
                args.GenerateWhere = false;
                args.AttachedWhere = AttachedWhereItem.Parse(attachedWhere);
            }

            //不能用 default(T) ，会是null
            SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(obj, args);

            DataSet ds = ExcuteDataSetSqlExpression(sqlExpression);
            List<T> dataList = RelationalMappingUnity.Select<T>(ds.Tables[0]);

            Debug.Assert(dataList.Count <= 1, "Fill 时取出的记录大于1条");

            if (dataList.Count != 1)
            {
                return false;
            }

            T dataObj = dataList[0];

            ReflectionHelper.Inject(obj, dataObj);

            return true;
        }


        public List<T> Select<T>() where T : class,new()
        {
            return Select<T>(new Dictionary<string, object>(), null);
        }

        public List<T> Select<T>(Dictionary<string, object> attachedWhere) where T : class,new()
        {
            return Select<T>(AttachedWhereItem.Parse(attachedWhere), null);
        }

        public List<T> Select<T>(Dictionary<string, object> attachedWhere, SqlExpressionPagingArgs pagingArgs) where T : class,new()
        {
            return Select<T>(AttachedWhereItem.Parse(attachedWhere), pagingArgs);
        }

        public List<T> Select<T>(List<AttachedWhereItem> attachedWhere) where T : class,new()
        {
            return Select<T>(attachedWhere, null);
        }

        public List<T> Select<T>(SqlExpressionPagingArgs pagingArgs) where T : class,new()
        {
            return Select<T>(new List<AttachedWhereItem>(), pagingArgs);
        }

        public List<T> Select<T>(List<AttachedWhereItem> attachedWhere, SqlExpressionPagingArgs pagingArgs) where T : class,new()
        {
            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Type = SqlExpressionType.Select;
            args.GenerateWhere = false;
            args.AttachedWhere = attachedWhere;
            args.PagingArgs = pagingArgs;

            //不能用 default(T) ，会是null
            SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(new T(), args);

            DataSet ds = ExcuteDataSetSqlExpression(sqlExpression);
            List<T> dataList = RelationalMappingUnity.Select<T>(ds.Tables[0]);

            if (pagingArgs != null)
            {
                if (ds.Tables.Count > 1)
                {
                    pagingArgs.TotalRow = int.Parse(ds.Tables[1].Rows[0][0].ToString());
                }
                else
                {
                    pagingArgs.TotalRow = ds.Tables[0].Rows.Count;
                }
                pagingArgs.TotalPage = pagingArgs.TotalRow / pagingArgs.PageSize;
                if (pagingArgs.TotalRow % pagingArgs.PageSize > 0)
                    pagingArgs.TotalPage++;
            }

            return dataList;
        }

        public List<T> Select<T>(string sql) where T : class
        {
            DataSet ds = ExecuteDataSet(sql);
            List<T> dataList = RelationalMappingUnity.Select<T>(ds.Tables[0]);
            return dataList;
        }

        public List<T> Select<T>(string sql, List<CommandParameter> parameterList) where T : class
        {
            DataSet ds = ExecuteDataSet(sql, parameterList, new string[] { "Table" });
            List<T> dataList = RelationalMappingUnity.Select<T>(ds.Tables[0]);
            return dataList;
        }

        /// <summary>
        /// 插入失败以异常形式抛出
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Insert(object obj)
        {
            if (obj == null)
                return false;

            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Type = SqlExpressionType.Insert;

            SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(obj, args);
            return ExcuteSqlExpression(sqlExpression) == 1;
        }

        /// <summary>
        /// 封装为一个事务进行写入
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="obj"></param>
        public void InsertList(object obj1, object obj2, params object[] obj)
        {
            List<object> objList = new List<object>();
            objList.Add(obj1);
            objList.Add(obj2);
            if (obj != null && obj.Length > 0)
            {
                foreach (var item in obj)
                {
                    objList.Add(item);
                }
            }
            InsertList(objList);
        }

        /// <summary>
        /// 封装为一个事务进行写入
        /// </summary>
        /// <param name="objList"></param>
        public void InsertList(List<object> objList)
        {
            if (objList == null)
                return;

            List<SqlExpression> sqlExpressionList = new List<SqlExpression>();

            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Type = SqlExpressionType.Insert;

            foreach (var item in objList)
            {
                if (item == null)
                {
                    Debug.Assert(false, "insert obj 为 null");
                    continue;
                }

                SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(item, args);
                sqlExpressionList.Add(sqlExpression);
            }

            ExcuteSqlExpression(sqlExpressionList);
        }

        public int Update(object obj)
        {
            return Update(obj, null);
        }

        public int Update(object obj, string table)
        {
            return Update(obj, table, null);
        }

        public int Update(object obj, string table, string excludeFields)
        {
            if (obj == null)
                return 0;

            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Table = table;
            args.Type = SqlExpressionType.Update;
            args.ExcludeFields = excludeFields;
            SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(obj, args);
            return ExcuteSqlExpression(sqlExpression);
        }

        public void UpdateList(object obj1, object obj2, params object[] obj)
        {
            List<object> objList = new List<object>();
            objList.Add(obj1);
            objList.Add(obj2);
            if (obj != null && obj.Length > 0)
            {
                foreach (var item in obj)
                {
                    objList.Add(item);
                }
            }
            UpdateList(objList);
        }

        public void UpdateList(List<object> objList)
        {
            if (objList == null)
                return;

            List<SqlExpression> sqlExpressionList = new List<SqlExpression>();

            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Type = SqlExpressionType.Update;

            foreach (var item in objList)
            {
                if (item == null)
                {
                    Debug.Assert(false, "insert obj 为 null");
                    continue;
                }

                SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(item, args);
                sqlExpressionList.Add(sqlExpression);
            }

            ExcuteSqlExpression(sqlExpressionList);
        }

        public int Remove(object obj)
        {
            if (obj == null)
                return 0;

            SqlExpressionArgs args = new SqlExpressionArgs();
            args.Type = SqlExpressionType.Delete;
            SqlExpression sqlExpression = RelationalMappingUnity.GetSqlExpression(obj, args);
            return ExcuteSqlExpression(sqlExpression);
        }

        public List<SQLiteParameter> CommandParameterToSqlParameter(List<CommandParameter> parameterList)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();

            if (parameterList == null || parameterList.Count == 0)
                return list;

            foreach (var item in parameterList)
            {
                SQLiteParameter sqlParameter = new SQLiteParameter(item.ParameterName, item.Value);
                list.Add(sqlParameter);
            }

            return list;
        }

       
    }
}
