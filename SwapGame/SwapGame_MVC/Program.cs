namespace SwapGame_MVC {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (!app.Environment.IsDevelopment()) {
            //    app.UseExceptionHandler("/MainPage/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.UseHttpsRedirection();
            //app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions() {
                OnPrepareResponse = (context) =>
                {
                    var headers = context.Context.Response.Headers;
                    headers["X-Content-Type-Options"]    = "nosniff";
                    headers["Strict-Transport-Security"] = "max-age=15724800; includeSubdomains";
                    headers["Content-Security-Policy"]   = "frame-ancestors 'none'";
                    headers["X-Frame-Options"]           = "DENY";
                }
            });

            //app.UseRouting();

            //app.UseAuthorization();

            //app.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=MainPage}/{action=Index}/{id?}");

            app.Run();
        }
    }
}