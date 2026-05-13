using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceInstance;

internal record DeleteResourceInstanceCommand(Guid InstanceId, Guid RequesterId) : ICommand;
