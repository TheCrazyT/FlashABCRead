using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip.Compression;
namespace FlashABCRead
{
    public class FlashRead
    {
        public static void ReadCompressed(Stream s, List<string> classnames)
        {
            long start = s.Position;
            BinaryReader br = new BinaryReader(s);
            byte[] mem = new byte[(int)s.Length + 8];
            byte[] buf = new byte[3];
            s.Read(buf, 0, 3);
            s.Seek(start+4, SeekOrigin.Begin);
            int size = br.ReadInt32();
            s.Seek(start+8, SeekOrigin.Begin);
            s.Read(mem, 0, (int)s.Length);
            s.Close();
            br.Close();

            try
            {
                s = new MemoryStream(mem);
                if (Encoding.Default.GetString(buf) == "CWS")
                {
                    Inflater i = new Inflater();
                    i.SetInput(mem);
                    byte[] mem2 = new byte[size + 8];
                    i.Inflate(mem2, 8, size);
                    s = new MemoryStream(mem2);
                    mem = new byte[0];
                }

                s.Seek(0x15, SeekOrigin.Begin);
                br = new BinaryReader(s);
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    uint taglen = br.ReadUInt16();
                    uint len = taglen & 0x3f;
                    uint tag = (taglen - len) / 64;
                    if (len == 63)
                        len = br.ReadUInt32();
                    start = br.BaseStream.Position;

                    if (tag == 82)
                    {
                        FlashABC fabc = new FlashABC(br.BaseStream, len);
                        fabc.FindClasses(classnames);
                    }
                    br.BaseStream.Seek(start + len, SeekOrigin.Begin);
                }
                br.Close();
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                return;
            }
        }
    }
    class FlashReadFile
    {
        Stream s;
        public FlashReadFile(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            byte[] mem = new byte[(int)fs.Length + 8];
            byte[] buf = new byte[3];
            fs.Read(buf, 0, 3);
            fs.Seek(4, SeekOrigin.Begin);
            int size = br.ReadInt32();
            fs.Seek(8, SeekOrigin.Begin);
            fs.Read(mem, 0, (int)fs.Length);
            fs.Close();
            br.Close();

            s = new MemoryStream(mem);
            if (Encoding.Default.GetString(buf) == "CWS")
            {
                Inflater i = new Inflater();
                i.SetInput(mem);
                byte[] mem2 = new byte[size + 8];
                i.Inflate(mem2, 8, size);
                s = new MemoryStream(mem2);
                mem = new byte[0];
            }
            s.Seek(0x15, SeekOrigin.Begin);
            br = new BinaryReader(s);
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                uint taglen = br.ReadUInt16();
                uint len = taglen & 0x3f;
                uint tag = (taglen - len) / 64;
                if (len == 63)
                    len = br.ReadUInt32();
                long start = br.BaseStream.Position;

                if (tag == 82)
                {
                    FlashABC fabc = new FlashABC(br.BaseStream, len);
                    List<string> classnames = new List<string>();
                    classnames.Add("cPlayerData");
                    fabc.FindClasses(classnames);
                }
                //Debug.Print("{0} {1}", tag, len+2);
                br.BaseStream.Seek(start + len, SeekOrigin.Begin);
            }
            fClass.InitClasses();
            br.Close();
        }
    }
}
