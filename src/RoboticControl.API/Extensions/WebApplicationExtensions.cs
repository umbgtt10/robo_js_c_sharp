namespace RoboticControl.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure the HTTP request pipeline middleware
    /// </summary>
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Configure the HTTP request pipeline

        // Exception handler must be first to catch all exceptions
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        // Rate limiting (after CORS, before Authentication)
        app.UseRateLimiter();

        // Authentication and Authorization (order matters: Authentication before Authorization)
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Create SignalR hub endpoint for real-time updates to the frontend (e.g. position and status updates) - this is where the frontend will connect to receive real-time notifications from the server about hardware events. The HardwareEventBroadcastService will push updates to this hub whenever the robot's position or status changes.
        app.MapHub<Hubs.RobotHub>("/hubs/robot");

        // Minimal health check endpoint
        app.MapGet("/health", () => new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });

        return app;
    }
}
