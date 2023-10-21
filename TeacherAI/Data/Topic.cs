namespace TeacherAI.Data
{
    public class Topic
    {
        public long Id { get; set; }
        public long StageID { get; set; }
        public long Content { get; set; }

        public Stage? Stage { get; set; }
    }
}
