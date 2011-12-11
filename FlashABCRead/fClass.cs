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
        public fClass gO(string property,uint index,string type2)
        {
            lock (this)
            {
                lock (this.classStream)
                {
                    classStream.Seek(classPosition, SeekOrigin.Begin);
                    return getObject(property, name,index,type2);
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

            /* 07.10.11
             * uint cnt = br.ReadUInt32();
            if (cnt > 0x10000000) Debugger.Break();*/
            br.ReadUInt32();
            br.ReadUInt32();
            uint offset = br.ReadUInt32();

            /* 07.10.11 */
            classStream.Seek(offset+4, SeekOrigin.Begin);
            uint cnt = br.ReadUInt32();
            offset += 8;

            for (int x = 0; x < cnt; x++)
            {
                classStream.Seek(offset+x*4, SeekOrigin.Begin);
                classStream.Seek(br.ReadUInt32() & 0xFFFFFFF8, SeekOrigin.Begin);
                list.Add(new fClass(classStream, type));
            }

            classStream.Seek(pos, SeekOrigin.Begin);
            return list;

        }

        private fClass getObject(string property, string type, uint index,string type2)
        {
            string[] steps = property.Split('.');
            long pos = classStream.Position;
            if (steps.Length == 1)
            {
                BinaryReader br = new BinaryReader(classStream);
                classStream.Seek(Offset(type + "." + property), SeekOrigin.Current);
                uint off = br.ReadUInt32();
                classStream.Seek(off & 0xFFFFFFF8, SeekOrigin.Begin);
                classStream.Seek(0x10, SeekOrigin.Current);

                uint list = (br.ReadUInt32() & 0xFFFFFFF8);
                short cnt = br.ReadInt16();
                //07.10.2011 ... + 0xC
                classStream.Seek(list+0xC, SeekOrigin.Begin);
                uint gotopos = 0;
                for (int i = 0; i < cnt; i++)
                {
                    uint idx = br.ReadUInt32();
                    if(idx==0){cnt++;continue;}
                    uint p   = br.ReadUInt32();
                    if ((idx - 6) / 8 == index)
                    {
                        gotopos = p & 0xFFFFFFF8;
                        break;
                    }
                }
                fClass c = null;
                if (gotopos != 0)
                {
                    classStream.Seek(gotopos, SeekOrigin.Begin);
                    c = new fClass(classStream, type2);
                }
                
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
                    c = getObject(property, t,index,type2);
                }
                finally
                {
                    this.name = main_name;
                    classStream.Seek(pos, SeekOrigin.Begin);
                }
                return c;
            }
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
                if (size > 1028) throw new Exception("String too big!");
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

        //momentan leider komplett falsch :(
        public String getClassName()
        {
            uint[] steps=new uint[11];
            long pos = classStream.Position;
            try
            {
                lock (this)
                {
                    lock (this.classStream)
                    {
                        classStream.Seek(classPosition, SeekOrigin.Begin);

                        //2 1 1 2 3 0 0 3 0 0 2
                        steps[0] = 2;
                        steps[1] = 1;
                        steps[2] = 2;
                        steps[3] = 1;
                        steps[4] = 3;
                        steps[5] = 4;
                        steps[6] = 8;
                        steps[7] = 2;
                        BinaryReader br = new BinaryReader(classStream);
                        for (int i = 0; i < 7; i++)
                        {
                            classStream.Seek(steps[i] * 4, SeekOrigin.Current);
                            uint off = br.ReadUInt32();
                            if (off == 0)
                            {
                                classStream.Seek(pos, SeekOrigin.Begin);
                                return "";
                            }
                            classStream.Seek(off, SeekOrigin.Begin);
                        }
                        classStream.Seek(steps[7]*4, SeekOrigin.Current);
                        uint off2 = br.ReadUInt32();
                        uint reloff = br.ReadUInt32();
                        int size = br.ReadInt32();
                        if (((uint)size > 0x100)||(size==0))
                        {
                            classStream.Seek(pos, SeekOrigin.Begin);
                            return "";
                        }
                        string s = "";
                        byte[] mem = new byte[size];
                        classStream.Seek(off2+reloff, SeekOrigin.Begin); 
                        br.Read(mem, 0, size);
                        s = Encoding.UTF8.GetString(mem);
                        if (s == null) s = "";
                        classStream.Seek(pos, SeekOrigin.Begin);
                        return s;
                    }
                }
            }
            catch (EndOfStreamException e)
            {
                classStream.Seek(pos, SeekOrigin.Begin);
                return "";
            }
            classStream.Seek(pos, SeekOrigin.Begin);
            return "";
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
        public static string GetClassName(int index)
        {
            return FoundClasses[index].name;
        }
        public static void AddStaticPropertiesToClass(int index,List<cProp> pr)
        {
            foreach (cProp p in pr)
            {
                propertyTypes.Add("static "+FoundClasses[index].name + "." + p.name, p.type);
                FoundClasses[index].properties.Add(p);
            }
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
                    /*case "cGO":
                        Debug.Print("{0} has superclass {1}!", c.name, c.superclass);
                        i += 12;
                        break;*/
                    case "EventDispatcher":
                        Debug.Print("{0} has superclass {1}!",c.name,c.superclass);
                        i += 4;
                        break;
                    case "Object":
                        break;
                    case null:
                        break;
                    default:
                        i += 4;
                        bool found = false;
                        foreach (fClass supc in FoundClasses)
                        {
                            if (supc.name == c.superclass)
                            {
                                int off = 0;
                                foreach (cProp p in supc.properties)
                                    i++;

                                Debug.Print("Superclass for {0} found.",c.name);
                                found = true;
                                break;
                            }
                        }
                        if(!found)Debug.Print("unhandled {0} has superclass {1}!",c.name,c.superclass);
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
 
//                            if ((p.type != "uint") && (p.type != "int") && (p.type != "Boolean") && (p.type != "Number")) i++;
                            offsets.Add(c.name + "." + p.name, i * 4);
                            i++;
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