using System.Text;
using authapi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace authapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string TokenKey { get => Configuration.GetValue<string>("TokenKey"); }
        public uint CacheSize { get => Configuration.GetValue<uint>("TokenCache"); }

        // This method gets called by the runtime. Use this method to add services to the container.


        public void ConfigureServices(IServiceCollection services)
        {
            var token = $"{TokenKey}_{System.Net.Dns.GetHostName()}";
            var key = Encoding.ASCII.GetBytes(token);
            services.AddControllers();
            services.AddSingleton<IRepository>(new Repository(Configuration.GetConnectionString("Default")));
            services.AddSingleton<IJwtAuthentication>(provider =>
            {
                var repoDependency = provider.GetRequiredService<IRepository>();
                var memDependency = provider.GetRequiredService<IMemCache>();
                return new JwtAuthenticator(repoDependency, memDependency, token);
            });
            services.AddSingleton<IMemCache>(new MemCache(CacheSize));

            // services.AddAuthentication(options =>
            // {
            //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            // })
            // .AddJwtBearer(options =>
            // {
            //     options.RequireHttpsMetadata = false;
            //     options.SaveToken = true;
            //     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //     {
            //         ValidateIssuerSigningKey = true,
            //         IssuerSigningKey = new SymmetricSecurityKey(key),
            //         ValidateIssuer = false,
            //         ValidateAudience = false,
            //         ValidateLifetime = true,
            //         ClockSkew = System.TimeSpan.FromSeconds(30)
            //     };
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthentication();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
