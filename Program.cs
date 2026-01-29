using Avalonia;
using System;
using Microsoft.EntityFrameworkCore;
using StonePACS.Data;

namespace StonePACS;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // ✅ Auto-Patch Database Schema (Postgres)
        try 
        {
            using (var db = new StoneDbContext()) 
            {
                // Add 'Title' column if missing
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Title\" text DEFAULT 'Mr.';");
            }
        } 
        catch (Exception ex) 
        {
            Console.WriteLine($"⚠️ Database Schema Update Failed: {ex.Message}");
        }

        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
