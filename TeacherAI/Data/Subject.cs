namespace TeacherAI.Data
{
    public class Subject
    {
        public Subject(string name)
        {
            Name = name;
        }

        public Subject()
        {
        }
        public Subject(long id, string name, List<Stage>? stages)
        {
            Id = id;
            Name = name;
            Stages = stages;
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public List<Stage>? Stages { get; set; }
    }
}
