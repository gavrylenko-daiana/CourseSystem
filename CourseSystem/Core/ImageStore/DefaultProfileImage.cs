using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ImageStore
{
    public static class DefaultProfileImage
    {
        private const string _avatar1 = "https://www.dropbox.com/scl/fi/45w51dd6noyokzqpnk9vt/avatar_1.jpg?rlkey=44au6vpoeosz8n9fg9znf936b&dl=0";

        public static string GetDefaultImageUrl()
        {
            //randomizer logic getting
            return _avatar1;
        }
    }
}
