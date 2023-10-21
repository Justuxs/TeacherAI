using TeacherAI.Data;

namespace TeacherAI.Service
{
    public class SubjectService
    {
        public static List<Subject> GenerateRandomSubjects(int subjectCount, int stagesPerSubject, int topicsPerStage)
        {
            Random random = new Random();
            List<Subject> subjects = new List<Subject>();

            for (int i = 0; i < subjectCount; i++)
            {
                Subject subject = new Subject
                {
                    Id = i + 1,
                    Name = $"Subject {i + 1}",
                    Stages = new List<Stage>()
                };

                for (int j = 0; j < stagesPerSubject; j++)
                {
                    Stage stage = new Stage
                    {
                        Id = (i * stagesPerSubject) + j + 1,
                        SubjectID = subject.Id,
                        Name = $"Stage {j + 1}",
                        Topics = new List<Topic>()
                    };

                    for (int k = 0; k < topicsPerStage; k++)
                    {
                        Topic topic = new Topic
                        {
                            Id = ((i * stagesPerSubject * topicsPerStage) + (j * topicsPerStage) + k) + 1,
                            StageID = stage.Id,
                            Content = $"Topic {k + 1}"
                        };
                        stage.Topics.Add(topic);
                    }

                    subject.Stages.Add(stage);
                }

                subjects.Add(subject);
            }

            return subjects;
        }
    }
}
