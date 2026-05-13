using System.Threading;
using System.Threading.Tasks;
using ThingsBooksy.Modules.Resources.Core.Domain;
using ThingsBooksy.Modules.Resources.Core.Exceptions;
using ThingsBooksy.Shared.Abstractions.Commands;
using ThingsBooksy.Shared.Abstractions.Time;

namespace ThingsBooksy.Modules.Resources.Core.Features.CreateResourceType;

internal sealed class CreateResourceTypeCommandHandler : ICommandHandler<CreateResourceTypeCommand, Guid>
{
    private readonly ICreateResourceTypeCommandDataProvider _dataProvider;
    private readonly IClock _clock;

    public CreateResourceTypeCommandHandler(ICreateResourceTypeCommandDataProvider dataProvider, IClock clock)
    {
        _dataProvider = dataProvider;
        _clock = clock;
    }

    public async Task<Guid> HandleAsync(CreateResourceTypeCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _dataProvider.GetGroupAsync(command.GroupId, cancellationToken);

        if (group is null)
            throw new ResourcesDomainException("Group not found.");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ResourcesDomainException("Resource type name cannot be empty.");

        if (group.OwnerId != command.CallerId)
            throw new ResourcesForbiddenException("Only the group owner may create a resource type.");

        var nameExists = await _dataProvider.NameExistsAsync(command.GroupId, command.Name, cancellationToken);

        if (nameExists)
            throw new ResourcesDomainException($"Resource type name '{command.Name}' is already taken within this group.");

        var resourceType = ResourceType.Create(command, _clock.CurrentDate());

        foreach (var def in command.PropertyDefinitions)
        {
            var definition = ResourcePropertyDefinition.Create(def, resourceType.Id);
            await _dataProvider.AddPropertyDefinitionAsync(definition, cancellationToken);
        }

        await _dataProvider.AddResourceTypeAsync(resourceType, cancellationToken);
        await _dataProvider.SaveChangesAsync(cancellationToken);

        return resourceType.Id;
    }
}
