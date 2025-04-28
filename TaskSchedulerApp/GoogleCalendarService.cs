using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TaskSchedulerApp.Models;

public class GoogleCalendarService
{
    private static string[] Scopes = { CalendarService.Scope.Calendar };
    private static string ApplicationName = "Task Scheduler App";

    public static async Task<CalendarService> AuthenticateAsync()
    {
        UserCredential credential;
        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }

        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }
    public async Task AddTaskToGoogleCalendar(TaskModel task)
    {
        try
        {
            var service = await GoogleCalendarService.AuthenticateAsync();

            Event newEvent = new Event()
            {
                Summary = task.Title,
                Start = new EventDateTime()
                {
                    DateTime = task.TaskDate,
                    TimeZone = "Europe/Budapest"
                },
                End = new EventDateTime()
                {
                    DateTime = task.TaskDate.Add(task.EstimatedDuration),
                    TimeZone = "Europe/Budapest"
                }
            };

            var request = service.Events.Insert(newEvent, "primary");
            var response = await request.ExecuteAsync();

            
            Console.WriteLine($"Event successfully added: {response.HtmlLink}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding event to Google Calendar: {ex.Message}");
        }
    }
   
}
