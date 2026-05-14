using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    public class RoleDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class RoleDeleteHandler : Service<Role>, IRequestHandler<RoleDeleteRequest, CommandResponse>
    {
        public RoleDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet().Include(r => r.UserRoles);
        }

        public async Task<CommandResponse> Handle(RoleDeleteRequest request, CancellationToken cancellationToken)
        {
            var existingEntity = await DbSet().SingleOrDefaultAsync(roleEntity => roleEntity.Id == request.Id, cancellationToken);
            if (existingEntity is null)
                return Error("Role not found!");

            Delete(existingEntity.UserRoles);

            await DeleteAsync(existingEntity, cancellationToken);
            return Success("Role deleted successfully.", existingEntity.Id);
        }
    }
}