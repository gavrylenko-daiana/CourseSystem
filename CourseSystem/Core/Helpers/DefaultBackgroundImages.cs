using Core.Models;

namespace Core.Helpers;

public static class DefaultBackgroundImages
{
    public static List<string> GetDefaultImagesLinks()
    {
        List<string> defaultImagesLinks = new List<string>
        {
            "https://www.dropbox.com/scl/fi/u318svdoeo9mkfbdzf6kl/default-1.png?rlkey=0ustmj3lz8752t6yq03gzaet6&raw=1",
            "https://www.dropbox.com/scl/fi/q69in6su1xo5p5awx4zg3/default-2.png?rlkey=6td6c23r6f7wjvrrbmj1by7mo&raw=1",
            "https://www.dropbox.com/scl/fi/ezuqqrqjfhd7m2d04k8vk/default-3.png?rlkey=iw3se3ffux2vv79qyv045yf2r&raw=1",
            "https://www.dropbox.com/scl/fi/6f74i3t85as6zrpjklkn7/default-4.png?rlkey=qwrrn13d1oz5npi8ua5y488qe&raw=1"
        };

        return defaultImagesLinks;
    }
}