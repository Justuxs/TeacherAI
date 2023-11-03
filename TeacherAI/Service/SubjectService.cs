using Microsoft.EntityFrameworkCore;
using TeacherAI.Data;
using TeacherAI.EF;

namespace TeacherAI.Service
{
    public class SubjectService
    {

        private readonly TeacherContext _context;

        public SubjectService(TeacherContext context)
        {
            _context = context;
        }

        public List<Subject> GetSubjects()
        {
            List<Subject> subjects = _context.Subjects.ToList();
            return subjects;
        }

        public async Task<List<Subject>> GetSubjectsAsync()
        {
            List<Subject> subjects = await _context.Subjects.ToListAsync();
            return subjects;
        }

        public async Task<List<Subject>> GetSubjectsFullAsync()
        {
            List<Subject> subjects = await _context.Subjects.Include(s => s.Stages).ThenInclude(stage => stage.Topics).ToListAsync();
            return subjects;
        }

        public async Task<Subject> GetSubjectFullAsync(long id)
        {
            Subject subject = await _context.Subjects
                .Where(s => s.Id == id)
                .Include(s => s.Stages)
                    .ThenInclude(stage => stage.Topics)
                .FirstOrDefaultAsync();

            return subject;
        }

        public async Task<Stage> GetSubjectStageAsync(long id, long stageId)
        {
            Subject subject = await _context.Subjects
                .Where(s => s.Id == id)
                .Include(s => s.Stages)
                    .ThenInclude(stage => stage.Topics)
                .FirstOrDefaultAsync();

            if (subject != null)
            {
                Stage stage = subject.Stages.FirstOrDefault(s => s.Id == stageId);
                return stage;
            }

            return null;
        }


        public async Task DeleteSubjectAsync(long subjectId)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject != null)
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStageFromSubjectAsync(long subjectId, long stageId)
        {
            // Step 1: Retrieve the Subject
            Subject subject = await _context.Subjects
                .Where(s => s.Id == subjectId)
                .Include(s => s.Stages)
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                throw new InvalidOperationException("Subject not found");
            }

            // Step 2: Retrieve the Stage
            Stage stageToRemove = subject.Stages.FirstOrDefault(stage => stage.Id == stageId);

            if (stageToRemove == null)
            {
                throw new InvalidOperationException("Stage not found in the subject");
            }

            // Step 3: Remove the Stage from the Subject's Stages Collection
            subject.Stages.Remove(stageToRemove);

            // Step 4: Save Changes to the Database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTopicAsync(long topicId)
        {
            // Step 1: Retrieve the Topic
            Topic topic = await _context.Topics
                .Where(t => t.Id == topicId)
                .FirstOrDefaultAsync();

            if (topic == null)
            {
                throw new InvalidOperationException("Topic not found");
            }

            // Step 2: Remove the Topic from the associated Stage's Topics Collection
            Stage associatedStage = topic.Stage;
            associatedStage.Topics.Remove(topic);

            // Step 3: Mark the Topic as Removed from DbContext
            _context.Topics.Remove(topic);

            // Step 4: Save Changes to the Database
            await _context.SaveChangesAsync();
        }


        public async Task AddSubjectAsync(string subjectName)
        {
            try
            {
                var existingSubject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name.Equals(subjectName));

                if (existingSubject == null)
                {
                    var newSubject = new Subject(subjectName);
                    await _context.Subjects.AddAsync(newSubject);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task AddStageToSubjectAsync(long subjectId, Stage newStage)
        {
            // Step 1: Retrieve the Subject
            Subject subject = await _context.Subjects
                .Where(s => s.Id == subjectId)
                .Include(s => s.Stages)
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                throw new InvalidOperationException("Subject not found");
            }

            // Step 2: Create a New Stage (You can set other properties as needed)
            // Stage newStage = new Stage { Name = "New Stage Name", ... };

            // Step 3: Associate the Stage with the Subject
            newStage.Subject = subject;

            // Step 4: Add the Stage to the Subject's Stages Collection
            subject.Stages.Add(newStage);

            // Step 5: Save Changes to the Database
            await _context.SaveChangesAsync();
        }

        public async Task AddTopicToStageAsync(long stageId, Topic newTopic)
        {
            // Step 1: Retrieve the Stage
            Stage stage = await _context.Stages
                .Where(s => s.Id == stageId)
                .Include(s => s.Topics)
                .FirstOrDefaultAsync();

            if (stage == null)
            {
                throw new InvalidOperationException("Stage not found");
            }

            // Step 2: Create a New Topic (You can set other properties as needed)
            // Topic newTopic = new Topic { Name = "New Topic Name", ... };

            // Step 3: Associate the Topic with the Stage
            newTopic.Stage = stage;

            // Step 4: Add the Topic to the Stage's Topics Collection
            stage.Topics.Add(newTopic);

            // Step 5: Save Changes to the Database
            await _context.SaveChangesAsync();
        }


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
