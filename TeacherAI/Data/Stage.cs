namespace TeacherAI.Data
{
    public class Stage
    {
        public long Id { get; set; }
        public long SubjectID { get; set; }
        public string Name { get; set; }

        public Subject? Subject { get; set; }
        public List<Topic>? Topics { get; set; }
    }

}
