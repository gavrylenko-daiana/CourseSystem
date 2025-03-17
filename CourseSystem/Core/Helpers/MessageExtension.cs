namespace Core.Helpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class MessageExtension
{
    public static void TempDataMessage(this ITempDataDictionary tempData, string key, string message)
    {
        tempData[key] = message;
    }

    public static void ViewDataMessage(this ViewDataDictionary viewData, string key, string message)
    {
        viewData[key] = message;
    }
}