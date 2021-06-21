using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace MirrorShield
{
    class DBManagement
    {
        private DataSet ds;
        private SqlDataAdapter adapter;

        public DBManagement()
        {
            //Constractor here. THERE IS NO NEED TO GET/SET HERE ANY ARGUMENTS
        }


        private string ConnStr()
        {
            // To change the database path you need to rewrite the AttachDbFilename and then put your database link
            string conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\owner\Desktop\Moshe\MSDatabase.mdf;Integrated Security=True;Connect Timeout=30";
            return conStr;
        }

        /////////////////////////////////////////////////////////////////////////////

        // DataSet פעולה המקבלת בקשה ומחזירה
        private DataSet GetDataSet(string strSql)
        {
            ds = new DataSet();
            //שלב ראשון: הגדרת צורת ההתחברות למסד הנתונים 
            SqlConnection connection = new SqlConnection(ConnStr());
            //שלב שני: טעינת הנתונים ממסד הנתונים לזיכרון 
            adapter = new SqlDataAdapter(strSql, connection);
            connection.Open();
            adapter.Fill(ds);
            //שלב שלישי: התנתקות ממסד הנתונים 
            connection.Close();
            return ds;
        }
        public void Insert(string user, string psw, string mail)
        {
            string query = string.Format("select * from Userstbl where UserName = '{0}'", user);
            ds = GetDataSet(query);
            DataRow dr = ds.Tables[0].NewRow();
            dr["UserName"] = user;
            dr["Password"] = psw;
            dr["Email"] = mail;
            ds.Tables[0].Rows.Add(dr);
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.Update(ds);
        }
        public bool Islogin(string user, string psw)
        {
            string query = string.Format("select * from Userstbl where UserName = '{0}'", user);
            ds = GetDataSet(query);
            int count = ds.Tables[0].Rows.Count;
            if (count == 0)
                return false;
            if (ds.Tables[0].Rows[0]["Password"].ToString() == psw)
                return true;
            return false;
        }
    }
}
