using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Roles
{
    public class RoleUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(25)]
        public string Name { get; set; }

        public List<int> UserIds { get; set; } = new List<int>();
    }

    public class RoleUpdateHandler : Service<Role>, IRequestHandler<RoleUpdateRequest, CommandResponse>
    {
        public RoleUpdateHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet().Include(r => r.UserRoles);
        }

        public async Task<CommandResponse> Handle(RoleUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(roleEntity => roleEntity.Id != request.Id && roleEntity.Name == request.Name.Trim(), cancellationToken))
                return Error("Role with the same name exists!");

            var existingEntity = await DbSet().SingleOrDefaultAsync(roleEntity => roleEntity.Id == request.Id, cancellationToken);
            if (existingEntity is null)
                return Error("Role not found!");

            Delete(existingEntity.UserRoles);

            existingEntity.Name = request.Name.Trim();
            existingEntity.UserIds = request.UserIds;

            await UpdateAsync(existingEntity, cancellationToken);
            return Success("Role updated successfully.", existingEntity.Id);
        }
    }
}