namespace TeacherAI.Data
{
    public class Chat
    {
        public int Id { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();

        public Chat() { }

        public void AddMessage(string content, string sender_, bool isGenerated)
        {
            Messages.Add(new Message(content, sender_, isGenerated));
        }


        public static Chat GenerateRandomMessages(int count)
        {
            string[] PossibleSenders = { "Teacher", "Kid" };
            string[] PossibleContents = { "Hello!", "How can I help you?", "This is an automated message.","Labai ilga zinute ssssssssssssssssssssssssssssssssssss \n aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "Welcome!", "Goodbye!" };

            List<Message> messages = new List<Message>();
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                string randomContent = PossibleContents[random.Next(PossibleContents.Length)];
                string randomSender = PossibleSenders[random.Next(PossibleSenders.Length)];
                bool isGenerated = false;

                if (randomSender == "Teacher") isGenerated = true;


                Message message = new Message(randomContent, randomSender, isGenerated);
                messages.Add(message);
            }

            return new Chat() { Messages = messages };
        }
    }
}
