using System;
using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal class GetUserQuery : IQuery<UserDetailsDto>
{
    public Guid UserId { get; set; }
}
