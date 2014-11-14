// (c) 2010-2014 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;

namespace IndiegameGarden.Util
{
    public class Util
    {
        // extract an extension e.g. "zip" from a partial or full URL e.g. http://server/test/name.zip 
        // <returns>extension after last dot, or default "zip" if no dot found in 'urlDl'.</returns>
        public static string ExtractFileExtension(string url)
        {
            int i = url.LastIndexOf('.');
            if (i == -1)
                return "zip";
            string s = url.Substring(i + 1);
            if (s.Length > 3)
            {
                return "zip"; // HACK - if no ext found, assume zip.
            }
            return s;
        }
    }
}
