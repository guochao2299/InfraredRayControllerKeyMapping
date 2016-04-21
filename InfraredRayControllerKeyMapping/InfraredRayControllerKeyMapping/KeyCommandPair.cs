using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace InfraredRayControllerKeyMapping
{
    [Serializable]
    public class KeyCommandPair
    {
        private string m_key = string.Empty;

        public string Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
            }
        }

        private string m_command = string.Empty;

        public string Command
        {
            get
            {
                return m_command;
            }
            set
            {
                m_command = value;
            }
        }
        public override bool Equals(object obj)
        {
            if(obj==null || (!(obj is KeyCommandPair)))
            {
                return false;
            }

            return string.Equals(this.Key,((KeyCommandPair)obj).Key, StringComparison.OrdinalIgnoreCase);
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
