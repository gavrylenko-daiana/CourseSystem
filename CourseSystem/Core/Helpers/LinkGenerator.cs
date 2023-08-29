using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Core.Helpers;

public static class LinkGenerator
{
    public static string GenerateCourseLink(IUrlHelperFactory urlHelperFactory, ControllerBase controller, Course course)
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(controller.ControllerContext);

        return urlHelper.Action(
            "Details",
            "Course",
            new
            {
                id = course.Id
            },
            protocol: controller.Request.Scheme);
    }
    
    public static string GenerateGroupLink(IUrlHelperFactory urlHelperFactory, ControllerBase controller, Course course)
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(controller.ControllerContext);

        return urlHelper.Action(
            "Details",
            "Course",
            new
            {
                id = course.Id
            },
            protocol: controller.Request.Scheme);
    }
}