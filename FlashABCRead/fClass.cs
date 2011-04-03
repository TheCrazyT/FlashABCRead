using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace FlashABCRead
{
    public class fClass 
    {
        private static List<fClass> FoundClasses = new List<fClass>();
        private static Hashtable offsets = new Hashtable();
        private static Hashtable propertyTypes = new Hashtable();
        public string name;
        public string superclass;
        public long classPosition;
        public List<cProp> properties;
        public MemoryStream classStream;
        private bool disposed = false;
        public long getOffset()
        {
            return this.classPosition;
        }
        public static int Count
        {
            get
            {
                return FoundClasses.Count;
            }
        }
        public fClass()
        {

        }
        public string gSTR(string property)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return getSTR(property, name);
                }
            }
        }
        public fClass gC(string property)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return getC(property, name);
                }
            }
        }
        public double gDBL(string property)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return getDBL(property, name);
                }
            }
        }
        public uint gUINT(string property)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return (uint)this.get(property, name);
                }
            }
        }
        public int gINT(string property)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return this.get(property, name);
                }
            }
        }
        public string getType(string property)
        {
            string s = (string)propertyTypes[name + "." + property];
            if (s == null) Debugger.Break();
            return s;
        }
        public List<fClass> getClassList(string type)
        {
            List<fClass> list = new List<fClass>();
            long pos = classStream.Position;
            classStream.Seek(this.classPosition, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(classStream);

            classStream.Seek(0x10, SeekOrigin.Current);

            uint cnt = br.ReadUInt32();
            if (cnt > 0x10000000) Debugger.Break();
            br.ReadUInt32();
            br.ReadUInt32();
            uint offset = br.ReadUInt32();


            for (int x = 0; x < cnt; x++)
            {
                classStream.Seek(offset+x*4, SeekOrigin.Begin);
                classStream.Seek(br.ReadUInt32() & 0xFFFFFFF8, SeekOrigin.Begin);
                list.Add(new fClass(classStream, type));
            }

            classStream.Seek(pos, SeekOrigin.Begin);
            return list;

        }


        private string getSTR(string property, string type)
        {
            string[] steps = property.Split('.');
            long pos = classStream.Position;
            if (steps.Length == 1)
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + property), SeekOrigin.Current);
                string s="";
                uint off = br.ReadUInt32();
                classStream.Seek(off+0x08, SeekOrigin.Begin);
                
                uint reloff = br.ReadUInt32();
                off = br.ReadUInt32();
                int size = br.ReadInt32();
                if(off!=0)
                {
                    byte[] mem = new byte[size];
                    classStream.Seek(off, SeekOrigin.Begin);
                    classStream.Seek(8, SeekOrigin.Current);
                    off = br.ReadUInt32();
                    classStream.Seek(off + reloff, SeekOrigin.Begin);
                    br.Read(mem, 0, size);
                    s = Encoding.UTF8.GetString(mem);
                    if (s == null) s = "";
                }
                else
                {
                    classStream.Seek(off + reloff, SeekOrigin.Begin);
                    byte[] mem = new byte[size];
                    br.Read(mem, 0, size);
                    s = Encoding.UTF8.GetString(mem);
                    if (s == null) s = "";
                }
                classStream.Seek(pos, SeekOrigin.Begin);
                return s;
            }
            else
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + steps[0]), SeekOrigin.Current);
                int i = br.ReadInt32();
                classStream.Seek(i, SeekOrigin.Begin);
                property = "";
                int x;
                for (x = 1; x < steps.Length - 1; x++)
                    property += steps[x] + ".";
                property += steps[x];

                string s = getSTR(property, this.getType(steps[0]));
                classStream.Seek(pos, SeekOrigin.Begin);
                return s;
            }
        }
        
        private fClass getC(string property, string type)
        {
            string[] steps = property.Split('.');
            long pos = classStream.Position;
            if (steps.Length == 1)
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + property), SeekOrigin.Current);
                classStream.Seek(br.ReadInt32(),SeekOrigin.Begin);
                fClass c = new fClass(classStream, this.getType(property));
                classStream.Seek(pos, SeekOrigin.Begin);
                return c;
            }
            else
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + steps[0]), SeekOrigin.Current);
                int i = br.ReadInt32();
                classStream.Seek(i, SeekOrigin.Begin);
                property = "";
                int x;
                for (x = 1; x < steps.Length - 1; x++)
                    property += steps[x] + ".";
                property += steps[x];

                string t = this.getType(steps[0]);
                string main_name = this.name;
                this.name = t;
                fClass c = null;
                try
                {
                    c = getC(property, t);
                }
                finally
                {
                    this.name = main_name;
                    classStream.Seek(pos, SeekOrigin.Begin);
                }
                return c;
            }
        }
        private double getDBL(string property, string type)
        {
            string[] steps = property.Split('.');
            long pos = classStream.Position;
            if (steps.Length == 1)
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + property), SeekOrigin.Current);
                double d = br.ReadDouble();
                classStream.Seek(pos, SeekOrigin.Begin);
                return d;
            }
            else
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + steps[0]), SeekOrigin.Current);
                int i = br.ReadInt32();
                classStream.Seek(i, SeekOrigin.Begin);
                property = "";
                int x;
                for (x = 1; x < steps.Length - 1; x++)
                    property += steps[x] + ".";
                property += steps[x];

                string t = this.getType(steps[0]);
                string main_name = this.name;
                this.name = t;
                double d = 0;
                try
                {
                    d = getDBL(property, t);
                }
                finally
                {
                    this.name = main_name;
                }
                classStream.Seek(pos, SeekOrigin.Begin);
                return d;
            }
        }
        private int get(string property, string type)
        {
            string[] steps = property.Split('.');
            long pos = classStream.Position;
            if (steps.Length == 1)
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + property), SeekOrigin.Current);
                int i = br.ReadInt32();
                classStream.Seek(pos, SeekOrigin.Begin);
                return i;
            }
            else
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + steps[0]), SeekOrigin.Current);
                int i = br.ReadInt32();
                classStream.Seek(i, SeekOrigin.Begin);
                property = "";
                int x;
                for (x = 1; x < steps.Length - 1; x++)
                    property += steps[x] + ".";
                property += steps[x];

                string t = this.getType(steps[0]);
                string main_name = this.name;
                this.name = t;
                i = 0;
                try
                {
                    i = get(property, t);
                }
                finally
                {
                    this.name = main_name;
                    classStream.Seek(pos, SeekOrigin.Begin);
                }
                return i;
            }
        }
        public fClass(MemoryStream ms, string classname)
        {
            this.classStream = ms;
            this.name = classname;
            this.classPosition = ms.Position;
        }
        public static void AddClass(fClass c)
        {
            foreach (cProp p in c.properties)
                propertyTypes.Add(c.name + "." + p.name, p.type);
            FoundClasses.Add(c);
        }
        public static void InitClasses()
        {
            foreach (fClass c in FoundClasses)
            {
                int i = 4;
                //EventDispatcher -> i+=4?
                switch (c.superclass)
                {
                    case "cGO":
                        Debug.Print("{0} has superclass {1}!", c.name, c.superclass);
                        i += 12;
                        break;
                    case "EventDispatcher":
                        Debug.Print("{0} has superclass {1}!",c.name,c.superclass);
                        i += 4;
                        break;
                    case "Object":
                        break;
                    default:
                        Debug.Print("unhandled {0} has superclass {1}!",c.name,c.superclass);
                        break;
                }

                foreach (cProp p in c.properties)
                {
                    if(p.type.Length>5)
                    if (p.type.Substring(0, 6) == "const_")
                        continue;
                    if ((p.type == "uint") || (p.type == "int") || (p.type == "Boolean"))
                    {
                        offsets.Add(c.name + "." + p.name,  i * 4);
                        i++;
                    }
                }
                foreach (cProp p in c.properties)
                {
                    if (p.type.Length > 5)
                        if (p.type.Substring(0, 6) == "const_")
                        {
 
                            if ((p.type != "uint") && (p.type != "int") && (p.type != "Boolean") && (p.type != "Number")) i++;
                            continue;
                        }
                    if ((p.type != "uint") && (p.type != "int") && (p.type != "Boolean") && (p.type != "Number"))
                    {
                        offsets.Add(c.name + "." + p.name,  i * 4);
                        i++;
                    }
                }
                foreach (cProp p in c.properties)
                {
                    if (p.type.Length > 5)
                        if (p.type.Substring(0, 6) == "const_")
                            continue;
                    if (p.type == "Number")
                    {
                        offsets.Add(c.name + "." + p.name, i * 4);
                        i++;
                        i++;
                    }
                }
            }
        }
        public static int Offset(string destination)
        {
            return (int)offsets[destination];
        }
 
    }
}