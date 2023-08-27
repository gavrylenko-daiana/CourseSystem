using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace DAL.Repository
{
    public class UnitOfWork : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<UnitOfWork> _logger;
        private ApplicationContext _context;
        private Repository<AppUser> _userRepository;
        private Repository<Assignment> _assignmentRepository;
        private Repository<AssignmentAnswer> _assignmentAnswerRepository;
        private Repository<AssignmentFile> _assignmentFileRepository;
        private Repository<Course> _courseRepository;
        private Repository<EducationMaterial> _educationMaterialRepository;
        private Repository<Group> _groupRepository;
        private Repository<UserAssignments> _userAssignmentsRepository;
        private Repository<UserCourses> _userCoursesRepository;
        private Repository<UserGroups> _userGroupsRepository;
        private Repository<UserActivity> _userActivityRepository;
        private Repository<Notification> _notificationRepository;
        private Repository<ChatMessage> _chatMessageRepository;
        private Repository<ProfileImage> _profileImageRepository;
        private Repository<CourseBackgroundImage> _courseBackgroundImageRepository;

        public UnitOfWork(ApplicationContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;
            _logger = new Logger<UnitOfWork>(_loggerFactory);
        }

        public Repository<CourseBackgroundImage> CourseBackgroundImageRepository
        {
            get
            {
                if (_courseBackgroundImageRepository == null)
                {
                    _courseBackgroundImageRepository = new Repository<CourseBackgroundImage>(_context,
                        new Logger<Repository<CourseBackgroundImage>>(_loggerFactory));
                }

                return _courseBackgroundImageRepository;
            }
        }

        public Repository<AppUser> UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new Repository<AppUser>(_context, 
                        new Logger<Repository<AppUser>>(_loggerFactory));
                }

                return _userRepository;
            }
        }

        public Repository<ChatMessage> ChatMessageRepository
        {
            get
            {
                if (_chatMessageRepository == null)
                {
                    _chatMessageRepository = new Repository<ChatMessage>(_context, 
                        new Logger<Repository<ChatMessage>>(_loggerFactory));
                }

                return _chatMessageRepository;
            }
        }

        public Repository<Notification> NotificationRepository
        {
            get
            {
                if (_notificationRepository == null)
                {
                    _notificationRepository = new Repository<Notification>(_context, 
                        new Logger<Repository<Notification>>(_loggerFactory));
                }

                return _notificationRepository;
            }
        }

        public Repository<UserActivity> UserActivityRepository
        {
            get
            {
                if (_userActivityRepository == null)
                {
                    _userActivityRepository = new Repository<UserActivity>(_context, 
                        new Logger<Repository<UserActivity>>(_loggerFactory));
                }

                return _userActivityRepository;
            }
        }

        public Repository<Assignment> AssignmentRepository
        {
            get
            {
                if (_assignmentRepository == null)
                {
                    _assignmentRepository = new Repository<Assignment>(_context, 
                        new Logger<Repository<Assignment>>(_loggerFactory));
                }

                return _assignmentRepository;
            }
        }

        public Repository<AssignmentAnswer> AssignmentAnswerRepository
        {
            get
            {
                if (_assignmentAnswerRepository == null)
                {
                    _assignmentAnswerRepository = new Repository<AssignmentAnswer>(_context, 
                        new Logger<Repository<AssignmentAnswer>>(_loggerFactory));
                }

                return _assignmentAnswerRepository;
            }
        }

        public Repository<AssignmentFile> AssignmentFileRepository
        {
            get
            {
                if (_assignmentFileRepository == null)
                {
                    _assignmentFileRepository = new Repository<AssignmentFile>(_context, 
                        new Logger<Repository<AssignmentFile>>(_loggerFactory));
                }

                return _assignmentFileRepository;
            }
        }

        public Repository<Course> CourseRepository
        {
            get
            {
                if (_courseRepository == null)
                {
                    _courseRepository = new Repository<Course>(_context, 
                        new Logger<Repository<Course>>(_loggerFactory));
                }

                return _courseRepository;
            }
        }

        public Repository<EducationMaterial> EducationMaterialRepository
        {
            get
            {
                if (_educationMaterialRepository == null)
                {
                    _educationMaterialRepository = new Repository<EducationMaterial>(_context, 
                        new Logger<Repository<EducationMaterial>>(_loggerFactory));
                }

                return _educationMaterialRepository;
            }
        }

        public Repository<Group> GroupRepository
        {
            get
            {
                if (_groupRepository == null)
                {
                    _groupRepository = new Repository<Group>(_context, 
                        new Logger<Repository<Group>>(_loggerFactory));
                }

                return _groupRepository;
            }
        }

        public Repository<UserAssignments> UserAssignmentsRepository
        {
            get
            {
                if (_userAssignmentsRepository == null)
                {
                    _userAssignmentsRepository = new Repository<UserAssignments>(_context, 
                        new Logger<Repository<UserAssignments>>(_loggerFactory));
                }

                return _userAssignmentsRepository;
            }
        }

        public Repository<UserCourses> UserCoursesRepository
        {
            get
            {
                if (_userCoursesRepository == null)
                {
                    _userCoursesRepository = new Repository<UserCourses>(_context, 
                        new Logger<Repository<UserCourses>>(_loggerFactory));
                }

                return _userCoursesRepository;
            }
        }

        public Repository<UserGroups> UserGroupsRepository
        {
            get
            {
                if (_userGroupsRepository == null)
                {
                    _userGroupsRepository = new Repository<UserGroups>(_context, 
                        new Logger<Repository<UserGroups>>(_loggerFactory));
                }

                return _userGroupsRepository;
            }
        }

        public Repository<ProfileImage> ProfileImageRepository
        {
            get
            {
                if (_profileImageRepository == null)
                {
                    _profileImageRepository = new Repository<ProfileImage>(_context, 
                        new Logger<Repository<ProfileImage>>(_loggerFactory));
                }

                return _profileImageRepository;
            }
        }

        public async Task<int> Save()
        {
            try
            {
                _logger.LogInformation("Saving changes to the database");

                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save changes to the database! Error: {errorMessage}", ex.Message);

                throw new Exception($"Fail to save changes to the database: {ex.Message}");
            }
        }

        private bool disposed = false;

        protected virtual async Task Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    await _context.DisposeAsync();
                }
            }

            this.disposed = true;
        }

        public async void Dispose()
        {
            await Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}