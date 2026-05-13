using System.Collections.Generic;

namespace ThingsBooksy.Modules.Users.Core;

internal class RegistrationOptions
{
    public bool Enabled { get; set; } = true;
    public IEnumerable<string> InvalidEmailProviders { get; set; } = [];
}
