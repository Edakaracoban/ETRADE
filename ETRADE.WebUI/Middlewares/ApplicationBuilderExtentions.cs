using Microsoft.Extensions.FileProviders;

namespace ETRADE.WebUI.Middlewares
{
    public static class ApplicationBuilderExtentions
    {
        public static IApplicationBuilder CustomStaticFiles(this IApplicationBuilder app)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "node_modules");
            var options = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(path),
                RequestPath = "/modules"
            };
            app.UseStaticFiles(options);
            return app;
        } //path yolunu belirtiyoruz ve requestpath ile hangi url'den erişileceğini belirtiyoruz.
        //modules diye bir istek gelirse varsayılan yerden node_modules a ulaş.
    }
}
