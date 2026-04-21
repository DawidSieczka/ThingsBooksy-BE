using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal sealed class GetUserHandler : IQueryHandler<GetUserQuery, UserDetailsDto>
{
    private readonly IGetUserRepository _repository;

    public GetUserHandler(IGetUserRepository repository)
        => _repository = repository;

    public async Task<UserDetailsDto> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(query.UserId);
        if (user is null) return null!;

        return new UserDetailsDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.Name,
            JobTitle = user.JobTitle,
            CreatedAt = user.CreatedAt,
            Permissions = user.Role.Permissions
        };
    }
}
