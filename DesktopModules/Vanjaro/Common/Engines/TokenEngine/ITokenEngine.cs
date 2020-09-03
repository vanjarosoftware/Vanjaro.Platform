namespace Vanjaro.Common.Engines.TokenEngine
{
    public interface ITokenEngine
    {
        /// <summary>
        /// Parses the given template for tokens 
        /// </summary>
        /// <param name="DNNContext"></param>
        /// <param name="Template"></param>
        /// <returns></returns>
        string Parse(string Template);
    }
}
