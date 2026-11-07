using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOGIN_C
{
    internal class Connection
    {
        private static string stringConnection = @"Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
                                               + "Initial Catalog=VIET_SPORT;"
                                               + "Integrated Security=True;"
                                               + "Encrypt=True;"
                                               + "TrustServerCertificate=True";
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(stringConnection);
        }
    }
}
