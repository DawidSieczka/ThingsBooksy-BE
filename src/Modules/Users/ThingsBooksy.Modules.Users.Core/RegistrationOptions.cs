using System.Collections.Generic;

namespace ThingsBooksy.Modules.Users.Core;

public class RegistrationOptions
{
    public bool Enabled { get; set; } = true;
    public IEnumerable<string> InvalidEmailProviders { get; set; } = [];
}
