using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace InfraredRayControllerKeyMapping
{
    [Serializable]
    public class InfraredRayController
    {
        private string m_name = string.Empty;

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        private List<KeyCommandPair> m_keys = new List<KeyCommandPair>();

        public List<KeyCommandPair> KeyCommands
        {
            get
            {
                return m_keys;
            }
            set
            {
                m_keys = value;
            }
        }

        private const string FileExtention = ".irc";
        private const string DirName = "IRC";

        public void Serialize2File()
        {
            System.IO.FileStream fs = null;
            try
            {
                string dirPath = Path.Combine(Application.StartupPath, DirName);

                if (Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string fileName=string.Format("{0}{1}",this.Name,FileExtention);
                string filePath = Path.Combine(dirPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                fs = new System.IO.FileStream(filePath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);

                XmlSerializer xs = new XmlSerializer(typeof(InfraredRayController));
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

        public static Dictionary<string,InfraredRayController> LoadIRCsFromFile()
        {
            System.IO.FileStream fs = null;
            try
            {
                string dirPath = Path.Combine(Application.StartupPath, DirName);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                string fileName = string.Format("*{0}", FileExtention);

                string[] files = Directory.GetFiles(dirPath, fileName);

                if (files == null || files.Length <= 0)
                {
                    return new Dictionary<string, InfraredRayController>();
                }

                Dictionary<string, InfraredRayController> results=new Dictionary<string,InfraredRayController>();

                foreach (string s in files)
                {
                    fs = new System.IO.FileStream(s, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    XmlSerializer xs = new XmlSerializer(typeof(InfraredRayController));
                    InfraredRayController irc = (InfraredRayController)xs.Deserialize(fs);
                    results.Add(irc.Name, irc);
                    fs.Close();
                }

                return results;
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
