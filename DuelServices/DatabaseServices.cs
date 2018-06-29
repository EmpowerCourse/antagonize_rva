using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DuelServices
{
    public class DatabaseServices
    {
        public bool IsAuthorizedUser(string probeUsername)
        {
            bool hasAccess = false;
            var cnStr = ConfigurationManager.AppSettings["ConnectionString"];
            using (SqlConnection cn = new SqlConnection(cnStr))
            {
                cn.Open();
                string sql = "dbo.spIsPermittedUser";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Username", probeUsername));
                    cmd.Parameters.Add(new SqlParameter("@IsValid", SqlDbType.Bit, 1) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    hasAccess = (bool)cmd.Parameters["@IsValid"].Value;
                }
                cn.Close();
            }
            return hasAccess;
        }

        public List<Participant> GetParticipants(TypeOfGender? gender = null)
        {
            List<Participant> result = new List<Participant>();
            var cityFilter = ConfigurationManager.AppSettings["TargetCity"];
            var cnStr = ConfigurationManager.AppSettings["ConnectionString"];
            using (SqlConnection cn = new SqlConnection(cnStr))
            {
                cn.Open();
                string sql = "SELECT Id, FirstName, LastName, Email, Phone, Gender FROM dbo.Participant WHERE City = @City";
                // horrible really, but this type of thing is why we have an ORM coming soon...
                if (gender.HasValue)
                {
                    sql += " AND Gender = '" + gender.Value.ToString().Substring(0, 1) + "'";
                }
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@City", cityFilter));
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new Participant()
                            {
                                Id = (int)dr["Id"],
                                FirstName = (string)dr["FirstName"],
                                LastName = (string)dr["LastName"],
                                Email = (string)dr["Email"],
                                Phone = (string)dr["Phone"],
                                Gender = (string)dr["Gender"] == "F"
                                    ? TypeOfGender.Female
                                    : TypeOfGender.Male
                            });
                        }
                    }
                }
                cn.Close();
            }
            return result;
        }

    }
}
