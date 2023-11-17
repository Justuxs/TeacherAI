using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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

        private void AddResponse(string message)
        {
            lastChat = message;
            if (!String.IsNullOrEmpty(allChat))
            {
                allChat += "\n";
            }
            allChat += message;
            AllChat.AddMessage(message, "AI Teacher", true);
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

            return await GenerateMoreContent();

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
                return false;
            }
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
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
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

                    HttpResponseMessage response = await client.GetAsync($"/klausimas/{sesionID}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        var responseObject = JsonConvert.DeserializeObject<BasicResponseModel>(responseBody);

                        if (responseObject != null)
                        {
                            var content = responseObject.response.Trim();

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
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ProvideAnswer(string answer)
        {
            Thread.Sleep(3000);
            try
            {
                if (sesionID == -1 && String.IsNullOrEmpty(allChat) && Quiz == null && !await StartConversation())
                {
                    return false;
                }

                using (HttpClient client = httpClientFactory.CreateClient("AIAPI"))
                {
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
                            QuizChat.AddMessage(responseObject.response, "AI Teacher", true);
                            QuizAnwser = QuizChat.Messages.Last();
                            Quiz.Atsakymas = responseObject.response;
                            return true;

                        }

                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
