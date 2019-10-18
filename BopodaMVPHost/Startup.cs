// Based ON:
// https://github.com/azure-samples/active-directory-b2c-dotnetcore-webapi
// https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapp
// https://github.com/Azure-Samples/active-directory-b2c-xamarin-native

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BopodaMVPHost
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => _configuration.Bind("Authentication:AzureAdB2C", options));

            services.AddControllers();

            services.AddDistributedMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRouting();
            app.UseEndpoints(t => t.MapDefaultControllerRoute());
        }
    }
}
