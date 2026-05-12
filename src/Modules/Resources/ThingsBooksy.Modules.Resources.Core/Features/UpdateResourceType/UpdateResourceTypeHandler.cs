using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThingsBooksy.Modules.Resources.Core.DAL;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;

internal sealed class UpdateResourceTypeHandler : ICommandHandler<UpdateResourceTypeCommand>
{
    private readonly ResourcesDbContext _dbContext;
    private readonly IClock _clock;

    public UpdateResourceTypeHandler(ResourcesDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dbContext.ResourceTypes
            .Include(t => t.PropertyDefinitions)
            .FirstOrDefaultAsync(t => t.Id == command.TypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(g => g.Id == resourceType.GroupId, cancellationToken);

        if (group is null || group.OwnerId != command.RequesterId)
            throw new ResourcesForbiddenException("Only the group owner may update a resource type.");

        resourceType.Update(command.Name, command.Description, _clock.CurrentDate());

        var inputList = (command.PropertyDefinitions ?? []).ToList();
        var existingDefinitions = resourceType.PropertyDefinitions.ToList();

        // Remove definitions not present in input
        var inputIds = inputList
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        var toRemove = existingDefinitions.Where(d => !inputIds.Contains(d.Id)).ToList();
        _dbContext.ResourcePropertyDefinitions.RemoveRange(toRemove);

        foreach (var input in inputList)
        {
            if (input.Id.HasValue)
            {
                var existing = existingDefinitions.FirstOrDefault(d => d.Id == input.Id.Value);
                if (existing is not null)
                    existing.Update(input.Name, input.DataType, input.IsRequired);
            }
            else
            {
                var newDefinition = ResourcePropertyDefinition.Create(
                    Guid.CreateVersion7(),
                    resourceType.Id,
                    input.Name,
                    input.DataType,
                    input.IsRequired);
                await _dbContext.ResourcePropertyDefinitions.AddAsync(newDefinition, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
