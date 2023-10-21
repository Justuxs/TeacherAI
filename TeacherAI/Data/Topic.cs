namespace TeacherAI.Data
{
    public class Topic
    {
        public long Id { get; set; }
        public long StageID { get; set; }
        public string Content { get; set; }

        public Stage? Stage { get; set; }
    }
}
