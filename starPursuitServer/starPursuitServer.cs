using System;
using System.Linq;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace starPursuitServer
{
    public class StarPursuitServer : BaseScript
    {
        public StarPursuitServer()
        {
            EventHandlers.Add("chatMessage", new Action<int, int, string, string>(ChatMessage));
        }

        private void ChatMessage([FromSource]int sourceCID, int sourceSID, string sourceName, string message)
        {
            string[] splitMessage = message.Split(' ');
            if(splitMessage[0] == "/etracker")
            {
                if (splitMessage.Count() >= 2)
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "cop:eTracker", Convert.ToInt32(splitMessage[1]));
                }
                else
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Syntax, use: /etracker <Tracker ID>");
                }
                CancelEvent();
            }
            else if(splitMessage[0] == "/ctrackers")
            {
                TriggerClientEvent(GetPlayerFromSID(sourceSID), "cop:cTrackers");
                CancelEvent();
            }
            else if (splitMessage[0] == "/ctracker")
            {
                if (splitMessage.Count() >= 2)
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "cop:cTracker", Convert.ToInt32(splitMessage[1]));
                }
                else
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Syntax, use: /ctracker <Tracker ID>");
                }
                CancelEvent();
            }
            else if (splitMessage[0] == "/dtracker")
            {
                if (splitMessage.Count() >= 2)
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "cop:dTracker", Convert.ToInt32(splitMessage[1]));
                }
                else
                {
                    TriggerClientEvent(GetPlayerFromSID(sourceSID), "chatMessage", "Star Pursuit", new[] { 255, 0, 0 }, "Invalid Syntax, use: /dtracker <Tracker ID>");
                }
                CancelEvent();
            }
            else if (splitMessage[0] == "/ltrackers")
            {
                TriggerClientEvent(GetPlayerFromSID(sourceSID), "cop:lTrackers");
                CancelEvent();
            }
        }

        private Player GetPlayerFromSID(int id)
        {
            Player playerToReturn = null;
            foreach (Player player in new PlayerList())
            {
                if (Convert.ToInt32(player.Handle) == id)
                    playerToReturn = player;
            }
            return playerToReturn;
        }
    }
}
