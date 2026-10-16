using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace QuanLyBanHangSupermarket
{
    public static class DatabaseHelper
    {
        public static string ConnectionString
        {
            get
            {
                var setting = ConfigurationManager.ConnectionStrings["ConnStr"];
                if (setting == null) throw new InvalidOperationException("Thiếu cấu hình ConnStr trong App.config.");
                return setting.ConnectionString;
            }
        }
        public static SqlConnection GetConnection() { return new SqlConnection(ConnectionString); }
        public static SqlParameter Param(string name, object value)
        {
            var p = new SqlParameter(name, value ?? DBNull.Value);
            if (value is decimal) { p.SqlDbType = SqlDbType.Decimal; p.Precision = 18; p.Scale = 2; }
            return p;
        }
        public static DataTable GetDataToTable(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            { cmd.Parameters.AddRange(parameters); var table = new DataTable(); adapter.Fill(table); return table; }
        }
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            { cmd.Parameters.AddRange(parameters); conn.Open(); return cmd.ExecuteNonQuery(); }
        }
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            { cmd.Parameters.AddRange(parameters); conn.Open(); return cmd.ExecuteScalar(); }
        }
        public static T InTransaction<T>(Func<SqlConnection, SqlTransaction, T> operation)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try { T result = operation(conn, tx); tx.Commit(); return result; }
                    catch { try { tx.Rollback(); } catch (InvalidOperationException) { } throw; }
                }
            }
        }
        public static int Execute(SqlConnection conn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            { cmd.Parameters.AddRange(parameters); return cmd.ExecuteNonQuery(); }
        }
        public static object Scalar(SqlConnection conn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            { cmd.Parameters.AddRange(parameters); return cmd.ExecuteScalar(); }
        }
    }
}