using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TeacherAI.Data;
using TeacherAI.Data.API;

namespace TeacherAI.Service
{
    public class AIService
    {
        private readonly IHttpClientFactory httpClientFactory;

        private int sesionID = -1;

        private string allChat = String.Empty;
        private string lastChat = String.Empty;

        private Chat AllChat = new Chat();

        private Chat QuizChat = new Chat();

        private Quiz Quiz;

        private string Subject;
        private string Scope;
        private string Topic;

        private Message QuizAnwser;

        // Inject HttpClient using constructor injection
        public AIService(IHttpClientFactory _httpClientFactory)
        {
            httpClientFactory = _httpClientFactory;
        }

        public string GetLastMessage()
        {
            return lastChat;
        }

        public Chat GetChat()
        {
            return AllChat;
        }

        public Quiz GetQuiz()
        {
            return Quiz;
        }


        public Quiz UpdateQuiz(Quiz mainQuiz)
        {
            mainQuiz.UpdateQuiz(Quiz);
            return mainQuiz;
        }


        public Message GetQuizAnswserMessage()
        {
            return QuizAnwser;
        }


        public void SetParams(string subject, string scope, string topic)
        {
            Subject = subject;
            Scope = scope;
            Topic = topic;
        }

        private void AddResponse(string message, bool isAnswered = false)
        {
            lastChat = message;
            if (!String.IsNullOrEmpty(allChat))
            {
                allChat += "\n";
            }
            allChat += message;
            AllChat.AddMessage(message, "AI Teacher", true, isAnswered);

        }



        public async Task<bool> GenerateAIContent()
        {
            lastChat = String.Empty;

            if (sesionID == -1 && !await StartConversation())
            {
                return false;
            }

            if (String.IsNullOrEmpty(allChat))
            {
                return await GenerateContent();
            }

            return await GenerateMoreContentWithRetries();

        }

        public async Task<bool> GenerateAnswerContent(string quastion)
        {
            if (string.IsNullOrEmpty(quastion))
            {
                return false;
            }

            lastChat = String.Empty;

            if (sesionID == -1 && !await StartConversation())
            {
                return false;
            }

            if (String.IsNullOrEmpty(allChat))
            {
                return await GenerateContent();
            }

            return await GenerateAnswerWithRetries(quastion);

        }

        private async Task<bool> StartConversation()
        {
            try
            {
                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    HttpResponseMessage response = await client.PostAsync("/create", null);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<CreationResponseModel>(responseBody);

                        sesionID = responseObject.Id;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> GenerateContent()
        {
            try
            {
                if (sesionID == -1 && !await StartConversation())
                {
                    return false;
                }
                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);


                    var payload = new
                    {
                        subject = Subject,
                        scope = Scope,
                        topic = Topic
                    };

                    // Convert the payload to JSON
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    // Create StringContent with JSON data
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Make a POST request
                    HttpResponseMessage response = await client.PostAsync($"/generuok/{sesionID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            AddResponse(responseObject.response);
                            return true;

                        }
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX respose : " + ex.Message);
                return false;
            }
        }


        public async Task<bool> GenerateMoreContentWithRetries(int maxRetries = 3)
        {
            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                if (await GenerateMoreContent())
                {
                    return true;
                }

                Console.WriteLine($"Retry #{retryCount + 1} failed.");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            return false;
        }



        private async Task<bool> GenerateMoreContent()
        {
            try
            {
                if (sesionID == -1 && String.IsNullOrEmpty(allChat) && !await StartConversation())
                {
                    return false;
                }

                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    HttpResponseMessage response = await client.GetAsync($"/toliau/{sesionID}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            AddResponse(responseObject.response);
                            return true;
                        }

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("BAD respose : " + await response.Content.ReadAsStringAsync());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX respose : " + ex.Message);
                return false;
            }
        }

        public async Task<bool> GenerateAnswerWithRetries(string question, int maxRetries = 3)
        {
            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                if (await GenerateAnswer(question))
                {
                    return true;
                }

                Console.WriteLine($"Retry #{retryCount + 1} failed.");

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            return false;
        }

        private async Task<bool> GenerateAnswer(string question)
        {
            if (string.IsNullOrEmpty(question))
            {
                throw new ArgumentException($"'{nameof(question)}' cannot be null or empty.", nameof(question));
            }

            try
            {
                if (sesionID == -1 && String.IsNullOrEmpty(allChat) && !await StartConversation())
                {
                    return false;
                }

                AllChat.AddMessage(question, "You", false);


                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    var payload = new
                    {
                        question = question
                    };

                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"/atsakyk/{sesionID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            AddResponse(responseObject.response, true);
                            return true;
                        }

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("BAD respose : " + await response.Content.ReadAsStringAsync());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX respose : " + ex.Message);
                return false;
            }
        }

        public async Task<bool> GenerateQuizWithRetries(int maxRetries = 3)
        {
            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                if (await GenerateQuiz())
                {
                    return true;
                }

                Console.WriteLine($"Retry #{retryCount + 1} failed.");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            return false;
        }

        public async Task<bool> GenerateQuiz()
        {
            Quiz = null;
            try
            {
                if (sesionID == -1 && String.IsNullOrEmpty(allChat) && !await StartConversation())
                {
                    return false;
                }

                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    HttpResponseMessage response = await client.GetAsync($"/klausimas/{sesionID}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            Console.WriteLine(responseObject.response);
                            string parsedJson = ExtractJsonFromString(responseObject.response.Trim());
                            Console.WriteLine("Isparsintas: " + parsedJson);

                            string content = "";
                            if (string.IsNullOrEmpty(parsedJson))
                            {
                                content = parsedJson;
                            }
                            else
                            {
                                content = responseObject.response.Trim();
                            }
                            var quiz = JsonConvert.DeserializeObject<Quiz>(content);
                            if (quiz != null)
                            {
                                Quiz = quiz;
                                QuizChat.AddMessage(Quiz.Klausimas, "AI Teacher", true);
                                QuizAnwser = null;
                                return true;
                            }
                        }

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("BAD respose : " + await response.Content.ReadAsStringAsync());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("parsinimo klaida : " + ex.Message);

                return false;
            }
        }

        static string ExtractJsonFromString(string inputString)
        {
            string jsonPattern = @"\{.*\}";
            Match match = Regex.Match(inputString, jsonPattern);

            return match.Success ? match.Value : null;
        }

        public async Task<bool> ProvideAnswer(string answer)
        {
            try
            {
                if (sesionID == -1 && String.IsNullOrEmpty(allChat) && Quiz == null && !await StartConversation())
                {
                    return false;
                }

                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
                    client.Timeout = TimeSpan.FromSeconds(8);

                    var payload = new
                    {
                        answer = answer
                    };
                    string jsonPayload = JsonConvert.SerializeObject(payload);

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync($"/atsakymas/{sesionID}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            QuizChat.AddMessage(answer, "User", false);
                            QuizChat.AddMessage(responseObject.response, "AI Teacher", false);
                            QuizAnwser = QuizChat.Messages.Last();
                            Quiz.Atsakymas = responseObject.response;
                            return true;

                        }

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("BAD respose : " + await response.Content.ReadAsStringAsync());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX respose : " + ex.Message);
                return false;
            }
        }
    }
}
