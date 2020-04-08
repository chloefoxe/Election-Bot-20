using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Bots
{
    // Class for storing a log of utterances (text of messages) as a list.
    public class UtteranceLog : IStoreItem
    {
        // A list of things that users have said to the bot
        public List<string> UtteranceList { get; } = new List<string>();

        // The number of conversational turns that have occurred        
        public int TurnNumber { get; set; } = 0;

        // Create concurrency control where this is used.
        public string ETag { get; set; } = "*";
    }
}
