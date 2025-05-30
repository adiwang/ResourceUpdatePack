using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLUtils {
    public class Option {

        Dictionary<string, string> valueDict = new Dictionary<string, string>();

        public Option(string[] args) {
            foreach(string arg in args) {
                int index = arg.IndexOf('=');
                if(index < 0) {
                    valueDict.Add(arg.TrimStart('-'), "true");
                } else {
                    string key = arg.Substring(0, index).TrimStart('-');
                    string value = arg.Substring(index + 1).Trim();
                    valueDict.Add(key, value);
                }
            }
        }

        private Option()
		{}

		public static Option FromConfig(string configContent)
		{
			Option option = new Option();
			StringReader reader = new StringReader(configContent);
            while (true)
			{
				string line = reader.ReadLine();
				if (line == null)
					break;

				String trimedLine = line.Trim();
				if (trimedLine.Length == 0 || trimedLine[0] == '#')
					continue;

                string[] elements = trimedLine.Split('=');
                if(elements.Length < 2)
				{
					throw new Exception("bad config line: " + line);
                }

                string key = elements[0].Trim();
                string value = elements[1].Trim();

                option.SetValueIfAbsent(key, value);
            }
			return option;
		}

        public int Length
        {
            get { return valueDict.Count; }
        }

        public void SetValue(string name, string value) {
            if(valueDict.ContainsKey(name)) {
                valueDict[name] = value;
            } else {
                valueDict.Add(name, value);
            }
        }

        public void SetValueIfAbsent(string name, string value) {
            if(!valueDict.ContainsKey(name)) {
                valueDict.Add(name, value);
            }
        }

        public bool TryGetValue(string name, out int result) {
            string str;
            if(valueDict.TryGetValue(name, out str)) {
                if(int.TryParse(str, out result)) {
                    return true;
                }
            }
            result = 0;
            return false;
        }

        public bool TryGetValue(string name, out bool result) {
            string str;
            if(valueDict.TryGetValue(name, out str)) {
                if(bool.TryParse(str, out result)) {
                    return true;
                }
            }

            result = false;
            return false;
        }

        public bool TryGetValue(string name, out string result) {
            if(valueDict.TryGetValue(name, out result)) {
                return true;
            }

            result = null;
            return false;
        }

    }
}
