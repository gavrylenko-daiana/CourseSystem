using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace DAL.Repository
{
    internal class UnitOfWork : IDisposable
    {
        private ApplicationContext _context;
        private Repository<AppUser> _appUserRepository;
        private Repository<Assignment> _assignmentRepository;
        private Repository<AssignmentAnswer> _assignmentAnswerRepository;
        private Repository<AssignmentFile> _assignmentFileRepository;
        private Repository<Course> _courseRepository;
        private Repository<EducationMaterial> _educationMaterialRepository;
        private Repository<Group> _groupRepository;
        private Repository<UserAssignments> _userAssignmentsRepository;
        private Repository<UserCourses> _userCoursesRepository;
        private Repository<UserGroups> _userGroupsRepository;

        public Repository<AppUser> AppUserRepository
        {
            get
            {

                if (_appUserRepository == null)
                {
                    _appUserRepository = new Repository<AppUser>(_context);
                }
                return _appUserRepository;
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

        public async Task Save()
        {
            await _context.SaveChangesAsync();
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
