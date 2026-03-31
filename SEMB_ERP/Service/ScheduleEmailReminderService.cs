using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SEMB_ERP.Services 
{
    public class ScheduleEmailReminderService : BackgroundService
    {
        private readonly ILogger<ScheduleEmailReminderService> _logger;
        private readonly string _connectionString;

        public ScheduleEmailReminderService(ILogger<ScheduleEmailReminderService> logger)
        {
            _logger = logger;
            _connectionString = "Data Source=10.155.152.114;Initial Catalog=SEMB_ERP_QAS;Persist Security Info=True;User ID=dt;Password=Dt@123;MultipleActiveResultSets=true;";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Schedule Email Reminder Service STARTED at: {time}", DateTime.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Schedule Email Reminder Service");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);   
            }
        }

        private async Task CheckAndSendReminders()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("SEND_EMAIL_SCHEDULE_REMINDER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120; 

                        await cmd.ExecuteNonQueryAsync();

                        _logger.LogInformation("Schedule reminders checked at: {time}", DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check schedule reminders");
            }
        }
    }
}