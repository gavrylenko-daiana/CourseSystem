using Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICourseBackgroundImageService : IGenericService<CourseBackgroundImage>
    {
        Task<Result<bool>> DeleteCourseBackgroundImage(Course course);
        Task<Result<bool>> UpdateBackgroundImage(Course course, IFormFile newCourseBackground);
        Task<Result<bool>> SetDefaultBackgroundImage(Course course);
        Task<Result<bool>> SetCustomBackgroundImage(Course course, IFormFile courseBackground);
        Task<Result<bool>> HasBackgroundImage(Course course);
    }
}
