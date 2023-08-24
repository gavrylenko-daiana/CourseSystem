using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ImageStore
{
    public class DefaultProfileImage
    {
        private static Dictionary<string, string> _avatarsPack = new Dictionary<string, string>()
        {
            {"avatar_1.jpg", "https://dl.dropboxusercontent.com/scl/fi/45w51dd6noyokzqpnk9vt/avatar_1.jpg?rlkey=44au6vpoeosz8n9fg9znf936b&dl=0" },
            {"avatar_2.png", "https://dl.dropboxusercontent.com/scl/fi/u52xviwlfzx31qytn4nwo/avatar_2.png?rlkey=iahvjnzxkwyhr6175gqv1cj8d&dl=0" },
            {"avatar_3.png", "https://dl.dropboxusercontent.com/scl/fi/zu09hkdk72klsmp947eyh/avatar_3.png?rlkey=e90mn2t0o07xcvq1lozwpi6yb&dl=0" },
            {"avatar_4.png", "https://dl.dropboxusercontent.com/scl/fi/gqxr25giw793ztq9yy2se/avatar_4.png?rlkey=p8oly8phv8i1q0hdupy57cpv6&dl=0" },
            {"avatar_5.png", "https://dl.dropboxusercontent.com/scl/fi/e87myzftdgfohbw89uyvi/avatar_5.png?rlkey=6b6c5x9k2otfms2oc0wu0zoel&dl=0" }
        };

        private static Random _random = new Random();
        public static (string, string) GetDefaultImageUrl(string? exceptName = null, Dictionary<string, string>? dynemicAvatarsPack = null)
        {
            var resultPack = dynemicAvatarsPack ?? _avatarsPack;

            var index = _random.Next(0, resultPack.Keys.Count);

            
            if (exceptName != null)
            {
                var fileExtension = Path.GetExtension(exceptName);
                string newExceptName;

                if (fileExtension == null || fileExtension.Equals(String.Empty))
                {
                    if (exceptName.Equals("avatar_1"))
                    {
                        newExceptName = exceptName + ".jpg";
                    }
                    else
                    {
                        newExceptName = exceptName + ".png";
                    }
                }
                else
                {
                    newExceptName = exceptName;
                }

                var keyArrayExcept = resultPack.Keys.ToList();
                keyArrayExcept.Remove(newExceptName);
                var indexExcept = _random.Next(0, keyArrayExcept.Count);
                var imageNameExecpt = keyArrayExcept[indexExcept];

                return (imageNameExecpt, resultPack[imageNameExecpt]);
            }

            var keyArray = resultPack.Keys.ToArray();
            var imageName = keyArray[index];

            return (imageName, resultPack[imageName]);
        }

        public static bool IsProfileImageDefault(string imageUrl, Dictionary<string, string>? dynemicAvatarsPack = null)
        {
            var resultPack = dynemicAvatarsPack ?? _avatarsPack;

            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            return resultPack.ContainsValue(imageUrl);
        }
    }
}
