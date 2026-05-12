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

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceInstance;

internal sealed class CreateResourceInstanceHandler : ICommandHandler<CreateResourceInstanceCommand>
{
    private readonly ResourcesDbContext _dbContext;
    private readonly IClock _clock;

    public CreateResourceInstanceHandler(ResourcesDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(CreateResourceInstanceCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dbContext.ResourceTypes
            .FirstOrDefaultAsync(t => t.Id == command.ResourceTypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dbContext.GroupReadModels
            .FirstOrDefaultAsync(g => g.Id == resourceType.GroupId, cancellationToken);

        if (group is null)
            throw new ResourcesDomainException("Group not found.");

        if (group.OwnerId != command.CallerId)
            throw new ResourcesForbiddenException("Only the group owner may create a resource instance.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ResourcesDomainException("Instance name cannot be empty.");

        var nameExists = await _dbContext.ResourceInstances
            .AnyAsync(x => x.ResourceTypeId == command.ResourceTypeId && x.Name == command.Name, cancellationToken);

        if (nameExists)
            throw new ResourcesDomainException("Instance name is already taken.");

        var propertyValuesList = (command.PropertyValues ?? []).ToList();

        var definitions = await _dbContext.ResourcePropertyDefinitions
            .Where(d => d.ResourceTypeId == command.ResourceTypeId)
            .ToListAsync(cancellationToken);

        var submittedIds = propertyValuesList.Select(pv => pv.PropertyDefinitionId).ToHashSet();

        var invalidRef = propertyValuesList.FirstOrDefault(pv => definitions.All(d => d.Id != pv.PropertyDefinitionId));
        if (invalidRef is not null)
            throw new ResourcesDomainException("Invalid property definition reference.");

        var missingRequired = definitions.FirstOrDefault(d => d.IsRequired && !submittedIds.Contains(d.Id));
        if (missingRequired is not null)
            throw new ResourcesDomainException($"Required property '{missingRequired.Name}' is missing.");

        foreach (var pv in propertyValuesList)
        {
            var def = definitions.First(d => d.Id == pv.PropertyDefinitionId);
            if (def.DataType == PropertyDataType.Number && !decimal.TryParse(pv.Value, out _))
                throw new ResourcesDomainException($"Property '{def.Name}' expects a numeric value.");
            if (def.DataType == PropertyDataType.Boolean && !bool.TryParse(pv.Value, out _))
                throw new ResourcesDomainException($"Property '{def.Name}' expects a boolean value (true/false).");
        }

        var instance = ResourceInstance.Create(
            command.InstanceId,
            command.ResourceTypeId,
            resourceType.GroupId,
            command.Name,
            command.Description,
            command.CallerId,
            _clock.CurrentDate());

        foreach (var pv in propertyValuesList)
        {
            var propertyValue = ResourcePropertyValue.Create(
                Guid.CreateVersion7(),
                command.InstanceId,
                pv.PropertyDefinitionId,
                pv.Value);

            await _dbContext.ResourcePropertyValues.AddAsync(propertyValue, cancellationToken);
        }

        await _dbContext.ResourceInstances.AddAsync(instance, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
