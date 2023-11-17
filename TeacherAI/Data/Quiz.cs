namespace TeacherAI.Data
{
    public class Quiz
    {
        public string Klausimas { get; set; }
        public Dictionary<string, string> Atsakymai { get; set; }
        public string Teisingas_atsakymas { get; set; }

        public string Atsakymas { get; set; } = string.Empty;

        public void UpdateQuiz(Quiz quiz2)
        {
            if (quiz2 == null)
            {
                return; // Nothing to update
            }

            if (!string.IsNullOrEmpty(quiz2.Klausimas))
            {
                Klausimas = quiz2.Klausimas;
            }

            if (quiz2.Atsakymai != null)
            {
                Atsakymai = quiz2.Atsakymai;
            }

            if (!string.IsNullOrEmpty(quiz2.Teisingas_atsakymas))
            {
                Teisingas_atsakymas = quiz2.Teisingas_atsakymas;
            }

            if (!string.IsNullOrEmpty(quiz2.Atsakymas))
            {
                Atsakymas = quiz2.Atsakymas;
            }
        }
    }
}
