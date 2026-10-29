using System.Configuration;
using System.Data.SqlClient;

namespace TravelHub.Data
{
    public static class TravelDb
    {
        public static SqlConnection Open()
        {
            var cs = ConfigurationManager.ConnectionStrings["TravelHub"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new ConfigurationErrorsException("Thiếu connectionStrings:TravelHub trong Web.config.");
            var conn = new SqlConnection(cs.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
