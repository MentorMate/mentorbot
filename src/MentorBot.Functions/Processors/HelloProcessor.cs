using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that return standard answers to trivial questions.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class HelloProcessor : ICommandProcessor
    {
        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => "Hello";

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var response = GetAnswer(info.TextSentenceChunk.ToLowerInvariant());
            return new ValueTask<ChatEventResult>(
                new ChatEventResult(response));
        }

        private static string GetAnswer(string question)
        {
            switch (question)
            {
                case "hi":
                case "hey":
                case "hello":
                case "good morning":
                case "good day":
                case "good evening":
                case "good afternoon":
                case "it’s nice to meet you":
                case "pleased to meet you":
                case "yo":
                case "howdy":
                case "sup":
                // cspell:disable-next-line
                case "whazzup":
                    return "Hello!";
                case "what’s up":
                case "what’s new":
                case "what’s going on":
                case "how’s everything":
                case "how are things":
                case "how’s life":
                case "how’s your day":
                case "how’s your day going":
                case "how have you been":
                case "how do you do":
                case "hiya":
                    return "Everything is great! Do have a command for me?";
                case "good to see you":
                case "nice to see you":
                    return "Thank you.";
                case "who are you":
                case "what are you":
                    return "I am your friendly neighborhood mentorbot.";
                case "where are you":
                    return "I am everywhere.";
                default:
                    return "Hello!";
            }
        }
    }
}
