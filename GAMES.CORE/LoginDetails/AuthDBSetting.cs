using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAMES.CORE.LoginDetails
{
    public class AuthDBSettings:IAuthDBSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string UsersCollectionName { get; set; } = "Users";
    }
}