using MassTransit;

            global using Contracts; // This is weird if Contracts is defined below. Usually global usings are at the top of the project or file to import namespaces. If I define `namespace Contracts` later, `global using Contracts` before it might cause a compilation error if `Contracts` isn't known yet?

using MassTransit;

            global using Contracts; // This might be problematic if Contracts isn't defined yet.
            namespace Contracts { ... }

using MassTransit;

            namespace Contracts {
                // Types here
            }

using MassTransit;

            namespace Contracts {
                // Types
            }

global using Contracts;
            using MassTransit;

            namespace Contracts { ... }

namespace Contracts {
                public class ...
            }

global using Contracts;
            using MassTransit;

            namespace Contracts { ... }

using MassTransit;

            namespace Contracts {
                // Types
            }

using MassTransit;

            global using Contracts; // This is often used in .NET projects to bring a namespace into scope. If I define it here, it's weird.