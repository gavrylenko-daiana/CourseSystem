using Core.Models;

namespace Core.ImageStore;

public static class DefaultBackgroundImages
{
    private static Dictionary<string, string> _avatarsPack = new Dictionary<string, string>()
        {
            {"default-1.png", "https://dl.dropboxusercontent.com/scl/fi/u318svdoeo9mkfbdzf6kl/default-1.png?rlkey=0ustmj3lz8752t6yq03gzaet6&raw=1" },
            {"default-2.png", "https://dl.dropboxusercontent.com/scl/fi/q69in6su1xo5p5awx4zg3/default-2.png?rlkey=6td6c23r6f7wjvrrbmj1by7mo&raw=1" },
            {"default-3.png", "https://dl.dropboxusercontent.com/scl/fi/ezuqqrqjfhd7m2d04k8vk/default-3.png?rlkey=iw3se3ffux2vv79qyv045yf2r&raw=1" },
            {"default-4.png", "https://dl.dropboxusercontent.com/scl/fi/6f74i3t85as6zrpjklkn7/default-4.png?rlkey=qwrrn13d1oz5npi8ua5y488qe&raw=1" }
        };

    private static Random _random = new Random();

    public static (string, string) GetRandomDefaultImage()
    {
        var index = _random.Next(0, _avatarsPack.Keys.Count);

        var imageName = _avatarsPack.Keys.ToArray()[index];

        return (imageName, _avatarsPack[imageName]);
    }

    public static bool IsDefaultBackgroundImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return false;
        }

        return _avatarsPack.ContainsValue(imageUrl);
    }
}