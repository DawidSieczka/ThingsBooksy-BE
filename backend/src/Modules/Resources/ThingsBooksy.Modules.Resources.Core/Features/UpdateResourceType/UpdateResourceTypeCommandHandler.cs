using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.UpdateResourceType;

internal sealed class UpdateResourceTypeCommandHandler : ICommandHandler<UpdateResourceTypeCommand>
{
    private readonly IUpdateResourceTypeCommandDataProvider _dataProvider;
    private readonly IClock _clock;

    public UpdateResourceTypeCommandHandler(IUpdateResourceTypeCommandDataProvider dataProvider, IClock clock)
    {
        _dataProvider = dataProvider;
        _clock = clock;
    }

    public async Task HandleAsync(UpdateResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var resourceType = await _dataProvider.GetResourceTypeWithDefinitionsAsync(command.TypeId, cancellationToken);

        if (resourceType is null)
            throw new ResourcesDomainException("Resource type not found.");

        var group = await _dataProvider.GetGroupAsync(resourceType.GroupId, cancellationToken);

        if (group is null || group.OwnerId != command.RequesterId)
            throw new ResourcesForbiddenException("Only the group owner may update a resource type.");

        resourceType.Update(command, _clock.CurrentDate());

        var inputList = (command.PropertyDefinitions ?? []).ToList();
        var existingDefinitions = resourceType.PropertyDefinitions.ToList();

        var inputIds = inputList
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        var toRemove = existingDefinitions.Where(d => !inputIds.Contains(d.Id)).ToList();
        _dataProvider.RemovePropertyDefinitions(toRemove);

        foreach (var input in inputList)
        {
            if (input.Id.HasValue)
            {
                var existing = existingDefinitions.FirstOrDefault(d => d.Id == input.Id.Value);
                if (existing is not null)
                    existing.Update(input);
            }
            else
            {
                var newDefinition = ResourcePropertyDefinition.Create(
                    new PropertyDefinitionInput(input.Name, input.DataType, input.IsRequired),
                    resourceType.Id);
                await _dataProvider.AddPropertyDefinitionAsync(newDefinition, cancellationToken);
            }
        }

        await _dataProvider.SaveChangesAsync(cancellationToken);
    }
}
