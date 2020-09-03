namespace Vanjaro.Common.Engines.TemplateEngine
{
    public interface ITemplateEngine
    {
        /// <summary>
        /// Loads the template, parses it for tokens, and mantains a cache
        /// </summary>
        /// <param name="DNNContext"></param>
        /// <param name="TemplatePath"></param>
        /// <returns></returns>
        string RenderTemplatePath(string TemplatePath);

        /// <summary>
        /// Parses template for tokens
        /// </summary>
        /// <param name="DNNContext"></param>
        /// <param name="TemplatePath"></param>
        /// <returns></returns>
        string Render(string Template);
    }
}
