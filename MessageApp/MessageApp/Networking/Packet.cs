using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.Client
{

    public class Packet : IDisposable
    {

        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        public Packet() {
            buffer = new List<byte>();
            readPos = 0;
        }

        public Packet(byte[] bytes) {
            buffer = new List<byte>();
            readPos = 0;
            
            SetBytes(bytes);
        }

        public Packet(int id) {
            buffer = new List<byte>();
            readPos = 0;
            
            Write(id);
        }

        public void SetBytes(byte[] data) {
            Write(data);
            readableBuffer = buffer.ToArray();
        }

        public void WriteLength() {
            InsertInt(Length());
        }

        public void InsertInt(int value) {
            buffer.InsertRange(0, BitConverter.GetBytes(value));
        }

        public byte[] ToArray() {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length() {
            return buffer.Count;
        }

        public int UnreadLength() {
            return Length() - readPos;
        }

        public void Reset(bool shouldReset) {
            if (shouldReset) {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else {
                readPos -= 4;
            }
        }

        public void Write(byte value) {
            buffer.Add(value);
        }

        public void Write(byte[] value) {
            buffer.AddRange(value);
        }

        public void Write(short value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(long value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(int value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(bool value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(string value) {
            Write(value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(value));
        }

        public byte ReadByte() {
            if (buffer.Count > readPos) {
                byte value = readableBuffer[readPos];
                readPos += 1;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type byte", ConsoleColor.Red);
                return 0;
            }
        }

        public byte[] ReadBytes(int length) {
            if (buffer.Count > readPos) {
                byte[] value = buffer.GetRange(readPos, length).ToArray();
                readPos += length;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type byte[]", ConsoleColor.Red);
                return null;
            }
        }

        public short ReadShort() {
            if (buffer.Count > readPos) {
                short value = BitConverter.ToInt16(readableBuffer, readPos);
                readPos += 2;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type short", ConsoleColor.Red);
                return -1;
            }
        }
        
        public long ReadLong() {
            if (buffer.Count > readPos) {
                long value = BitConverter.ToInt64(readableBuffer, readPos);
                readPos += 8;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type long", ConsoleColor.Red);
                return -1;
            }
        }
        
        public int ReadInt() {
            if (buffer.Count > readPos) {
                int value = BitConverter.ToInt32(readableBuffer, readPos);
                readPos += 4;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type int", ConsoleColor.Red);
                return -1;
            }
        }

        public float ReadFloat() {
            if (buffer.Count > readPos) {
                float value = BitConverter.ToSingle(readableBuffer, readPos);
                readPos += 4;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type float", ConsoleColor.Red);
                return -1;
            }
        }
        
        public bool ReadBool() {
            if (buffer.Count > readPos) {
                bool value = BitConverter.ToBoolean(readableBuffer, readPos);
                readPos += 1;

                return value;
            }
            else {
                Console.WriteLine("Failed to read value of type bool", ConsoleColor.Red);
                return false;
            }
        }

        public string ReadString() {
            try {
                int length = ReadInt(); // Get the length from packet
                string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);

                if (value.Length > 0) {
                    readPos += length;
                }

                return value;
            } catch {
                Console.WriteLine("Failed to read value of type string", ConsoleColor.Red);
                return null;
            }
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}