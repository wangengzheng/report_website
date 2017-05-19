using AVGD.Common;
using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.DAL
{

    public class LinkDALController
    {
        public DbReport _db = null;


        public LinkDALController(DbReport db)
        {
            _db = db;
        }

        #region ----   获取 DataTable   ----
        public DataTable GetDataTable(string sqlValue, /*
                                                                 * MySqlParameter[] dic*/Array dic=null)
        {
            DataTable dt = new DataTable();        
            using (DbReport context = new DbReport())
            {
                try
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sqlValue;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    if (dic != null && dic.Length > 0)
                    {
                        cmd.Parameters.AddRange(dic);
                    }
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    BugLog.Write(ex.ToString());
                    context.Database.Connection.Close();
                }
            }

            return dt;

        }

        #endregion


    }
}