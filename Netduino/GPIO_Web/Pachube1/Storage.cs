using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using System.Collections;
using System.Text;
using System.IO;
using astra.http;

public class Storage
{
    protected Hashtable contents = null;
    protected const String root = @"\SD\";
    protected String path;

    public Storage(String path = "")
    {
        this.path = path;
    }

    public String get(String key)
    {
        return contents == null ? null : (String)contents[key];
    }

    public void put(String key, String value)
    {
        if (contents == null)
            contents = new Hashtable();
        contents[key] = value;
    }

    public void clear()
    {
        if (contents == null)
            contents = new Hashtable();
        contents.Clear();
    }

    public void Load()
    {
        Load(path);
    }

    public void Save()
    {
        Save(path);
    }

    public Boolean Load(String path)
    {
        try
        {
            this.path = path;
            path = root + path;
            FileInfo fInfo = new FileInfo(path);
            if (fInfo.Exists && fInfo.Length < 2048)
            {
                int size = (int)fInfo.Length;
                FileStream fStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, size);
                byte[] buffer = new byte[size];
                fStream.Read(buffer, 0, buffer.Length);
                contents = new Hashtable();
                parseContents(new String(UTF8Encoding.UTF8.GetChars(buffer)));
                fStream.Close();
                return true;
            }
            else
                return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    protected void createDirectories(String path)
    {
        int index;
        index = path.IndexOf('\\');
        path = path.Substring(0, index);
        path = root + path;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public void Save(String path)
    {
        createDirectories(path);
        this.path = path;
        path = root + path;
        FileInfo fInfo = new FileInfo(path);
        StringBuilder sb = new StringBuilder(128);
        {
            FileStream fStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            foreach (String key in contents.Keys)
            {
                sb.Append(key + '=' + (String)contents[key] + "\n");
            }
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(sb.ToString());
            fStream.Write(buffer, 0, buffer.Length);
            fStream.Close();
        }
    }

    public void parseContents(String s)
    {
        int i0, i1;
        while (s.Length != 0)
        {
            if ((i0 = s.IndexOf('=')) != -1)
            {
                if ((i1 = s.IndexOf('\n')) != -1)
                {
                    if(i1 - i0 - 1 == 0)
                        setAttribute(s.Substring(0, i0), "");
                    else
                        setAttribute(s.Substring(0, i0), s.Substring(i0 + 1, i1 - i0 - 1));
                    s = s.Substring(i1 + 1);
                }
                else
                {
                    setAttribute(s.Substring(0, i0), s.Substring(i0 + 1));
                    break;
                }
            }
            else
                break;
        }
    }

    protected void setAttribute(String attribute, String value)
    {
        attribute = attribute.Trim();
        value = value.Trim();
        if (contents.Contains(attribute))
            contents.Remove(attribute);
        contents.Add(attribute.Trim(), value.Trim());
    }
}
