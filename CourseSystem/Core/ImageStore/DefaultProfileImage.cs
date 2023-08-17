using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ImageStore
{
    public static class DefaultProfileImage
    {
        private static (string, string) _avatar1 =("avatar_1", "https://dl.dropboxusercontent.com/scl/fi/45w51dd6noyokzqpnk9vt/avatar_1.jpg?rlkey=44au6vpoeosz8n9fg9znf936b&dl=0");

        public static (string, string) GetDefaultImageUrl()
        {
            //randomizer logic getting
            return _avatar1;
        }
    }
}
