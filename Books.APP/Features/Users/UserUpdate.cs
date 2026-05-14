using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Users
{
    public class UserUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        public bool IsActive { get; set; }
    }

    public class UserUpdateHandler : Service<User>, IRequestHandler<UserUpdateRequest, CommandResponse>
    {
        public UserUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.Id != request.Id && u.UserName == request.UserName.Trim(), cancellationToken))
                return Error($"User with the same user name: \"{request.UserName.Trim()}\" exists!");

            if (await DbSet().AnyAsync(u => u.Id != request.Id && u.Email == request.Email.Trim(), cancellationToken))
                return Error($"User with the same email: \"{request.Email.Trim()}\" exists!");

            var entity = await DbSet().SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("User not found!");

            entity.UserName = request.UserName?.Trim();
            entity.Password = request.Password?.Trim();
            entity.Email = request.Email?.Trim();
            entity.IsActive = request.IsActive;

            await UpdateAsync(entity, cancellationToken);

            return Success($"User with user name {request.UserName.Trim()} updated successfully.", entity.Id);
        }
    }
}