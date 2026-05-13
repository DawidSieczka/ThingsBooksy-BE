using ThingsBooksy.Shared.Abstractions.Queries;

namespace ThingsBooksy.Modules.Users.Core.Features.GetUser;

internal record GetUserQuery(Guid UserId) : IQuery<GetUserQueryResult?>;
