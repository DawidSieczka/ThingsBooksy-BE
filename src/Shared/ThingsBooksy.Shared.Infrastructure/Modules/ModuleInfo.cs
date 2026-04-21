using System.Collections.Generic;

namespace ThingsBooksy.Shared.Infrastructure.Modules;

public record ModuleInfo(string Name, IEnumerable<string> Policies);