using CORE.APP.Models;
using CORE.APP.Services;
using Books.APP.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.APP.Features.Users
{
    public class UserQueryRequest : Request, IRequest<IQueryable<UserQueryResponse>>
    {
    }

    public class UserQueryResponse : Response
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

        public string IsActiveF { get; set; }
    }

    public class UserQueryHandler : Service<User>, IRequestHandler<UserQueryRequest, IQueryable<UserQueryResponse>>
    {
        public UserQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet()
                .OrderBy(u => u.UserName);
        }

        public Task<IQueryable<UserQueryResponse>> Handle(UserQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(u => new UserQueryResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Password = u.Password,
                Email = u.Email,
                IsActive = u.IsActive,
                IsActiveF = u.IsActive ? "Active" : "Not Active"
            });

            return Task.FromResult(query);
        }
    }
}