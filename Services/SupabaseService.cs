using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Interfaces;

namespace DOSSOKAM2019.Services
{
    public class SupabaseService
    {
        private readonly Client _client;

        public SupabaseService(IConfiguration config)
        {
            var url = config["Supabase:Url"];
            var key = config["Supabase:AnonKey"];
            var options = new ClientOptions { AutoRefreshToken = true };
            _client = new Client(url, key, options);
            _client.InitializeAsync().Wait();
        }

        public Client Client => _client;
    }
}