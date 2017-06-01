﻿using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using MpRpServer.Data;
using MpRpServer.Server.Admin;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MpRpServer.Server
{
    public class ChatController : Script
    {         
        public ChatController()
        {
            API.onChatMessage += OnChatMessageHandler;
            API.onChatCommand += OnChatCommandHandler;
        }

        public static void SendMessageInMyGroup (Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            Character character = characterController.Character;
            string formatName = character.Name.Replace("_", " ");
            
            var allGroupPlayers = ContextFactory.Instance.Character.Where(x => x.GroupType == character.GroupType);
            
            foreach (var groupPlayer in allGroupPlayers)
            {
                if (groupPlayer.Online)
                {
                    Client target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == groupPlayer.SocialClub);
                    if (target == null) return;
                    API.shared.sendChatMessageToPlayer(target, "Игрок " + formatName + " говорит группе:\n" + message);
                }                
            }
        }

        public static void SendMessageInGroup(Client player, int group, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            Character character = characterController.Character;
            string formatName = character.Name.Replace("_", " ");

            var allGroupPlayers = ContextFactory.Instance.Character.Where(x => x.GroupType == character.DebtMafia);

            foreach (var groupPlayer in allGroupPlayers)
            {
                if (groupPlayer.Online)
                {
                    Client target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == groupPlayer.SocialClub);
                    if (target == null) continue;
                    API.shared.sendChatMessageToPlayer(target, "Игрок " + formatName + " говорит группе:\n" + message);
                }
            }
        }

        public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
        {
            
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            var formatName = characterController.Character.Name.Replace("_", " ");

            SendProxMessage(player, 15.0f, "~#FFFFFF~", formatName + " says: " + message);
            e.Cancel = true;
        }

        public void OnChatCommandHandler(Client player, string arg, CancelEventArgs ce)
        {
            if (API.getEntityData(player, "DOWNLOAD_FINISHED") != true) ce.Cancel = true;

            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            if (!arg.Contains("/login") && !arg.Contains("/register"))
            {
                if (characterController == null) ce.Cancel = true;
            }
            
        }

		public static void LoginMessages(Client player, Character character)
		{
			if (character.Admin > 0)
			{
				API.shared.sendChatMessageToPlayer(player, "~#FFCCBB~You are a " + AdminController.GetAdminRank(character.Admin) + " on this server. Please respect fellow staff and players.");
			}
		}

        public static void SendProxMessage(Client player, float radius, string color, string msg)
        {
            List<Client> proxPlayers = API.shared.getPlayersInRadiusOfPlayer(radius, player);
            foreach (Client target in proxPlayers)
            {
                API.shared.sendChatMessageToPlayer(target, color, msg);
            }
        }

        // Chat-related commands:
        [Command("me", GreedyArg = true)]
        public void ME_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");            
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " " + msg);
        }

        [Command("melow", GreedyArg = true)]
        public void MELow_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#C2A2DA~", formatName + " " + msg);
        }

        [Command("do", GreedyArg = true)] // do command 
        public void DO_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#C2A2DA~", "* " + message + " (( " + formatName + " ))");
        }

        [Command("dolow", GreedyArg = true)] // do command 
        public void DOLow_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#C2A2DA~", "* " + message + " (( " + formatName + " ))");
        }

        [Command("s", Alias = "shout", GreedyArg = true)]
        public void S_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 25.0f, "~#FFFFFF~", formatName + " shouts: " + message + "!");
        }
        [Command("w", Alias = "whisper", GreedyArg = true)]
        public void W_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#FFFFFF~", formatName + " whispers: " + message);
        }

        [Command("b", GreedyArg = true)]
        public void B_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#FFFFFF~", "(( " + formatName + ": " + msg + " ))");
        }

        [Command("o", GreedyArg = true)]
        public void OOC_Command(Client player, string msg)
        {
            foreach (Client c in API.getAllPlayers())
            {
                CharacterController characterController = player.getData("CHARACTER");
                var formatName = characterController.Character.Name.Replace("_", " ");
                API.sendChatMessageToPlayer(c, "~#FFFFFF~", "(( " + formatName + ": " + msg + " ))");
            }
        }
        
        [Command("pm", GreedyArg = true)]
        public void PmCommand(Client player, Client targetPlayer, string message)
        {
            var senderAccountController =  player.GetCharacterController();
            if (senderAccountController == null) return;

            var targetAccountController = targetPlayer.GetCharacterController();
            if (targetAccountController == null) return;

            //targetAccountController.SendPmMessage(senderAccountController, message);
        }
    }
}
