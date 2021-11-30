using Microsoft.AspNetCore.SignalR;
using Project.Constants;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Hubs
{
    public class QuizHub : Hub
    {
        public static Dictionary<string, string> UserToRoomDict = new Dictionary<string, string>();

        public static Dictionary<string, int> RoomToUserCountDict = new Dictionary<string, int>();

        public static Dictionary<string, int> UserToScoreDict = new Dictionary<string, int>();

        public static Dictionary<string, int> UserToQuestionIndexDict = new Dictionary<string, int>();

        public static Dictionary<string, int> RoomToQuestionsCountDict = new Dictionary<string, int>();

        public static Dictionary<string, bool> RoomToGameStateDict = new Dictionary<string, bool>();

        public static Dictionary<string, List<int>> RoomToQuestionsListDict = new Dictionary<string, List<int>>();

        public static Dictionary<string, int> RoomToPlayAgainDict = new Dictionary<string, int>();

        public static List<QuizQuestion> StandardQuizQues = GetQuizQuestions();

        /// <summary>
        /// To initiate the processes to score the user based on the answer, update the count of questions answered in the group, generating result if the game has concluded, and
        /// send next question to the players.
        /// </summary>
        /// <param name="groupName">Group name where answer is sent.</param>
        /// <param name="message">Answer sent by user.</param>
        /// <returns></returns>
        public async Task SendAnswer(string groupName, string message)
        {
            try
            {
                int prevOptionQuestion = 0;
                if (UserToQuestionIndexDict.ContainsKey(Context.ConnectionId))
                {
                    prevOptionQuestion = UserToQuestionIndexDict[Context.ConnectionId];
                }

                await VerifyAnswerAndScoreUpdate(groupName, message, prevOptionQuestion);

                UpdateNumberOfQuestionsAnsweredCount(groupName);

                if (UserToQuestionIndexDict.ContainsKey(Context.ConnectionId))
                {
                    int questionIndex = UserToQuestionIndexDict[Context.ConnectionId];

                    if (questionIndex + 1 == 5)
                    {
                        if (HasGameConcluded(groupName))
                        {
                            await GeneratingResultAndConcludingGame(groupName);
                        }
                        else
                        {
                            try
                            {
                                await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "Please wait for the other person to finish");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception occured: ", e);
                            }
                        }

                    }
                    else
                    {
                        // Updating next question index in the dictionary.
                        UserToQuestionIndexDict[Context.ConnectionId] = questionIndex + 1;

                        await GenerateAndSendNextQuestion(groupName, questionIndex);
                    }
                }
                else
                {
                    // Updating next question index in the dictionary.
                    UserToQuestionIndexDict.Add(Context.ConnectionId, 1);

                    await GenerateAndSendNextQuestion(groupName, 0);
                }
            } catch(Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }

        }

        /// <summary>
        /// To generate and send next question when game is still in progress.
        /// </summary>
        /// <param name="groupName">Group name whose question is to be generated.</param>
        /// <param name="questionIndex">Last question index.</param>
        /// <returns></returns>
        private async Task GenerateAndSendNextQuestion(string groupName, int questionIndex)
        {
            try
            {
                // Generating next question.
                string question = GenerateQuestion(groupName, questionIndex + 1);
                // Sending next question.
                await Clients.Caller.SendAsync(EventConstants.ReceiveQuestion, $"{questionIndex + 2}. {question}");
            } catch(Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }

        /// <summary>
        /// To Generate result and conclude game.
        /// </summary>
        /// <param name="groupName">Group name whose result is to be generated.</param>
        /// <returns></returns>
        private async Task GeneratingResultAndConcludingGame(string groupName)
        {
            try
            {
                List<string> usersInGroup = GetUsersInGroup(groupName);

                // Get maximum score of the group.
                int max = 0;
                foreach (string user in usersInGroup)
                {
                    if (UserToScoreDict[user] > max)
                    {
                        max = UserToScoreDict[user];
                    }
                }

                // Send result to each user in group.
                foreach (string user in usersInGroup)
                {
                    try
                    {
                        if (UserToScoreDict[user] == max)
                        {
                            UserToScoreDict.Remove(user);
                            UserToQuestionIndexDict.Remove(user);
                            await Clients.Client(user).SendAsync(EventConstants.RecieveResult, "Congratulations! You Won!");
                        }
                        else
                        {
                            UserToScoreDict.Remove(user);
                            UserToQuestionIndexDict.Remove(user);
                            await Clients.Client(user).SendAsync(EventConstants.RecieveResult, "Sorry, You Lose!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                    }
                }

                // Resetting room details in dictionary.
                RoomToQuestionsCountDict.Remove(groupName);
                RoomToQuestionsListDict.Remove(groupName);
                RoomToGameStateDict[groupName] = false;
            } catch(Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }


        /// <summary>
        /// To update the count of number of questions answered in the group.
        /// </summary>
        /// <param name="groupName">Group name whose question count to be updated.</param>
        private static void UpdateNumberOfQuestionsAnsweredCount(string groupName)
        {
            try
            {
                lock (RoomToQuestionsCountDict)
                {
                    if (RoomToQuestionsCountDict.ContainsKey(groupName))
                    {
                        int questionAnsweredCount = RoomToQuestionsCountDict[groupName];
                        RoomToQuestionsCountDict[groupName] = ++questionAnsweredCount;
                    }
                    else
                    {
                        RoomToQuestionsCountDict.Add(groupName, 1);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }

        /// <summary>
        /// Verify answer and update score of user whose answer is correct.
        /// </summary>
        /// <param name="groupName">Group name where question is answered.</param>
        /// <param name="message">Answer sent by the user.</param>
        /// <param name="questionAnsweredIndex">Index of answered question.</param>
        /// <returns></returns>
        private async Task VerifyAnswerAndScoreUpdate(string groupName, string message, int questionAnsweredIndex)
        {
            try
            {
                // Verifying and updating score.
                string answer = GetAnswer(groupName, questionAnsweredIndex);
                if (message.Equals(answer))
                {
                    if (UserToScoreDict.ContainsKey(Context.ConnectionId))
                    {
                        int score = UserToScoreDict[Context.ConnectionId];
                        UserToScoreDict[Context.ConnectionId] = ++score;
                        await Clients.Caller.SendAsync(EventConstants.ReceiveScore, score);
                    }
                    else
                    {
                        UserToScoreDict.Add(Context.ConnectionId, 1);
                        await Clients.Caller.SendAsync(EventConstants.ReceiveScore, 1);
                    }
                }
                else
                {
                    // Sending initial score to user incase the answer in incorrect.
                    if (!UserToScoreDict.ContainsKey(Context.ConnectionId))
                    {
                        UserToScoreDict.Add(Context.ConnectionId, 0);
                        await Clients.Caller.SendAsync(EventConstants.ReceiveScore, 0);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }

        /// <summary>
        /// To add user to a group.
        /// </summary>
        /// <param name="user">Name of user.</param>
        /// <param name="groupName">Name of group which user wants to join.</param>
        /// <returns></returns>
        public async Task AddToGroup(String user, string groupName)
        {
            if(user != null && user.Length > 0 && groupName != null && groupName.Length > 0)
            {
                if (await IsUserJoinOperationInvalid(groupName))
                {
                    return;
                }

                try
                {
                    lock (RoomToUserCountDict)
                    {
                        if (RoomToUserCountDict.ContainsKey(groupName))
                        {
                            int userCount = RoomToUserCountDict[groupName];

                            if (userCount < 2)
                            {
                                Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                                UserToRoomDict.Add(Context.ConnectionId, groupName);
                                RoomToUserCountDict[groupName] = ++userCount;
                                Clients.Group(groupName).SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"{user} has joined the group {groupName}");

                                if (userCount == 2)
                                {
                                    GenerateQuestionListAndStartGame(groupName);
                                }
                            }
                            else
                            {
                                Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "Game already begin");
                            }
                        }
                        else
                        {
                            Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                            UserToRoomDict.Add(Context.ConnectionId, groupName);
                            RoomToUserCountDict.Add(groupName, 1);
                            Clients.Group(groupName).SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"{user} has joined the group {groupName}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: ", e);
                }
                
            }
        }

        /// <summary>
        /// To check if the user join group request in valid or not.
        /// </summary>
        /// <param name="groupName">Group name for which user join request will be validated.</param>
        /// <returns>true if user's join is invalid; otherwise, false.</returns>
        private async Task<bool> IsUserJoinOperationInvalid(string groupName)
        {
            bool isValidationFailed = false;

            try
            {
                if (UserToRoomDict.ContainsKey(Context.ConnectionId))
                {
                    if (UserToRoomDict[Context.ConnectionId].Equals(groupName))
                    {
                        await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "You are already in this group");
                    }
                    else
                    {
                        string prevRoom = UserToRoomDict[Context.ConnectionId];
                        await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"Please leave group {prevRoom} to join new group");
                    }

                    isValidationFailed = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }

            return isValidationFailed;
        }

        /// <summary>
        /// To remove user from thr group.
        /// </summary>
        /// <param name="user">Name of the user to be removed from the group.</param>
        /// <param name="groupName">Name of group from which user is to be removed.</param>
        /// <returns></returns>
        public async Task RemoveFromGroup(String user, string groupName)
        {
            if (user != null && user.Length > 0 && groupName != null && groupName.Length > 0)
            {
                if (await IsLeaveRoomInvalid(groupName))
                {
                    return;
                }

                try
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                    try
                    {
                        await Clients.Group(groupName).SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"{user} has left the group {groupName}");
                        await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"You have left the group {groupName}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                    }

                    lock (RoomToUserCountDict)
                    {
                        if (RoomToUserCountDict.ContainsKey(groupName))
                        {
                            int userCount = RoomToUserCountDict[groupName];

                            // If user leaves between the game.
                            if (RoomToGameStateDict.ContainsKey(groupName) && RoomToGameStateDict[groupName])
                            {
                                EndingGameWhenUserLeave(groupName);
                            }

                            RoomToPlayAgainDict.Remove(groupName);
                            RoomToUserCountDict[groupName] = --userCount;

                            if (userCount == 0)
                            {
                                RoomToUserCountDict.Remove(groupName);
                                RoomToGameStateDict.Remove(groupName);
                            }
                        }
                    }

                    UserToRoomDict.Remove(Context.ConnectionId);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: ", e);
                }
                
            }

        }

        /// <summary>
        /// To validate user's leave group request.
        /// </summary>
        /// <param name="groupName">Name of group which user is requesting to leave.</param>
        /// <returns>true if user's leave group request is invalid; otherwise, false.</returns>
        private async Task<bool> IsLeaveRoomInvalid(string groupName)
        {
            bool isLeaveRoomInvalid = false;

            try
            {
                if (UserToRoomDict.ContainsKey(Context.ConnectionId))
                {
                    if (!UserToRoomDict[Context.ConnectionId].Equals(groupName))
                    {
                        string prevRoom = UserToRoomDict[Context.ConnectionId];
                        await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"You are attempting to leave a wrong group, please try again with group {prevRoom}");
                        isLeaveRoomInvalid = true;
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"You have already left the group {groupName}");
                    isLeaveRoomInvalid = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
            return isLeaveRoomInvalid;
        }

        /// <summary>
        /// To end game if user leave group in between of a game.
        /// </summary>
        /// <param name="groupName">Name of group which user request to leave.</param>
        private void EndingGameWhenUserLeave(string groupName)
        {
            try
            {
                List<string> usersInGroup = GetUsersInGroup(groupName);

                foreach (string u in usersInGroup)
                {
                    try
                    {
                        if (u.Equals(Context.ConnectionId))
                        {
                            UserToScoreDict.Remove(u);
                            UserToQuestionIndexDict.Remove(u);
                            Clients.Client(u).SendAsync(EventConstants.StopGame, "Sorry, You Lose!");
                        }
                        else
                        {
                            UserToScoreDict.Remove(u);
                            UserToQuestionIndexDict.Remove(u);
                            Clients.Client(u).SendAsync(EventConstants.StopGame, "Congratulations! You Won!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occured: ", e);
                    }

                }

                RoomToQuestionsCountDict.Remove(groupName);
                RoomToQuestionsListDict.Remove(groupName);
                RoomToGameStateDict[groupName] = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }

        /// <summary>
        /// To get all the users in the group.
        /// </summary>
        /// <param name="groupName">Group name whose list of users is required.</param>
        /// <returns>List of users who are part of the mentioned group.</returns>
        private static List<string> GetUsersInGroup(string groupName)
        {
            List<string> usersInGroup = new List<string>();
            foreach (KeyValuePair<string, string> entry in UserToRoomDict)
            {
                if (entry.Value.Equals(groupName))
                {
                    usersInGroup.Add(entry.Key);
                }
            }

            return usersInGroup;
        }

        /// <summary>
        /// To handle requests if users want to play the quiz again.
        /// </summary>
        /// <param name="groupName">Group name where users want to play the quiz again.</param>
        public void PlayAgain(string groupName)
        {
           if (IsPlayAgainRequestInvalid(groupName))
            {
                return;
            }

            lock (RoomToPlayAgainDict)
            {
                try
                {
                    if (RoomToPlayAgainDict.ContainsKey(groupName))
                    {
                        int playAgainCount = RoomToPlayAgainDict[groupName];
                        RoomToPlayAgainDict[groupName] = ++playAgainCount;

                        // If both users want to play the game again.
                        if (playAgainCount == 2)
                        {
                            GenerateQuestionListAndStartGame(groupName);
                        }
                        else
                        {
                            Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "Waiting for other player response");
                        }
                    }
                    else
                    {
                        RoomToPlayAgainDict.Add(groupName, 1);
                        Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "Waiting for other player response");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: ", e);
                }

            }

        }

        /// <summary>
        /// To validate the user's play again request.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>true if user's play again request is invalid; otherwise, false.</returns>
        private bool IsPlayAgainRequestInvalid(string groupName)
        {
            bool isPlayAgainRequestInvalid = false;

            try
            {
                if (UserToRoomDict.ContainsKey(Context.ConnectionId))
                {
                    // If user is sending play again request in wrong group.
                    if (!UserToRoomDict[Context.ConnectionId].Equals(groupName))
                    {
                        string prevRoom = UserToRoomDict[Context.ConnectionId];
                        Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, $"You have sent play again request for wrong group, please send it to group {prevRoom} again");
                        Clients.Caller.SendAsync(EventConstants.EnablePlayAgainButton);
                        isPlayAgainRequestInvalid = true;
                    }
                }
                else
                {
                    Clients.Caller.SendAsync(EventConstants.ReceiveMessage, EventConstants.Admin, "You have left the group, please join the same group again to play");
                    isPlayAgainRequestInvalid = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
            return isPlayAgainRequestInvalid;
        }

        /// <summary>
        /// To generate random questions list and start the game. 
        /// </summary>
        /// <param name="groupName">Group name where game is required to get started.</param>
        private void GenerateQuestionListAndStartGame(string groupName)
        {
            try
            {
                // Getting list of unique random index for questions list.
                List<int> questionRandIndexList = GetUniqueRandomIndexList();

                if (RoomToQuestionsListDict.ContainsKey(groupName))
                {

                    RoomToQuestionsListDict[groupName] = questionRandIndexList;
                }
                else
                {
                    RoomToQuestionsListDict.Add(groupName, questionRandIndexList);
                }

                // Generating first question.
                string question = GenerateQuestion(groupName, 0);

                Clients.Group(groupName).SendAsync(EventConstants.ReceiveQuestion, $"1. {question}");

                // Setting game state for the group to running.
                if (RoomToGameStateDict.ContainsKey(groupName))
                {
                    RoomToGameStateDict[groupName] = true;
                }
                else
                {
                    RoomToGameStateDict.Add(groupName, true);
                }

                // Clearing play again requests for the group.
                RoomToPlayAgainDict.Remove(groupName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
        }

        /// <summary>
        /// To handle user disconnection.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                if (UserToRoomDict.ContainsKey(Context.ConnectionId))
                {
                    // If user belongs to a group, then removing user from that group.
                    await RemoveFromGroup("Other player", UserToRoomDict[Context.ConnectionId]);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: ", e);
            }
            
        }

        /// <summary>
        /// To check if the game has ended.
        /// </summary>
        /// <param name="groupName">Name of the group where game conclusion is to be checked.</param>
        /// <returns>true if game has ended; otherwise, false.</returns>
        public bool HasGameConcluded(string groupName)
        {
            // returns true if all the users in the group have answered all the questions.
            return RoomToQuestionsCountDict[groupName] == (5 * RoomToUserCountDict[groupName]);
        }

        /// <summary>
        /// To get list of distinct random indexes.
        /// </summary>
        /// <returns>List of distinct random indexes.</returns>
        public List<int> GetUniqueRandomIndexList()
        {
            Random rand = new Random();
            List<int> possible = Enumerable.Range(0, 11).ToList();
            List<int> listNumbers = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                int index = rand.Next(0, possible.Count);
                listNumbers.Add(possible[index]);
                possible.RemoveAt(index);
            }

            return listNumbers;
        }

        /// <summary>
        /// To generate the question once the game has started in a client readable form.
        /// </summary>
        /// <param name="groupName">Name of group for which question is to be generated.</param>
        /// <param name="index">Question number - 1</param>
        /// <returns>Question in a client readable form.</returns>
        public string GenerateQuestion(string groupName, int index)
        {
            List<int> quizQuestionRandIndexes = RoomToQuestionsListDict[groupName];

            QuizQuestion quizQuestion = StandardQuizQues[quizQuestionRandIndexes[index]];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                if(i == 0)
                {
                    sb.Append(quizQuestion.Question);   
                } else
                {
                    sb.Append("|");
                    sb.Append(quizQuestion.Options[i - 1]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// To get the answer of the question.
        /// </summary>
        /// <param name="groupName">Name of the group in which the question is asked.</param>
        /// <param name="index">Question number - 1</param>
        /// <returns>Answer of the question.</returns>
        public string GetAnswer(string groupName, int index)
        {
            List<int> quizQuestionRandIndexes = RoomToQuestionsListDict[groupName];

            QuizQuestion quizQuestion = StandardQuizQues[quizQuestionRandIndexes[index]];

            return quizQuestion.Answer;
        }

        /// <summary>
        /// To get the list of quiz questions.
        /// </summary>
        /// <returns>List of quiz questions.</returns>
        public static List<QuizQuestion> GetQuizQuestions()
        {
            List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

            List<string> options = new List<string>
            {
                "1901",
                "1893",
                "1895",
                "1898"
            };

            quizQuestions.Add(new QuizQuestion("When was the first Nobel Prize awarded?", options, "1901"));

            options = new List<string>
            {
                "Bob Dylan",
                "Kazuo Ishiguro",
                "Peter Handke",
                "Louise Glück"
            };

            quizQuestions.Add(new QuizQuestion("Who won the Nobel Prize 2020 in Literature?", options, "Louise Glück"));

            options = new List<string>
            {
                "Physiology or Medicine",
                "Literature",
                "Peace prize",
                "Economic Science"
            };

            quizQuestions.Add(new QuizQuestion("Harvey J. Alter, Michael Houghton and Charles M. Rice won Nobel Prize 2020 in which field?", options, "Physiology or Medicine"));

            options = new List<string>
            {
                "George Smith, Frances Arnold and Greg Winter",
                "Emmanuelle Charpentier and Jennifer A. Doudna",
                "Jacques Dubochet, Joachim Frank and Richard Henderson",
                "John B. Goodenough, M. Stanley Whittingham and Akira Yoshino"
            };

            quizQuestions.Add(new QuizQuestion("Who won the Nobel Prize for discovering the method for genome editing?", options, "Emmanuelle Charpentier and Jennifer A. Doudna"));

            options = new List<string>
            {
                "Arthur Ashkin, Arthur Ashkin and Donna Strickland",
                "Rainer Weiss, Barry C. Barish and Kip S. Thorne",
                "Roger Penrose, Reinhard Genzel and Andrea Ghez",
                "James Peebles, Michel Mayor and Didier Queloz"
            };

            quizQuestions.Add(new QuizQuestion("Who won the Nobel Prize in Physics 2020?", options, "Roger Penrose, Reinhard Genzel and Andrea Ghez"));

            options = new List<string>
            {
                "Mother Teresa",
                "Shirin Ebadi",
                "Marie Curie",
                "Elizabeth H. Blackburn"
            };

            quizQuestions.Add(new QuizQuestion("Who was the first woman to win a Nobel Prize?", options, "Marie Curie"));

            options = new List<string>
            {
                "Malala Yousafzai",
                "Arthur Ashkin",
                "James P. Allison",
                "Yoshinori Ohsumi"
            };

            quizQuestions.Add(new QuizQuestion("Name the youngest Laureate who received the Nobel Peace Prize in 2014.", options, "Malala Yousafzai"));

            options = new List<string>
            {
                "Medicine",
                "Economics",
                "Physics",
                "Literature"
            };

            quizQuestions.Add(new QuizQuestion("Which of the following fields was not included in the Nobel Prize category at the time the Nobel Prizes were first established?", options, "Economics"));

            options = new List<string>
            {
                "Albert Einstein",
                "Enrico Fermi",
                "Stephen Hawking",
                "Shuji Nakamura"
            };

            quizQuestions.Add(new QuizQuestion("Which of the following famous physicists has not received a Nobel Prize yet?", options, "Stephen Hawking"));

            options = new List<string>
            {
                "4",
                "2",
                "3",
                "6"
            };

            quizQuestions.Add(new QuizQuestion("What is the maximum number of people who can share a Nobel Prize?", options, "3"));

            options = new List<string>
            {
                "10 December",
                "10 October",
                "10 January",
                "10 November"
            };

            quizQuestions.Add(new QuizQuestion("When does the formal Nobel Prize ceremony take place every year?", options, "10 December"));

            options = new List<string>
            {
                "International Campaign to Abolish Nuclear Weapons (ICAN)",
                "Abiy Ahmed Ali",
                "World Food Programme (WFP)",
                "Juan Manuel Santos"
            };

            quizQuestions.Add(new QuizQuestion("Nobel Peace Prize 2020 has been awarded to:", options, "World Food Programme (WFP)"));

            return quizQuestions;
        }
    }
}