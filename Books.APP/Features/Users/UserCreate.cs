using System.ComponentModel.DataAnnotations;
using Books.APP.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Users
{
    public class UserCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        public bool IsActive { get; set; }
    }

    public class UserCreateHandler : Service<User>, IRequestHandler<UserCreateRequest, CommandResponse>
    {
        public UserCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(UserCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.UserName == request.UserName.Trim(), cancellationToken))
                return Error($"User with the same user name: \"{request.UserName.Trim()}\" exists!");

            if (await DbSet().AnyAsync(u => u.Email == request.Email.Trim(), cancellationToken))
                return Error($"User with the same email: \"{request.Email.Trim()}\" exists!");

            var entity = new User
            {
                UserName = request.UserName?.Trim(),
                Password = request.Password?.Trim(),
                Email = request.Email?.Trim(),
                IsActive = request.IsActive
            };

            await CreateAsync(entity, cancellationToken);

            return Success($"User with user name {request.UserName.Trim()} created successfully.", entity.Id);
        }
    }
}