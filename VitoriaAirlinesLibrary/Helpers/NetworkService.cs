namespace VitoriaAirlinesLibrary.Helpers
{
    public class NetworkService
    {
        public async Task<Response> CheckConnection()
        {
            var client = new HttpClient();

            try
            {

                var response = await client.GetAsync("http://clients3.google.com/generate_204");

                return new Response
                {
                    IsSuccess = true,
                };

            }
            catch
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Please check your internet connection.",
                };
            }
        }
    }
}
