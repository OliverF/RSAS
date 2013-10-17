using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.ServerSide
{
    class User
    {
        private User()
        {
            
        }

        public static User CreateFromUsername(string username)
        {
            return new User();
        }
    }
}
