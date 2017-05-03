using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DIS
{
    public abstract class DiskImage
    {
        protected string _filename;
        protected DiskContents Contents;
        protected int MaxTrackSize = 0;        

        public DiskImage(String filename)
        {
            _filename = filename;
            Contents = null;
        }

        public DiskImage(String filename, DiskContents contents)
        {
            _filename = filename;
            Contents = contents;
            if (contents != null)
            { 
                Contents.logical = new LogicalContents();
            }
            
        }

        public abstract PhysicalContents Read();
        public abstract PhysicalContents PerformRead();
        public abstract List<LogicalEntity> GetContainedItems();
        public abstract List<LogicalEntity> GetFilesnames();
        public abstract void Write();
        public abstract byte[] readSector(int sectorNumber);
        //public abstract void PrintAnalysis();
        protected abstract void WriteExractedTrack(string filename, int track, int head);

        
        
        protected void WriteBytes(BinaryWriter writer, byte value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(value);
            }
        }       

        protected void WriteBytes(BinaryWriter writer, byte[] values, int count)
        {
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                writer.Write(values[index]);
                index = (index + 1) % values.Length;
            }
        }

        protected void WriteBytes(BinaryWriter writer, string text, int count)
        {
            WriteBytes(writer, new ASCIIEncoding().GetBytes(text), count);
        }
        
        protected int FindBytes(byte[] haystack, int offset, byte[] needle, int maxOffset)
        {
            bool found = false;            
            if (maxOffset == 0)
            {
                maxOffset = haystack.Length-1;
            }
            int maxFoundOffset = maxOffset - needle.Length + 1;           
            int pos = 0;
            while (!found && ((offset + pos) <= maxFoundOffset))
            {
                if (CompareBytes(haystack, offset + pos, needle, 0, needle.Length))
                {
                    found = true;
                }
                else
                {
                    pos++;
                }
            }
            if ((offset + pos) > maxFoundOffset)
            {
                pos = -1;
            }
            return pos;
        }

        protected bool CompareBytes(byte[] source, int srcOffset, byte[] target, int targetOffset, int length) 
        {
            int index = 0;
            while ((index < length) && (source[srcOffset + index] == target[targetOffset + index]))
            {
                index++;
            }
            return (index == length);
        }

        protected long GetMultiByteVal(byte[] source, int offset,  int numBytes, bool bigEndian)
        {
            int retVal = 0;
            int factor = (bigEndian ? -8 : 8);
            int start = (bigEndian ? 8*(numBytes -1) : 0);
            
            for (int i = 0; i < numBytes; i++)
            {
                retVal += (source[i + offset] << (start + (factor * i)));                
            }
            return retVal;
        }

        protected void SetMultiByteVal(byte[] source, int offset, int numBytes, long value, bool bigEndian)
        {
            int factor = (bigEndian ? -8 : 8);
            int start = (bigEndian ? 8 * (numBytes - 1) : 0);            
            for (int i = 0; i < numBytes; i++)
            {
                int shift = start + (factor *i);                
                source[i + offset] = (byte)(value >> shift);
            }
        }

        protected byte FindMostOccuringByte(byte[] data, int offset, int numBytes)
        {            
            byte[] freq = new byte[256];
            byte highest = 0;
            int maxfreq = -1;
            for (int i = offset; i < (offset + numBytes); i++)
            {
                freq[data[i]]++;
                if (freq[data[i]] > maxfreq)
                {
                    highest = data[i];
                    maxfreq = freq[data[i]];
                }
            }
            return highest;
        }

        protected int SectorSizeFromCode(int code)
        {
            return (64 << ((code & 3) + 1));
        }

        protected ushort CalcCRC16(byte[] data, long offset, int length)
        {
            ushort crc = 0xffff;
            CalcCRC16(data, offset, length, ref crc);
            return crc;
        }

        protected void CalcCRC16(byte[] data, long offset, int length, ref ushort crc)
        {
            ushort wData;
            for (int i = 0; i < length; i++)
            {
                wData = Convert.ToUInt16(data[i + offset] << 8);
                for (int j = 0; j < 8; j++, wData <<= 1)
                {
                    var a = (crc ^ wData) & 0x8000;
                    if (a != 0)
                    {
                        var c = (crc << 1) ^ 0x1021;
                        crc = Convert.ToUInt16(c & 0xffff);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
        }

        public List<LogicalEntity> GetItems()
        {
            if (Contents == null)
            {
                Read();
            }
            return GetContainedItems();
        }
    }
}
