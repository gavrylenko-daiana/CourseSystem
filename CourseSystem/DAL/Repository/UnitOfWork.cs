using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace DAL.Repository
{
    public class UnitOfWork : IDisposable
    {
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

        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }

        public Repository<AppUser> UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new Repository<AppUser>(_context);
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
                    _chatMessageRepository = new Repository<ChatMessage>(_context);
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
                    _notificationRepository = new Repository<Notification>(_context);
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
                    _userActivityRepository = new Repository<UserActivity>(_context);
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
                    _assignmentRepository = new Repository<Assignment>(_context);
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
                    _assignmentAnswerRepository = new Repository<AssignmentAnswer>(_context);
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
                    _assignmentFileRepository = new Repository<AssignmentFile>(_context);
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
                    _courseRepository = new Repository<Course>(_context);
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
                    _educationMaterialRepository = new Repository<EducationMaterial>(_context);
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
                    _groupRepository = new Repository<Group>(_context);
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
                    _userAssignmentsRepository = new Repository<UserAssignments>(_context);
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
                    _userCoursesRepository = new Repository<UserCourses>(_context);
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
                    _userGroupsRepository = new Repository<UserGroups>(_context);
                }

                return _userGroupsRepository;
            }
        }

        public async Task<int> Save()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
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