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

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceInstance;

internal sealed class UpdateResourceInstanceHandler : ICommandHandler<UpdateResourceInstanceCommand>
{
    private readonly ResourcesDbContext _dbContext;
    private readonly IClock _clock;

    public UpdateResourceInstanceHandler(ResourcesDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateResourceInstanceCommand command, CancellationToken cancellationToken = default)
    {
        var instance = await _dbContext.ResourceInstances
            .Include(x => x.PropertyValues)
            .FirstOrDefaultAsync(x => x.Id == command.InstanceId, cancellationToken);

        if (instance is null)
            throw new ResourcesDomainException("Resource instance not found.");

        var isOwner = await _dbContext.GroupReadModels
            .AnyAsync(g => g.Id == instance.GroupId && g.OwnerId == command.RequesterId, cancellationToken);

        if (!isOwner)
            throw new ResourcesForbiddenException("Only the group owner may update a resource instance.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ResourcesDomainException("Instance name cannot be empty.");

        var propertyValuesList = (command.PropertyValues ?? []).ToList();

        var definitions = await _dbContext.ResourcePropertyDefinitions
            .Where(d => d.ResourceTypeId == instance.ResourceTypeId)
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

        _dbContext.ResourcePropertyValues.RemoveRange(instance.PropertyValues);

        instance.Update(command.Name, command.Description, _clock.CurrentDate());

        foreach (var pv in propertyValuesList)
        {
            var propertyValue = ResourcePropertyValue.Create(
                Guid.CreateVersion7(),
                instance.Id,
                pv.PropertyDefinitionId,
                pv.Value);

            await _dbContext.ResourcePropertyValues.AddAsync(propertyValue, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
