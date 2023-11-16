namespace TeacherAI.Data
{
    public class Quiz
    {
        public string Klausimas { get; set; }
        public Dictionary<string, string> Atsakymai { get; set; }
        public string TeisingasAtsakymas { get; set; }

        public string Atsakymas = string.Empty;
    }
}
