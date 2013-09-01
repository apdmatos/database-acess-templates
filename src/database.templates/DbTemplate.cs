using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using database.templates;

namespace database.templates
{
    public static class DbTemplate<T>
    {
        private static DbCommand CreateCommand(DbConnection conn, String commandText, CommandType cmdType, int? timeout)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = cmdType;
            command.Connection = conn;

            if (timeout.HasValue) command.CommandTimeout = timeout.Value;

            return command;
        }

        private static void AddDbParameters2Command(IEnumerable<DbTemplateParameter> parameters, DbCommand command)
        {
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    param.ToDbParameter(command);
                }
            }
        }


        private static class DbExecutor<V>
        {
            private delegate void StatementPrepared(DbCommand command);
            private static void PrepareStatement(DbConnection connection, StatementPrepared prepared, string cmdText, CommandType commandType, IEnumerable<DbTemplateParameter> parameters, int? timeout)
            {
                using (DbCommand command = CreateCommand(connection, cmdText, commandType, timeout))
                {
                    AddDbParameters2Command(parameters, command);
                    if(connection.State != ConnectionState.Open)
                        connection.Open();

                    prepared(command);
                }

            }

            public delegate V ConstructFromReader(IDataReader reader);
            public static V GetSQLObject(DbConnection connection, ConstructFromReader builder, string cmdText, CommandType commandType, IEnumerable<DbTemplateParameter> parameters, int? timeout) 
            {
                V v = default(V);
                PrepareStatement(
                    connection, 
                    (command) => {
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            v = builder(reader);
                            reader.Close();
                        }
                    }, 
                    cmdText, 
                    commandType,
                    parameters,
                    timeout);

                return v;
            }

            public static bool ExecuteSql(DbConnection connection, string cmdText, CommandType commandType, IEnumerable<DbTemplateParameter> parameters, int? timeout) 
            {
                bool executed = false;
                PrepareStatement(
                    connection, 
                    (command) =>
                    {
                        command.ExecuteNonQuery();
                        executed = true;
                    },
                    cmdText,
                    commandType,
                    parameters,
                    timeout);

                return executed;
            }
        }

        private static T GetObject(DbConnection connection, ConstructObject constructor, string cmdText, CommandType commandType, IEnumerable<DbTemplateParameter> parameters, int? timeout)
        {
            return DbExecutor<T>.GetSQLObject(
                        connection, 
                        (reader) =>
                        {
                            T t = default(T);
                            if (reader.Read())
                            {
                                t = constructor(reader);
                            }

                            return t;
                        }, 
                        cmdText, 
                        commandType, 
                        parameters, 
                        timeout);
        }

        private static IEnumerable<T> GetObjectList(DbConnection connection, ConstructObject constructor, string cmdText, CommandType commandType, IEnumerable<DbTemplateParameter> parameters, int? timeout)
        {

            return DbExecutor<List<T>>.GetSQLObject(
                        connection, 
                        (reader) =>
                        {
                            List<T> t_list = new List<T>();

                            while (reader.Read())
                            {
                                T t = constructor(reader);
                                t_list.Add(t);
                            }

                            return t_list;
                        },
                        cmdText,
                        commandType,
                        parameters,
                        timeout);
        }

        public delegate T ConstructObject(IDataReader reader);

        public static T GetObjectBySQLQuery(DbConnection conn, ConstructObject constructor, string query, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObject(conn, constructor, query, CommandType.Text, parameters, timeout);
        }

        public static T GetObjectByProcedure(DbConnection conn, ConstructObject constructor, string procedure, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObject(conn, constructor, procedure, CommandType.StoredProcedure, parameters, timeout);
        }

        public static IEnumerable<T> GetListByProcedure(DbConnection conn, ConstructObject constructor, string procedure, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObjectList(conn, constructor, procedure, CommandType.StoredProcedure, parameters, timeout);
        }

        public static IEnumerable<T> GetListBySQLQuery(DbConnection conn, ConstructObject constructor, string query, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObjectList(conn, constructor, query, CommandType.Text, parameters, timeout);
        }

        public static IEnumerable<T> GetValuesBySQLQuery(DbConnection conn, string query, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObjectList(
                conn,
                (reader) => 
                { 
                    return (T)reader[0]; 
                }, 
                query,
                CommandType.Text, 
                parameters, 
                timeout);
        }

        public static IEnumerable<T> GetValuesByProcedure(DbConnection conn, string procedure, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObjectList(
                conn,
                (reader) =>
                {
                    return (T)reader[0];
                },
                procedure,
                CommandType.StoredProcedure,
                parameters,
                timeout);
        }

        // Get value
        public static T GetValueBySQLQuery(DbConnection conn, string query, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObject(
                conn,
                (reader) =>
                {
                    return (T)reader[0];
                },
                query,
                CommandType.Text,
                parameters,
                timeout);
        }

        public static T GetValueByProcedure(DbConnection conn, string procedure, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return GetObject(
                conn,
                (reader) =>
                {
                    return (T)reader[0];
                },
                procedure,
                CommandType.StoredProcedure,
                parameters,
                timeout);
        }

        public static bool ExecuteSQL(DbConnection conn, string query, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return DbExecutor<bool>.ExecuteSql(conn, query, CommandType.Text, parameters, timeout);
        }

        public static bool ExecuteProcedure(DbConnection conn, string procedure, IEnumerable<DbTemplateParameter> parameters, int? timeout = null)
        {
            return DbExecutor<bool>.ExecuteSql(conn, procedure, CommandType.StoredProcedure, parameters, timeout);
        }
    }
}
