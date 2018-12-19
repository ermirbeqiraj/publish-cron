using Dapper;
using MySql.Data.MySqlClient;

namespace PublishCron
{
    public class DbService
    {
        private readonly string connectionString;
        public DbService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public MySqlConnection DefaultConnection => new MySqlConnection(connectionString);


        public int PublishScheduledArticles(System.Action<string> log)
        {
            var queryString = @"UPDATE articles
SET articles.Scheduled = null,
    articles.ArticleStatus = 1,
    articles.Date = utc_timestamp()
WHERE Scheduled IS NOT NULL
		AND Scheduled < utc_timestamp();";

            log("Query build.");
            try
            {
                using (var conn = DefaultConnection)
                {
                    conn.Open();
                    log("Connection opened");
                    var markResult = conn.Execute(queryString);
                    log($"Command executed the query successfully with [{markResult}] results");
                    return markResult;
                }
            }
            catch (System.Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                log($"ERROR: Connection failed with message:  {ex.Message} and stacktrace: {ex.StackTrace}");
                return 0;
            }
        }
    }
}
