using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace NanoMite
{
    public class Mite
    {
        
        public JObject Configuration { get; set;}
        
        protected Mite()
        {
        }
                
        public IEnumerable<string> GetHelpParameters()
        {
            IEnumerable<string> results = new List<string>();
            try
            {
                results = GetHelpParametersCore();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return results;
        }

        protected virtual IEnumerable<string> GetHelpParametersCore()
        {
            var helpParameters = new Collection<string>();
            return helpParameters;
        }

        public IEnumerable<string> Checkup()
        {
            IEnumerable<string> results = new List<string>();
            try
            {
                results = CheckupCore();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return results;
        }

        protected virtual IEnumerable<string> CheckupCore()
        {
            var checkupIssues = new Collection<string>();
            return checkupIssues;
        }

        public IEnumerable<string> Heartbeat()
        {
            IEnumerable<string> results = new List<string>();
            try
            {
                results = HeartbeatCore();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return results;
        }

        protected virtual IEnumerable<string> HeartbeatCore()
        {
            var heartbeatItems = new Collection<string>();
            return heartbeatItems;
        }

        public bool CustomAction(string customAction)
        {
            var results = false;
            try
            {
                results = CustomActionCore(customAction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return results;
        }

        protected virtual bool CustomActionCore(string customAction)
        {
            return false;
        }
    }
}