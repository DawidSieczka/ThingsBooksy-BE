using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;

internal sealed class CreateResourceTypeHandler : ICommandHandler<CreateResourceTypeCommand>
{
    private readonly ResourcesDbContext _dbContext;
    private readonly IClock _clock;

    public CreateResourceTypeHandler(ResourcesDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(CreateResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(g => g.Id == command.GroupId, cancellationToken);

        if (group is null)
            throw new ResourcesDomainException("Group not found.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ResourcesDomainException("Resource type name cannot be empty.");

        if (group.OwnerId != command.CallerId)
            throw new ResourcesForbiddenException("Only the group owner may create a resource type.");

        var nameExists = await _dbContext.ResourceTypes
            .AnyAsync(t => t.GroupId == command.GroupId && t.Name == command.Name, cancellationToken);

        if (nameExists)
            throw new ResourcesDomainException($"Resource type name '{command.Name}' is already taken within this group.");

        var resourceType = ResourceType.Create(command.TypeId, command.GroupId, command.Name, command.Description, _clock.CurrentDate());

        foreach (var def in command.PropertyDefinitions)
        {
            var definition = ResourcePropertyDefinition.Create(
                Guid.CreateVersion7(), command.TypeId, def.Name, def.DataType, def.IsRequired);
            await _dbContext.ResourcePropertyDefinitions.AddAsync(definition, cancellationToken);
        }

        await _dbContext.ResourceTypes.AddAsync(resourceType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
