using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class QuizQuestion
    {
        public QuizQuestion(string question, List<string> options, string answer)
        {
            Question = question;
            Options = options;
            Answer = answer;
        }

        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string Answer { get; set; }
    }
}
