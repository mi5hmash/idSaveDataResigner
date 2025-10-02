using idSaveDataResignerCore.Infrastructure;

namespace idSaveDataResignerCore.GamingPlatforms;

public class Gog : IGamingPlatform
{
    public const string StoreBaseUrl = "https://www.gog.com/game";
    public void OpenStoreProductPage(string appId) => $"{StoreBaseUrl}/{appId}".OpenUrl();
}