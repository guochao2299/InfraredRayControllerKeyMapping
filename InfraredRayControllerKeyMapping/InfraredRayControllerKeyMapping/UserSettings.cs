using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace InfraredRayControllerKeyMapping
{
    [Serializable]
    public class UserSettings
    {
        public event EventHandler UserSettingsChanged;

        private string m_serialPort = string.Empty;

        public string SerialPort
        {
            get
            {
                return m_serialPort;
            }
            set
            {
                if (m_serialPort == value)
                {
                    return;
                }

                m_serialPort = value;

                if (UserSettingsChanged != null)
                {
                    UserSettingsChanged(this, null);
                }
            }
        }

        private bool m_isAutoSaveWhenAppClosed=false;
        
        public bool IsAutoSaveWhenAppClosed 
        {
            get
            {
                return m_isAutoSaveWhenAppClosed;
            }
            set
            {
                if (m_isAutoSaveWhenAppClosed == value)
                {
                    return;
                }

                m_isAutoSaveWhenAppClosed = value;

                if (UserSettingsChanged != null)
                {
                    UserSettingsChanged(this, null);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("端口:{0},系统关闭时自动保存:{1}",SerialPort,IsAutoSaveWhenAppClosed);
        }

        public const string UserSettingsFileName = "UserSettings.xml";

        public static UserSettings LoadFromFile()
        {
            System.IO.FileStream fs = null;
            try
            {                
                if (!System.IO.File.Exists(UserSettingsFileName))
                {
                    return new UserSettings();
                }

                fs = new System.IO.FileStream(UserSettingsFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                XmlSerializer xs = new XmlSerializer(typeof(UserSettings));
                return (UserSettings)xs.Deserialize(fs);
            }
            catch (Exception ex)
            {
                throw new Exception("从文件加载用户配置失败，错误消息为:" + ex.Message, ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public void Serialize2File()
        {
            System.IO.FileStream fs = null;
            try
            {
                if (System.IO.File.Exists(UserSettingsFileName))
                {
                    System.IO.File.Delete(UserSettingsFileName);
                }

                fs = new System.IO.FileStream(UserSettingsFileName, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);

                XmlSerializer xs = new XmlSerializer(typeof(UserSettings));
                xs.Serialize(fs, this);
            }
            catch (Exception ex)
            {
                throw new Exception("序列化到文件失败，错误消息为:" + ex.Message, ex);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
    }
}
