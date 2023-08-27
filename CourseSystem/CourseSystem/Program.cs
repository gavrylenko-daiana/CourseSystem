using BLL.Interfaces;
using BLL.Services;
using Core.Configuration;
using Core.Models;
using DAL;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UI;
using Westwind.AspNetCore.Markdown;
using UI.Hubs;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using static Dropbox.Api.TeamLog.EventCategory;
using UI.Jobs;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger(); 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMarkdown();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IUserCourseService, UserCourseService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IAssignmentAnswerService, AssignmentAnswerService>();
builder.Services.AddScoped<IUserAssignmentService, UserAssignmentService>();
builder.Services.AddScoped<IEducationMaterialService, EducationMaterialService>();
builder.Services.AddScoped<IDropboxService, DropboxService>();
builder.Services.AddScoped<IProfileImageService, ProfileImageService>();
builder.Services.AddScoped<IAssignmentFileService, AssignmentFileService>();
builder.Services.AddScoped<ICourseBackgroundImageService, CourseBackgroundImageService>();
builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("DropboxSettings"));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<UpdateGroupAccessJob>(); //won't be needed if server is running constantly
builder.Services.AddScoped<UpdateAssignmentAccessJob>(); //won't be needed if server is running constantly

builder.Services.AddQuartz(q =>
{
    q.AddJob<UpdateGroupAccessJob>(opts => opts.WithIdentity(new JobKey("UpdateGroupAccessJob")));
    q.AddJob<UpdateAssignmentAccessJob>(opts => opts.WithIdentity(new JobKey("UpdateAssignmentAccessJob")));

    q.AddTrigger(opts => opts
        .ForJob("UpdateGroupAccessJob")
        .WithIdentity("UpdateGroupAccessJob-OnApplicationStart-trigger")
        .StartNow()
    );

    q.AddTrigger(opts => opts
        .ForJob("UpdateGroupAccessJob")
        .WithIdentity("UpdateGroupAccessJob-trigger")
        .WithCronSchedule("0 0 * ? * *")
    );

    q.AddTrigger(opts => opts
        .ForJob("UpdateAssignmentAccessJob")
        .WithIdentity("UpdateAssignmentAccessJob-OnApplicationStart-trigger")
        .StartNow()
    );

    q.AddTrigger(opts => opts
        .ForJob("UpdateAssignmentAccessJob")
        .WithIdentity("UpdateAssignmentAccessJob-trigger")
        .WithCronSchedule("0 * * ? * *")
    );
});

builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//For chats
builder.Services.AddSignalR();

//Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));

//For identity app User settings
builder.Services.AddIdentity<AppUser, IdentityRole>().
    AddDefaultTokenProviders().AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationContext>().AddSignInManager<SignInManager<AppUser>>();
builder.Services.AddScoped<UserManager<AppUser>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//For chats
app.MapHub<ChatHub>("/chat");

app.Run();
