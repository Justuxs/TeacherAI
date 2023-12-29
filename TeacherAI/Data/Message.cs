using NuGet.Protocol.Plugins;

namespace TeacherAI.Data
{
    public class Message
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public string Sender_ { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool isGenerated { get; set; }
        public bool isAnswered { get; set; } = false;

        public Message(string content, string sender_, bool isGenerated)
        {
            Content = content;
            Sender_ = sender_;
            this.isGenerated = isGenerated;
        }

        public Message(string content, string sender_, bool isGenerated, bool isAnswered)
        {
            Content = content;
            Sender_ = sender_;
            this.isGenerated = isGenerated;
            this.isAnswered = isAnswered;
        }

    }
}
