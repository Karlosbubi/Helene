using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helene.Utils
{
    internal static class PathHelpers
    {
        internal static string GetRelativePath(string from, string to)
        {

            // Ensure the fromPath ends with the directory separator
            if (!from.EndsWith("/"))
            {
                from += "/";
            }

            if (!to.StartsWith("/"))
            {
                to += "/";
            }

            Uri fromUri = new Uri(from);
            Uri toUri = new Uri(to);

            // Get the relative path
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            Console.WriteLine($"From : \"{fromUri}\" To : \"{toUri}\" -> \"{relativeUri}\"");
            return Uri.UnescapeDataString(relativeUri.ToString());
        }
    }
}
