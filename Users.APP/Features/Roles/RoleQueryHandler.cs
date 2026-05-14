using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Users;

namespace Users.APP.Features.Roles
{
    public class RoleQueryRequest : Request, IRequest<IQueryable<RoleQueryResponse>>
    {
    }

    public class RoleQueryResponse : Response
    {
        public string Name { get; set; }

        public List<int> UserIds { get; set; }

        public List<string> UsersF { get; set; }

        public List<UserQueryResponse> Users { get; set; }
    }

    public class RoleQueryHandler : Service<Role>, IRequestHandler<RoleQueryRequest, IQueryable<RoleQueryResponse>>
    {
        public RoleQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet().Include(roleEntity => roleEntity.UserRoles).ThenInclude(userRoleEntity => userRoleEntity.User);
        }

        public Task<IQueryable<RoleQueryResponse>> Handle(RoleQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(roleEntity => new RoleQueryResponse
            {
                Id = roleEntity.Id,
                Name = roleEntity.Name,
                UserIds = roleEntity.UserIds,
                UsersF = roleEntity.UserRoles.Select(userRoleEntity => userRoleEntity.User.UserName).ToList(),
                Users = roleEntity.UserRoles.Select(userRoleEntity => new UserQueryResponse
                {
                    Id = userRoleEntity.User.Id,
                    UserName = userRoleEntity.User.UserName
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}