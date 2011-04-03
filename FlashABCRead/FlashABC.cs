using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace FlashABCRead
{
    public class FlashABC
    {
        enum TraitKind : int
        {
	        Slot = 0,
	        Method = 1,
	        Getter = 2,
	        Setter = 3,
	        Class = 4,
	        Function = 5,
	        Const = 6,
        }
        const int OP_bkpt = 0x01;
        const int OP_nop = 0x02;
        const int OP_throw = 0x03;
        const int OP_getsuper = 0x04;
        const int OP_setsuper = 0x05;
        const int OP_dxns = 0x06;
        const int OP_dxnslate = 0x07;
        const int OP_kill = 0x08;
        const int OP_label = 0x09;
        const int OP_ifnlt = 0x0C;
        const int OP_ifnle = 0x0D;
        const int OP_ifngt = 0x0E;
        const int OP_ifnge = 0x0F;
        const int OP_jump = 0x10;
        const int OP_iftrue = 0x11;
        const int OP_iffalse = 0x12;
        const int OP_ifeq = 0x13;
        const int OP_ifne = 0x14;
        const int OP_iflt = 0x15;
        const int OP_ifle = 0x16;
        const int OP_ifgt = 0x17;
        const int OP_ifge = 0x18;
        const int OP_ifstricteq = 0x19;
        const int OP_ifstrictne = 0x1A;
        const int OP_lookupswitch = 0x1B;
        const int OP_pushwith = 0x1C;
        const int OP_popscope = 0x1D;
        const int OP_nextname = 0x1E;
        const int OP_hasnext = 0x1F;
        const int OP_pushnull = 0x20;
        const int OP_pushundefined = 0x21;
        const int OP_pushconstant = 0x22;
        const int OP_nextvalue = 0x23;
        const int OP_pushbyte = 0x24;
        const int OP_pushshort = 0x25;
        const int OP_pushtrue = 0x26;
        const int OP_pushfalse = 0x27;
        const int OP_pushnan = 0x28;
        const int OP_pop = 0x29;
        const int OP_dup = 0x2A;
        const int OP_swap = 0x2B;
        const int OP_pushstring = 0x2C;
        const int OP_pushint = 0x2D;
        const int OP_pushuint = 0x2E;
        const int OP_pushdouble = 0x2F;
        const int OP_pushscope = 0x30;
        const int OP_pushnamespace = 0x31;
        const int OP_hasnext2 = 0x32;
        const int OP_newfunction = 0x40;
        const int OP_call = 0x41;
        const int OP_construct = 0x42;
        const int OP_callmethod = 0x43;
        const int OP_callstatic = 0x44;
        const int OP_callsuper = 0x45;
        const int OP_callproperty = 0x46;
        const int OP_returnvoid = 0x47;
        const int OP_returnvalue = 0x48;
        const int OP_constructsuper = 0x49;
        const int OP_constructprop = 0x4A;
        const int OP_callsuperid = 0x4B;
        const int OP_callproplex = 0x4C;
        const int OP_callinterface = 0x4D;
        const int OP_callsupervoid = 0x4E;
        const int OP_callpropvoid = 0x4F;
        const int OP_newobject = 0x55;
        const int OP_newarray = 0x56;
        const int OP_newactivation = 0x57;
        const int OP_newclass = 0x58;
        const int OP_getdescendants = 0x59;
        const int OP_newcatch = 0x5A;
        const int OP_findpropstrict = 0x5D;
        const int OP_findproperty = 0x5E;
        const int OP_finddef = 0x5F;
        const int OP_getlex = 0x60;
        const int OP_setproperty = 0x61;
        const int OP_getlocal = 0x62;
        const int OP_setlocal = 0x63;
        const int OP_getglobalscope = 0x64;
        const int OP_getscopeobject = 0x65;
        const int OP_getproperty = 0x66;
        const int OP_getpropertylate = 0x67;
        const int OP_initproperty = 0x68;
        const int OP_setpropertylate = 0x69;
        const int OP_deleteproperty = 0x6A;
        const int OP_deletepropertylate = 0x6B;
        const int OP_getslot = 0x6C;
        const int OP_setslot = 0x6D;
        const int OP_getglobalslot = 0x6E;
        const int OP_setglobalslot = 0x6F;
        const int OP_convert_s = 0x70;
        const int OP_esc_xelem = 0x71;
        const int OP_esc_xattr = 0x72;
        const int OP_convert_i = 0x73;
        const int OP_convert_u = 0x74;
        const int OP_convert_d = 0x75;
        const int OP_convert_b = 0x76;
        const int OP_convert_o = 0x77;
        const int OP_coerce = 0x80;
        const int OP_coerce_b = 0x81;
        const int OP_coerce_a = 0x82;
        const int OP_coerce_i = 0x83;
        const int OP_coerce_d = 0x84;
        const int OP_coerce_s = 0x85;
        const int OP_astype = 0x86;
        const int OP_astypelate = 0x87;
        const int OP_coerce_u = 0x88;
        const int OP_coerce_o = 0x89;
        const int OP_negate = 0x90;
        const int OP_increment = 0x91;
        const int OP_inclocal = 0x92;
        const int OP_decrement = 0x93;
        const int OP_declocal = 0x94;
        const int OP_typeof = 0x95;
        const int OP_not = 0x96;
        const int OP_bitnot = 0x97;
        const int OP_concat = 0x9A;
        const int OP_add_d = 0x9B;
        const int OP_add = 0xA0;
        const int OP_subtract = 0xA1;
        const int OP_multiply = 0xA2;
        const int OP_divide = 0xA3;
        const int OP_modulo = 0xA4;
        const int OP_lshift = 0xA5;
        const int OP_rshift = 0xA6;
        const int OP_urshift = 0xA7;
        const int OP_bitand = 0xA8;
        const int OP_bitor = 0xA9;
        const int OP_bitxor = 0xAA;
        const int OP_equals = 0xAB;
        const int OP_strictequals = 0xAC;
        const int OP_lessthan = 0xAD;
        const int OP_lessequals = 0xAE;
        const int OP_greaterthan = 0xAF;
        const int OP_greaterequals = 0xB0;
        const int OP_instanceof = 0xB1;
        const int OP_istype = 0xB2;
        const int OP_istypelate = 0xB3;
        const int OP_in = 0xB4;
        const int OP_increment_i = 0xC0;
        const int OP_decrement_i = 0xC1;
        const int OP_inclocal_i = 0xC2;
        const int OP_declocal_i = 0xC3;
        const int OP_negate_i = 0xC4;
        const int OP_add_i = 0xC5;
        const int OP_subtract_i = 0xC6;
        const int OP_multiply_i = 0xC7;
        const int OP_getlocal0 = 0xD0;
        const int OP_getlocal1 = 0xD1;
        const int OP_getlocal2 = 0xD2;
        const int OP_getlocal3 = 0xD3;
        const int OP_setlocal0 = 0xD4;
        const int OP_setlocal1 = 0xD5;
        const int OP_setlocal2 = 0xD6;
        const int OP_setlocal3 = 0xD7;
        const int OP_debug = 0xEF;
        const int OP_debugline = 0xF0;
        const int OP_debugfile = 0xF1;
        const int OP_bkptline = 0xF2;

        
        private long start;
        private uint len;
        private Stream stream;
        private ushort flags;
        private ushort minor_version;
        private ushort major_version;

        private long intStart;
        private long uintStart;
        private long doubleStart;
        private long stringStart;
        private long namespaceStart;
        private long namespacesetStart;
        private long multinameStart;
        private long methodinfoStart;
        private long metadatainfoStart;
        private long instanceinfoStart;
        private long classinfoStart;
        private long scriptinfoStart;
        private long methodbodyStart;
        public FlashABC(Stream s,uint len)
        {
            this.stream=s;
            this.len=len;
        }
        public void FindClasses(List<String> classes)
        {
            Stream s=this.stream;
            start=s.Position;
            BinaryReader br = new BinaryReader(s);
            flags = br.ReadUInt16();
            ushort unkn = br.ReadUInt16();
            int c;
            string name="";
            
            while((char)(c=br.ReadChar())!='\0')
                name += (char)c;
            Debug.Print("{0}", name);
            minor_version = br.ReadUInt16();
            minor_version = br.ReadUInt16();

            ulong cnt;
            Debug.Print("IS {0:x} {1:x}\n", br.BaseStream.Position, len+start);
            intStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++) skipS32(br.BaseStream);

            Debug.Print("UIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            uintStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++) skipU32(br.BaseStream);

            Debug.Print("DS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            doubleStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++) br.ReadDouble();

            Debug.Print("SS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            stringStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++)
            {
                //Debug.Print("{0}",readString(br.BaseStream));
                skipString(br.BaseStream);
            }

            Debug.Print("NSS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            namespaceStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++)
            {
                switch (br.ReadByte())
                {
                    case 8:
                    case 5:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                        skipU32(br.BaseStream);
                        break;
                    default:
                        Debugger.Break();
                        break;
                }
            }

            Debug.Print("NSSS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            namespacesetStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++)
            {
                ulong cnt2 = getU32(br.BaseStream);
                for (ulong m = 0; m < cnt2; m++)
                    skipU32(br.BaseStream);
            }

            Debug.Print("MNS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            multinameStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 1; n < cnt; n++)
            {
                short kind = br.ReadByte();
                switch (kind)
                {
                    case 7:
                    case 13:
                    case 9:
                    case 14:
                        skipU32(br.BaseStream);//namespace
                        skipU32(br.BaseStream);//name
                        break;
                    case 15:
                    case 16:
                    case 27:
                        skipU32(br.BaseStream);
                        break;
                    case 17:
                    case 18:
                        break;


                    case 29://experimental,GENERIC
                        skipU32(br.BaseStream);
                        ulong param_count = getU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        if (param_count != 1) Debugger.Break();
                        break;
                    case 28://experimental,Multiname_LA
                        skipU32(br.BaseStream);
                        break;

                    default:
                        Debug.Print("MNS Error {0:x} {1:x} kind:{2}\n", br.BaseStream.Position, len + start,kind);
                        return;
                        //break;
                }
            }

            Debug.Print("MIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            methodinfoStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 0; n < cnt; n++)
            {
                ulong param_count = getU32(br.BaseStream);
                skipU32(br.BaseStream);
                for (ulong m = 0; m < param_count; m++)
                    skipU32(br.BaseStream);
                ulong name_index = getU32(br.BaseStream);
                uint mis_flags=br.ReadByte();
                if ((mis_flags & 8) != 0)
                {
                    ulong optional_count = getU32(br.BaseStream);
                    for (ulong m = param_count - optional_count; m < param_count; ++m)
                    {
                        skipU32(br.BaseStream);
                        br.ReadByte();
                    }
                }
                if ((mis_flags & 128) != 0)
                    for(uint m = 0; m< param_count; ++m)
                        skipU32(br.BaseStream);
            }

            Debug.Print("MDIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            metadatainfoStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 0; n < cnt; n++)
            {
                skipU32(br.BaseStream);
                ulong cnt2 = getU32(br.BaseStream);
                for (ulong m = 0; m < cnt2; m++)
                {
                    skipU32(br.BaseStream);
                    skipU32(br.BaseStream);
                }
            }

            Debug.Print("IIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            instanceinfoStart=br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 0; n < cnt; n++)
            {
                string instanceName = getLabel(getU32(br.BaseStream));
                string superclass = getLabel(getU32(br.BaseStream));

                if((br.ReadByte() & 8)!=0)
                    skipU32(br.BaseStream);
                ulong cnt2 = getU30(br.BaseStream);
                for (ulong m = 0; m < cnt2; m++)
                    skipU32(br.BaseStream);

                skipU32(br.BaseStream);

                if(!classes.Contains(instanceName))
                    skipTraits(br);
                else
                {
                    fClass instance=new fClass();
                    instance.name=instanceName;
                    instance.superclass = superclass;
                    Debug.Print("Instance:{0} found!", instanceName);
                    List<cProp> prop = new List<cProp>();
                    getTraits(br,out prop);
                    instance.properties = prop;
                    fClass.AddClass(instance);
                    if (fClass.Count == classes.Count) return;
                }
            }

            /*
            Debug.Print("CIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            classinfoStart = br.BaseStream.Position;
            for (ulong n = 0; n < cnt; n++)
            {
                skipU32(br.BaseStream);
                skipTraits(br);
            }

            Debug.Print("SIS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            scriptinfoStart = br.BaseStream.Position;
            cnt = getU30(br.BaseStream);
            for (ulong n = 0; n < cnt; n++)
            {
                skipU32(br.BaseStream);
                skipTraits(br);
            }

            Debug.Print("MBS {0:x} {1:x}\n", br.BaseStream.Position, len + start);
            methodbodyStart = br.BaseStream.Position;

            cnt = getU30(br.BaseStream);
            for (ulong n = 0; n < cnt; n++)
            {
                ulong minfo = getU32(br.BaseStream);
                ulong max_stack = getU32(br.BaseStream);
                ulong max_regs = getU32(br.BaseStream);
                ulong scope_depth = getU32(br.BaseStream);
                ulong max_scope = getU32(br.BaseStream);
                ulong code_length = getU32(br.BaseStream);

                byte[] buf = new byte[code_length];
                br.Read(buf, 0, (int)code_length);
                parseCode(buf);

                ulong ex_count = getU32(br.BaseStream);
                for (ulong m = 0; m < ex_count; m++)
                {
                    skipU32(br.BaseStream);
                    skipU32(br.BaseStream);
                    skipU32(br.BaseStream);
                    skipU32(br.BaseStream);
                    skipU32(br.BaseStream);
                }

                skipTraits(br);
            }*/
            Debug.Print("End parse {0:x} {1:x}\n", br.BaseStream.Position, len + start);
        }

        public void skipTraits(BinaryReader br)
        {
            List<cProp> al = new List<cProp>();
            getTraits(br, false, out al);
        }
        public void getTraits(BinaryReader br,  out List<cProp> properties)
        {
            getTraits(br, true, out properties);
        }
        public void getTraits(BinaryReader br,bool addproperties,out List<cProp> properties)
        {
            properties = new List<cProp>();
            ulong type = 0;
            ulong cnt2 = getU30(br.BaseStream);
            for (ulong m = 0; m < cnt2; m++)
            {
                ulong name=getU32(br.BaseStream);
                byte tag=br.ReadByte();
                TraitKind kind = (TraitKind)(tag & 0xF);
                switch (kind)
                {
                    case TraitKind.Slot:
                        skipU32(br.BaseStream);
                        type = getU32(br.BaseStream);
                        ulong v1 = getU32(br.BaseStream);
                        byte v2 = 0;
                        if (v1 > 0)
                            v2=br.ReadByte();
                        if (addproperties)
                        {
                            cProp p = new cProp();
                            p.name = getLabel(name);
                            p.type = getLabel(type);
                            p.v1 = v1;
                            p.v2 = v2;
                            ulong nsidx = getNameSpaceByMultinameIdx(name);
                            p.nskind = 0;
                            if(nsidx>0)
                                p.nskind = getNameSpaceKind(nsidx);
                            Debug.Print("var {0} {1} {2} {3} {4}", p.name, p.type, v1, v2,p.nskind);
                            properties.Add(p);
                        }
                        break;
                    case TraitKind.Const:
                        type = getU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        v1 = getU32(br.BaseStream);
                        v2 = 0;
                        if (v1 > 0)
                            v2 = br.ReadByte();
                        if (addproperties)
                        {
                            cProp p = new cProp();
                            p.name = getLabel(name);
                            p.type = "const_"+getLabel(type);
                            p.v1 = v1;
                            p.v2 = v2;
                            properties.Add(p);
                            ulong nsidx = getNameSpaceByMultinameIdx(name);
                            p.nskind = 0;
                            if (nsidx > 0)
                                p.nskind = getNameSpaceKind(nsidx);
                            Debug.Print("const {0} {1} {2} {3} {4}", p.name, p.type, v1, v2, p.nskind);
                        }
                        break;
                    case TraitKind.Class:
                    case TraitKind.Function:
                    case TraitKind.Method:
                    case TraitKind.Getter:
                    case TraitKind.Setter:
                        skipU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        break;

                    /*experimental kinds
                    case 11:
                    case 8:
                    case 14:
                    case 9:
                    case 12:
                    case 15:
                    case 7:
                    case 13:
                    case 10:
                        //Debugger.Break();

                        skipU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        //return;
                        break;*/
                    default:
                        //skipU32(br.BaseStream);
                        //skipU32(br.BaseStream);
                        Debugger.Break();
                        break;
                }
                if ((tag & (0x04 << 4))!=0)
                {
                    ulong cnt3 = getU30(br.BaseStream);
                    for (ulong m2 = 0; m2 < cnt3; m2++)
                        skipU32(br.BaseStream);
                }
                //Debug.Print("{0:x} {1:x}", br.BaseStream.Position,br.BaseStream.Length);
            }
        }
        string readString(Stream st)
        {
            int l = (int)getU32(st);
            char[] s = new char[l];
            BinaryReader br = new BinaryReader(st);
            byte[] b=br.ReadBytes(l);
            return Encoding.Default.GetString(b);
        }
        void skipString(Stream s)
        {
            int l=(int)getU32(s);
            s.Seek(l, SeekOrigin.Current);
        }
        public byte getNameSpaceKind(ulong idx)
        {
            long pos = this.stream.Position;
            BinaryReader br = new BinaryReader(this.stream);
            br.BaseStream.Seek(namespaceStart, SeekOrigin.Begin);
            getU32(br.BaseStream);
            for (ulong n = 1; n < idx; n++)
            {
                switch (br.ReadByte())
                {
                    case 8:
                    case 5:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                        skipU32(br.BaseStream);
                        break;
                    default:
                        Debugger.Break();
                        break;
                }
            }
            byte nsk = br.ReadByte();
            switch (nsk)
            {
                case 8:
                case 5:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    break;
                default:
                    Debugger.Break();
                    break;
            }
            this.stream.Seek(pos, SeekOrigin.Begin);
            return nsk;
        }
        public ulong getNameSpaceByMultinameIdx(ulong idx)
        {
            ulong ns = 0;
            long pos = this.stream.Position;
            BinaryReader br = new BinaryReader(this.stream);
            br.BaseStream.Seek(multinameStart, SeekOrigin.Begin);

            getU32(br.BaseStream);

            for (ulong n = 1; n < idx; n++)
            {
                short kind = br.ReadByte();
                switch (kind)
                {
                    case 7:
                    case 13:
                    case 9:
                    case 14:
                        skipU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        break;
                    case 15:
                    case 16:
                    case 27:
                        skipU32(br.BaseStream);
                        break;
                    case 17:
                    case 18:
                        break;

                    case 29://experimental,GENERIC
                        skipU32(br.BaseStream);
                        ulong param_count = getU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        if (param_count != 1) Debugger.Break();
                        break;
                    case 28://experimental,Multiname_LA
                        skipU32(br.BaseStream);
                        break;

                    default:
                        idx = 0;
                        Debugger.Break();
                        break;
                }
            }
            switch (br.ReadByte())
            {
                case 7:
                case 13:
                case 9:
                case 14:
                    ns = getU32(br.BaseStream);
                    idx = getU32(br.BaseStream);
                    break;
                case 15:
                case 16:
                case 27:
                    idx = getU32(br.BaseStream);
                    break;
                case 17:
                case 18:
                    break;

                case 29://experimental,GENERIC
                    idx = getMultiName(getU32(br.BaseStream));
                    ulong param_count = getU32(br.BaseStream);
                    skipU32(br.BaseStream);
                    if (param_count != 1) Debugger.Break();
                    break;
                case 28://experimental,Multiname_LA
                    idx = getU32(br.BaseStream);
                    break;
                default:
                    idx = 0;
                    //Debugger.Break();
                    break;
            }
            this.stream.Seek(pos, SeekOrigin.Begin);
            return ns;
        }
        public ulong getMultiName(ulong idx)
        {
            long pos = this.stream.Position;
            BinaryReader br = new BinaryReader(this.stream);
            br.BaseStream.Seek(multinameStart, SeekOrigin.Begin);

            getU32(br.BaseStream);

            for (ulong n = 1; n < idx; n++)
            {
                short kind = br.ReadByte();
                switch (kind)
                {
                    case 7:
                    case 13:
                    case 9:
                    case 14:
                        skipU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        break;
                    case 15:
                    case 16:
                    case 27:
                        skipU32(br.BaseStream);
                        break;
                    case 17:
                    case 18:
                        break;

                    case 29://experimental,GENERIC
                        skipU32(br.BaseStream);
                        ulong param_count = getU32(br.BaseStream);
                        skipU32(br.BaseStream);
                        if (param_count != 1) Debugger.Break();
                        break;
                    case 28://experimental,Multiname_LA
                        skipU32(br.BaseStream);
                        break;

                    default:
                        idx = 0;
                        Debugger.Break();
                        break;
                }
            }
            switch (br.ReadByte())
            {
                case 7:
                case 13:
                case 9:
                case 14:
                    ulong ns = getU32(br.BaseStream);
                    idx = getU32(br.BaseStream);
                    break;
                case 15:
                case 16:
                case 27:
                    idx = getU32(br.BaseStream);
                    break;
                case 17:
                case 18:
                    break;

                case 29://experimental,GENERIC
                    idx = getMultiName(getU32(br.BaseStream));
                    ulong param_count = getU32(br.BaseStream);
                    skipU32(br.BaseStream);
                    if (param_count != 1) Debugger.Break();
                    break;
                case 28://experimental,Multiname_LA
                    idx = getU32(br.BaseStream);
                    break;
                default:
                    idx = 0;
                    //Debugger.Break();
                    break;
            }
            this.stream.Seek(pos, SeekOrigin.Begin);
            return idx;
        }
        public string getLabel(ulong idx)
        {
            long pos = this.stream.Position;
            StreamReader s=new StreamReader(this.stream);
            BinaryReader br = new BinaryReader(this.stream);

            idx = getMultiName(idx);

            br.BaseStream.Seek(stringStart, SeekOrigin.Begin);

            getU32(br.BaseStream);
            for (ulong n = 1; n < idx; n++) skipString(br.BaseStream);
            string st = "";
            if(idx!=0)st = readString(br.BaseStream);


            this.stream.Seek(pos, SeekOrigin.Begin);

            return st;
        }
        void skipS24(Stream s)
        {
            s.ReadByte();
			s.ReadByte();
			s.ReadByte();
        }
        void skipS32(Stream s)
        {
            skipU32(s);
        }
        void skipU32(Stream s)
        {
            while ((s.ReadByte() & 0x80)==0x80) if(s.Position>=s.Length)return;
        }
        long getS32(Stream s)
        {
    		ulong l = getU32(s);
		    if ((l & 0xFFFFFFFF00000000)!=0) // preserve unused bits
			    return (long)l;
		    else
			    return (int)l;
        }
        ulong getU30(Stream s)
        {
            return getU32(s) & 0x3FFFFFFF;
        }
        ulong getU32(Stream s)
        {
            if (s.Position >= s.Length) return 0;
            BinaryReader br = new BinaryReader(s);
            uint result = br.ReadByte();
            if (!((result & 0x00000080) != 0))
                return result;
            result = (uint)((result & 0x0000007f) | (s.ReadByte()<<7));

            if (!((result & 0x00004000)!=0))
                return result;

            result = (uint)((result & 0x00003fff) | (s.ReadByte()<<14));

            if (!((result & 0x00200000)!=0))
                return result;
        
            result = (uint)((result & 0x001fffff) | (s.ReadByte()<<21));
            if (!((result & 0x10000000)!=0))
                return result;
            return (uint)((result & 0x0fffffff) | (s.ReadByte()<<28));
        }
        /*public void parseCode(byte[] buf)
        {
            ulong idx;
            MemoryStream ms=new MemoryStream(buf);
            BinaryReader br=new BinaryReader(ms);
            while(ms.Position<ms.Length)
            {
                int opcode = ms.ReadByte();

                switch(opcode)
					{

                        case OP_pushconstant:
                            idx = getU32(br.BaseStream);
                            //Debug.Print("{0}", getLabel(idx));
                            break;
						case OP_debugfile:
						case OP_pushstring:
							//skipU32(br.BaseStream);
                            idx = getU32(br.BaseStream);
                            //Debug.Print("{0}", getLabel(idx));
							break;
						case OP_pushnamespace:
							skipU32(br.BaseStream);
							break;
						case OP_pushint:
							skipU32(br.BaseStream);
							break;
						case OP_pushuint:
							skipU32(br.BaseStream);
							break;
						case OP_pushdouble:
							skipU32(br.BaseStream);
							break;
                        case OP_initproperty:
                            idx=getU32(br.BaseStream);
                            //Debug.Print("{0}", getLabel(idx));
                            break;
                        case OP_getsuper: 
						case OP_setsuper: 
						case OP_getproperty: 
						case OP_setproperty: 
						case OP_findpropstrict: 
						case OP_findproperty:
						case OP_finddef:
						case OP_deleteproperty: 
						case OP_istype: 
						case OP_coerce: 
						case OP_astype: 
						case OP_getdescendants:
                        case OP_getlex:
                            idx = getU32(br.BaseStream);
                            //Debug.Print("{0} {1}", getLabel(idx),idx);
                            break;

                        case OP_constructprop:
						case OP_callproperty:
						case OP_callproplex:
						case OP_callsuper:
						case OP_callsupervoid:
						case OP_callpropvoid:
							skipU32(br.BaseStream);
							skipU32(br.BaseStream);
							break;
						case OP_newfunction: {
							skipU32(br.BaseStream);
							break;
						}
						case OP_callstatic:
							skipU32(br.BaseStream);
							skipU32(br.BaseStream);
							break;
						case OP_newclass: 
							skipU32(br.BaseStream);
							break;
						case OP_lookupswitch:
							skipS24(br.BaseStream);
                            ulong maxindex = getU32(br.BaseStream);
                            for (ulong i = 0; i <= maxindex; i++) 
								skipS24(br.BaseStream);
							break;
						case OP_jump:
						case OP_iftrue:
                        case OP_iffalse:
						case OP_ifeq:
                        case OP_ifne:
						case OP_ifge:
                        case OP_ifnge:
						case OP_ifgt:
                        case OP_ifngt:
						case OP_ifle:
                        case OP_ifnle:
						case OP_iflt:
                        case OP_ifnlt:
						case OP_ifstricteq:
                        case OP_ifstrictne:
							skipS24(br.BaseStream);
							break;
						case OP_inclocal:
						case OP_declocal:
						case OP_inclocal_i:
						case OP_declocal_i:
						case OP_getlocal:
						case OP_kill:
						case OP_setlocal:
						case OP_debugline:
						case OP_getglobalslot:
						case OP_getslot:
						case OP_setglobalslot:
						case OP_setslot:
						case OP_pushshort:
						case OP_newcatch:
							skipU32(br.BaseStream);
							break;
						case OP_debug:
                            br.ReadByte();
							skipU32(br.BaseStream);
                            br.ReadByte();
                            skipU32(br.BaseStream);
							break;
						case OP_newobject:
							skipU32(br.BaseStream);
							break;
						case OP_newarray:
							skipU32(br.BaseStream);
							break;
						case OP_call:
						case OP_construct:
						case OP_constructsuper:
							skipU32(br.BaseStream);
							break;
						case OP_pushbyte:
						case OP_getscopeobject:
                            br.ReadByte();
							break;
						case OP_hasnext2:
							skipU32(br.BaseStream);
                            skipU32(br.BaseStream);
                            break;
						default:
							break;
					}
            }
        }*/
    }
   
    public class cProp
    {
        public string name;
        public string type;
        public ulong v1;
        public byte v2;
        public byte nskind;
    }
}
