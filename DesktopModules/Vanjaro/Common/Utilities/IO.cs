using System.IO;

namespace Vanjaro.Common.Utilities
{
    public static class IO
    {
        public static string OpenTextFile(string FilePath)
        {
            using (StreamReader sr = File.OpenText(FilePath))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
