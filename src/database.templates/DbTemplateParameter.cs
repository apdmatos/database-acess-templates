using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace database.templates
{
    public class DbTemplateParameter
    {
        private ParameterDirection _direction = ParameterDirection.Input;

        public DbType DbType { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }


        private IDataParameter _dbParameter;
        public IDataParameter Parameter { 
            get {
                return _dbParameter;
            } 
        }

        public DbTemplateParameter() { }
        public DbTemplateParameter(DbType type, string name, object value) 
            : this(type, name, value, ParameterDirection.Input) { }

        public DbTemplateParameter(DbType type, string name, object value, ParameterDirection direction) 
        {
            DbType = type;
            Name = name;
            Value = value;
            Direction = direction;
        }

        public IDataParameter ToDbParameter(DbCommand command)
        {

            if (_dbParameter == null)
            {
                DbParameter param = command.CreateParameter();
                param.ParameterName = Name;
                param.Value = Value;
                param.DbType = DbType;
                param.Direction = Direction;

                command.Parameters.Add(param);

                _dbParameter = param;
            }


            return _dbParameter;
        }
    }
}
