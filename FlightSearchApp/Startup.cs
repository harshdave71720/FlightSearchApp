using FlightSearchApp.Configurations;
using FlightSearchApp.DbContexts;
using FlightSearchApp.Repositories;
using FlightSearchApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace FlightSearchApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen();
            
            var flightSearchConfiguration = new FlightSearchConfiguration();
            Configuration.GetSection("FlightSearchConfiguration").Bind(flightSearchConfiguration);
            services.AddSingleton<FlightSearchConfiguration>(flightSearchConfiguration);

            var ianaAirportConfig = new IanaAirportConfig();
            Configuration.GetSection("IanaAirportConfig").Bind(ianaAirportConfig);
            services.AddSingleton<IanaAirportConfig>(ianaAirportConfig);

            // To run in-memory version comment this statement and uncomment next one
            services.AddDbContext<FlightDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"));
            });
            //services.AddSingleton<IFlightRepository, InMemoryFlightRepository>();
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<IFlightService, FlightService>();
            services.AddSingleton<IAirportTimeConverter, AirportTimeConverter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
