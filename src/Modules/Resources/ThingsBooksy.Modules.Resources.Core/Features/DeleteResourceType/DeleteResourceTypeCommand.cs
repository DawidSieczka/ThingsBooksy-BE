using System;
using ThingsBooksy.Shared.Abstractions.Commands;

namespace ThingsBooksy.Modules.Resources.Core.Features.DeleteResourceType;

internal record DeleteResourceTypeCommand(Guid TypeId, Guid RequesterId) : ICommand;
