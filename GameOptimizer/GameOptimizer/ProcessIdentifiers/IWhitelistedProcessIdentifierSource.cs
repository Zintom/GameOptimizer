using System.Diagnostics;
using Zintom.GameOptimizer.Optimization;

namespace Zintom.GameOptimizer.ProcessIdentifiers
{
    internal interface IWhitelistedProcessIdentifierSource
    {
        /// <summary>
        /// Determines whether the given <paramref name="process"/> is whitelisted.
        /// </summary>
        /// <param name="process"></param>
        /// <returns><see langword="true"/> if the given <paramref name="process"/> is whitelisted, or <see langword="false"/> otherwise.</returns>
        bool IsWhitelisted(IProcess process);

        /// <summary>
        /// Refreshes any internal state of the identifier.
        /// </summary>
        void Refresh();
    }
}
