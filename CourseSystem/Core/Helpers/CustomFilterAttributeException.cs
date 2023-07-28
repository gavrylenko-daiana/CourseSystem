using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Core.Helpers;

public class CustomFilterAttributeException : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        context.ExceptionHandled = true;
        
        context.HttpContext.Items["Error"] = exception.Message;

        //Redirection to model performance
        context.Result = new ViewResult
        {
            ViewName = "Error",
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
        };
    }
}