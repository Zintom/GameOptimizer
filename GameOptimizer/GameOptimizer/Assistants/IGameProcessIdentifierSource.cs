using System.Diagnostics;

namespace Zintom.GameOptimizer.Assistants
{
    internal interface IGameProcessIdentifierSource
    {
        /// <summary>
        /// Determines whether the given <paramref name="process"/> is a game or not.
        /// </summary>
        /// <param name="process"></param>
        /// <returns><see langword="true"/> if the given <paramref name="process"/> is believed to be a game, or <see langword="false"/> otherwise.</returns>
        bool IsGame(Process process);

        /// <summary>
        /// Refreshes any internal state of the identifier.
        /// </summary>
        void Refresh();
    }
}
