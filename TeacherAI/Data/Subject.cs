namespace TeacherAI.Data
{
    public class Subject
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<Stage>? Stages { get; set; }
    }
}
