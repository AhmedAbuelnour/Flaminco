using DbUp.Engine;
using DbUp.Support;

namespace Flaminco.Migration.Oracle.Implementations
{
    internal class DirectoryScriptFilter(string[] directories) : IScriptFilter
    {
        public IEnumerable<SqlScript> Filter(IEnumerable<SqlScript> sorted, HashSet<string> executedScriptNames, ScriptNameComparer comparer)
        {
            return sorted
                       .Where(s => s.SqlScriptOptions.ScriptType == ScriptType.RunAlways
                           || !executedScriptNames.Contains(s.Name, comparer)).OrderBy(s => GetDirectoryIndex(s.Name));
        }

        private int GetDirectoryIndex(string scriptName)
        {
            for (int i = 0; i < directories.Length; i++)
            {
                if (scriptName.StartsWith(directories[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return directories.Length; // Scripts not in the specified directories will be at the end
        }
    }
}
