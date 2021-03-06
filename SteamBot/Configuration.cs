using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBot
{
    public class Configuration
    {
        public static Configuration LoadConfiguration (string filename)
        {
            TextReader reader = new StreamReader(filename);
            string json = reader.ReadToEnd();
            reader.Close();

            Configuration config =  JsonConvert.DeserializeObject<Configuration>(json);

            config.Admins = config.Admins ?? new ulong[0];
            int bots = config.Bots.Length;

            if (!config.AutoStartAllBots)
            {
                // Gets number of Bots by removing those that don't start
                foreach (BotInfo bot in config.Bots)
                {
                    if (!bot.AutoStart)
                    {
                        bots--;
                    }
                }
            }

            if (config.UseSeparateProcesses)
            {
                // Seperate Processes not currently supported.
                config.TotalBots = 0;
            }
            else
            {
                config.TotalBots = bots;
            }

            // None of this should be neccessary.
            //foreach (BotInfo bot in config.Bots)
            //{
            //    // merge bot-specific admins with global admins
            //    foreach (ulong admin in config.Admins)
            //    {
            //        if (!bot.Admins.Contains(admin))
            //        {
            //            bot.Admins.Add(admin);
            //        }
            //    }
            //}

            return config;
        }

        #region Top-level config properties
        
        /// <summary>
        /// Gets or sets the admins.
        /// </summary>
        /// <value>
        /// An array of Steam Profile IDs (64 bit IDs) of the users that are an 
        /// Admin of your bot(s). Each Profile ID should be a string in quotes 
        /// and separated by a comma. These admins are global to all bots 
        /// listed in the Bots array.
        /// </value>
        public ulong[] Admins { get; set; }

        /// <summary>
        /// Gets or sets the bots array.
        /// </summary>
        /// <value>
        /// The Bots object is an array of BotInfo objects containing
        ///  information about each individual bot you will be running. 
        /// </value>
        public BotInfo[] Bots { get; set; }

        /// <summary>
        /// Gets or sets YOUR API key.
        /// </summary>
        /// <value>
        /// The API key you have been assigned by Valve. If you do not have 
        /// one, it can be requested from Value at their Web API Key page. This
        /// is required and the bot(s) will not work without an API Key. 
        /// </value>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the main log file name.
        /// </summary>
        public string MainLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use separate processes.
        /// </summary>
        /// <value>
        /// <c>true</c> if bot manager is to open each bot in it's own process;
        /// otherwise, <c>false</c> to open each bot in a separate thread.
        /// Default is <c>false</c>.
        /// </value>
        public bool UseSeparateProcesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto start all bots.
        /// </summary>
        /// <value>
        /// <c>true</c> to make the bots start on program load; otherwise,
        /// <c>false</c> to not start them.
        /// </value>
        public bool AutoStartAllBots { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto craft all weapons in
        /// the bots' inventories.
        /// </summary>
        /// <value>
        /// <c>true</c> to make the bots craft any weapons before trading.
        /// <c>false</c> to not craft any weapons at all.
        /// </value>
        public bool AutoCraftWeapons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the total number of bots being loaded.
        /// </summary>
        public int TotalBots { get; set; }

        /// <summary>
        /// Gets or sets the user's custom options.
        /// </summary>
        /// <value>
        /// A Dictionary of custom options where the option name is the key.
        /// </value>
        public Optional Options { get; set; }

        #endregion Top-level config properties


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var fields = this.GetType().GetProperties();

            foreach (var propInfo in fields)
            {
                sb.AppendFormat("{0} = {1}" + Environment.NewLine,
                    propInfo.Name,
                    propInfo.GetValue(this, null));
            }

            return sb.ToString();
        }

        public class Optional
        {
            public int[] Crates { get; set; }
            public bool MyOption { get; set; }
        }

        public class BotInfo
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string DisplayName { get; set; }
            public string ChatResponse { get; set; }
            public string LogFile { get; set; }
            public string BotControlClass { get; set; }
            public int MaximumTradeTime { get; set; }
            public int MaximumActionGap { get; set; }
            public string DisplayNamePrefix { get; set; }
            public int TradePollingInterval { get; set; }
            public string LogLevel { get; set; }
            //public List<ulong> Admins { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to auto start this bot.
            /// </summary>
            /// <value>
            /// <c>true</c> to make the bot start on program load.
            /// </value>
            /// <remarks>
            /// If <see cref="SteamBot.Configuration.AutoStartAllBots "/> is true,
            /// then this property has no effect and is ignored.
            /// </remarks>
            [JsonProperty (Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue (true)]
            public bool AutoStart { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                var fields = this.GetType().GetProperties();

                foreach (var propInfo in fields)
                {
                    sb.AppendFormat("{0} = {1}" + Environment.NewLine,
                        propInfo.Name, 
                        propInfo.GetValue(this, null));
                }

                return sb.ToString();
            }
        }
    }
}
