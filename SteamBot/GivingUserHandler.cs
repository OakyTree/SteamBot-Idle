﻿using System;
using SteamKit2;
using System.Collections.Generic;
using SteamTrade;
using SteamTrade.Exceptions;
using System.Threading;

namespace SteamBot
{
    public class GivingUserHandler : UserHandler
    {
        public GivingUserHandler(Bot bot, SteamID sid, Configuration config) : base(bot, sid, config) 
        {
            Success = false;

            //Just makes referencing the bot's own SID easier.
            mySteamID = Bot.SteamUser.SteamID;
        }

        public override void OnLoginCompleted()
        {
            List<Inventory.Item> itemsToTrade = new List<Inventory.Item>();

            // Optional Crafting
            if (AutoCraftWeps)
            {
                AutoCraftAll();
                // Inventory must be up-to-date before trade
                Thread.Sleep(300);
            }

            // Must get inventory here
            Log.Info("Getting Inventory");
            Bot.GetInventory();

            itemsToTrade = GetAllNonCrates(Bot.MyInventory);
            if (!BotItemMap.ContainsKey(mySteamID))
            {
                BotItemMap.Add(mySteamID, itemsToTrade);
                Admins.Add(mySteamID);
            }

            Log.Info("[Giving] SteamID: " + mySteamID + " checking in. " + BotItemMap.Count + " of " + NumberOfBots + " Bots.");

            if (BotItemMap[mySteamID].Count > 0)
            {
                TradeReadyBots.Add(mySteamID);
                Log.Info("SteamID: " + mySteamID + " has items. Added to list." + TradeReadyBots.Count + " Bots waiting to trade.");
            }
            else
            {
                Log.Info("SteamID: " + mySteamID + " did not have a trade-worthy item.");
                Log.Info("Stopping bot.");
                Bot.StopBot();
            }
        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            //Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            //base.OnChatRoomMessage(chatID, sender, message);
        }

        public override bool OnFriendAdd()
        {
            if (IsAdmin)
            {
                return true;
            }
            return false;
        }

        public override void OnFriendRemove() { }

        public override void OnMessage(string message, EChatEntryType type) 
        {
            if ((OtherSID == PrimaryAltSID) && (message == "ready"))
            {
                Bot.SteamFriends.SendChatMessage(PrimaryAltSID, EChatEntryType.ChatMsg, "ready");
            }
        }

        public override bool OnTradeRequest()
        {
            Thread.Sleep(200);
            if (IsAdmin)
            {
                return true;
            }
            return false;
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Oh, there was an error: " + error + "."
                                              );
            Log.Warn(error);
        }

        public override void OnTradeTimeout()
        {
            //Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
            //                                  "Trade timeout.");
            Log.Warn("Trade timeout.");
            Log.Debug("Something's gone wrong.");
            Log.Info("Getting Inventory");
            Bot.GetInventory();

            if (GetAllNonCrates(Bot.MyInventory).Count > 0)
            {
                Log.Debug("Still have items to trade");
                //errorOcccured = true;
                CancelTrade();
                OnTradeClose();
            }
            else
            {
                Log.Debug("No items in inventory, removing");
                TradeReadyBots.Remove(mySteamID);
                CancelTrade();
                OnTradeClose();
                Bot.StopBot();
            }
        }

        public override void  OnTradeClose()
        {
            Log.Warn ("[Giving] TRADE CLOSED");
            Bot.CloseTrade ();
            // traded = true;
        }

        public override void OnTradeInit()
        {
            Thread.Sleep(500);
            Log.Debug("Adding all items.");
            uint added = AddItemsFromList(BotItemMap[mySteamID]);

            if (added > 0)
            {
                Log.Info("Added " + added + " items.");
                System.Threading.Thread.Sleep(50);
                if (!SendMessage("ready"))
                {
                    CancelTrade();
                    OnTradeClose();
                }
            }
            else
            {
                Log.Debug("Something's gone wrong.");
                Bot.GetInventory();
                if (GetAllNonCrates(Bot.MyInventory).Count > 0)
                {
                    Log.Debug("Still have items to trade, aborting trade.");
                    //errorOcccured = true;
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "failed");
                    OnTradeClose();
                }
                else
                {
                    Log.Debug("No items in bot inventory. This shouldn't be possible.");
                    TradeReadyBots.Remove(mySteamID);

                    CancelTrade();
                    OnTradeClose();
                    Bot.StopBot();
                }
            }
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeMessage(string message) 
        {
            System.Threading.Thread.Sleep(100);
            Log.Debug("Message Received: " + message);

            if (message == "ready")
            {
                if (!SetReady(true))
                {
                    CancelTrade();
                    OnTradeClose();
                }
            }
        }

        public override void OnTradeReady(bool ready)
        {
            Log.Debug("OnTradeReady");
            Thread.Sleep(100);

            if (ready && IsAdmin)
            {
                TradeAccept();
            }
        }

        public override void OnTradeAccept()
        {
            TradeReadyBots.Remove(mySteamID);
            OnTradeClose();
        }
        public void TradeAccept()
        {
            Thread.Sleep(100);
            Success = AcceptTrade();

            if (Success)
            {
                Log.Success("Trade was Successful!");
                //Trade.Poll();
                //Bot.StopBot();
            }
            else
            {
                Log.Warn("Trade might have failed.");
                Bot.GetInventory();

                if (GetAllNonCrates(Bot.MyInventory).Count == 0)
                {
                    Log.Warn("Bot has no items, trade may have succeeded. Removing bot.");
                    TradeReadyBots.Remove(mySteamID);
                    OnTradeClose();
                    Bot.StopBot();
                }
            }
        }
    }

}