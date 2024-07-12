using DbUp.Engine;
using DbUp.Support;

namespace Flaminco.Migration.Implementations
{
    internal class DirectoryScriptFilter : IScriptFilter
    {
        private readonly string[] _directories;

        public DirectoryScriptFilter(string[] directories) => _directories = directories;

        public IEnumerable<SqlScript> Filter(IEnumerable<SqlScript> sorted, HashSet<string> executedScriptNames, ScriptNameComparer comparer)
        {
            return sorted
                       .Where(s => s.SqlScriptOptions.ScriptType == ScriptType.RunAlways
                           || !executedScriptNames.Contains(s.Name, comparer)).OrderBy(s => GetDirectoryIndex(s.Name));
        }

        private int GetDirectoryIndex(string scriptName)
        {
            for (int i = 0; i < _directories.Length; i++)
            {
                if (scriptName.StartsWith(_directories[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return _directories.Length; // Scripts not in the specified directories will be at the end
        }
    }
}
