using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Groups;
using Users.APP.Features.Roles;

namespace Users.APP.Features.Users
{
    public class UserQueryRequest : Request, IRequest<IQueryable<UserQueryResponse>>
    {
    }

    public class UserQueryResponse : Response
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public decimal Score { get; set; }

        public bool IsActive { get; set; }

        public string Address { get; set; }

        public int? CountryId { get; set; }

        public int? CityId { get; set; }

        public int? GroupId { get; set; }

        public List<int> RoleIds { get; set; }

        public string FullName { get; set; }

        public string GenderF { get; set; }

        public string BirthDateF { get; set; }

        public string RegistrationDateF { get; set; }

        public string ScoreF { get; set; }

        public string IsActiveF { get; set; }

        public string GroupF { get; set; }

        public List<string> RolesF { get; set; }

        public GroupQueryResponse Group { get; set; }

        public List<RoleQueryResponse> Roles { get; set; }
    }

    public class UserQueryHandler : Service<User>, IRequestHandler<UserQueryRequest, IQueryable<UserQueryResponse>>
    {
        public UserQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet().Include(userEntity => userEntity.Group)
                .Include(userEntity => userEntity.UserRoles).ThenInclude(userRoleEntity => userRoleEntity.Role)
                .OrderByDescending(userEntity => userEntity.IsActive).ThenByDescending(userEntity => userEntity.RegistrationDate).ThenBy(userEntity => userEntity.UserName);
        }

        public Task<IQueryable<UserQueryResponse>> Handle(UserQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(userEntity => new UserQueryResponse
            {
                Address = userEntity.Address,
                BirthDate = userEntity.BirthDate,
                CityId = userEntity.CityId,
                CountryId = userEntity.CountryId,
                FirstName = userEntity.FirstName,
                Gender = userEntity.Gender,
                GroupId = userEntity.GroupId,
                Id = userEntity.Id,
                IsActive = userEntity.IsActive,
                LastName = userEntity.LastName,
                Password = userEntity.Password,
                RegistrationDate = userEntity.RegistrationDate,
                RoleIds = userEntity.RoleIds,
                Score = userEntity.Score,
                UserName = userEntity.UserName,

                FullName = userEntity.FirstName + " " + userEntity.LastName,
                IsActiveF = userEntity.IsActive ? "Active" : "Inactive",
                ScoreF = userEntity.Score.ToString("N1"),
                GenderF = userEntity.Gender.ToString(),
                RegistrationDateF = userEntity.RegistrationDate.ToString("MM/dd/yyyy HH:mm:ss"),
                BirthDateF = userEntity.BirthDate.HasValue ? userEntity.BirthDate.Value.ToString("MM/dd/yyyy") : string.Empty,

                GroupF = userEntity.Group.Title,
                Group = new GroupQueryResponse
                {
                    Id = userEntity.Group.Id,
                    Title = userEntity.Group.Title
                },

                RolesF = userEntity.UserRoles.Select(userRoleEntity => userRoleEntity.Role.Name).ToList(),
                Roles = userEntity.UserRoles.Select(userRoleEntity => new RoleQueryResponse
                {
                    Id = userRoleEntity.Role.Id,
                    Name = userRoleEntity.Role.Name,
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}