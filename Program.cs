using Memory.Utils;
using Memory.Win64;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

class Program
{
    private static MemoryHelper64 helper;
    private static ulong targetAddr;

    static void Main(string[] args)
    {

        Console.WriteLine("Finding dota2.exe");
        System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessesByName("dota2").FirstOrDefault();
        

        if (p == null)
        {
            Console.WriteLine("I dont find dota2.exe");
        } else
        {
            Console.WriteLine("Finded");
        }

        helper = new MemoryHelper64(p);

        Console.WriteLine("Init helper..");

        

        Console.WriteLine("Inited");

        int offset = 0x18;

        int count = 1;

        Console.WriteLine("starting cycle...");

        while (true)
        {

            ulong baseAddress = helper.GetBaseAddress(0x435C3E0, "client.dll");

            int health = helper.ReadMemory<int>(MemoryUtils.OffsetCalculator(helper, baseAddress, new int[] { offset, 0x31C }));
            int max_health = helper.ReadMemory<int>(MemoryUtils.OffsetCalculator(helper, baseAddress, new int[] { offset, 0x318 }));

            byte[] hero_name = helper.ReadMemoryBytes(MemoryUtils.OffsetCalculator(helper, baseAddress, new int[] { offset, 0xB70, 0 }), 50);
            string heroNameString = Encoding.UTF8.GetString(hero_name);
            heroNameString = heroNameString.Replace("Hero_", "");

            float floatMana = (float)Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(helper.ReadMemory<int>(MemoryUtils.OffsetCalculator(helper, baseAddress, new int[] { offset, 0xA9C }))), 0));
            float maxMana = (float)Math.Round(BitConverter.ToSingle(BitConverter.GetBytes(helper.ReadMemory<int>(MemoryUtils.OffsetCalculator(helper, baseAddress, new int[] { offset, 0xAA0 }))), 0));

            if (heroNameString.Contains("Hero"))
            {
                
                Console.WriteLine(count + ". " + heroNameString + " " + + health + "/" + max_health +  " - " + floatMana + "/" + maxMana);
                count++;
            }
            offset = offset + 0x18;

            if (count == 11)
            {
                count = 0;
                offset = 0x00;
                Console.WriteLine("========================");
                Thread.Sleep(3000);
            }

            


        }

    }

}

namespace Memory.Win32
{



    class MemoryHelper32
    {
        System.Diagnostics.Process process;
        public MemoryHelper32(System.Diagnostics.Process TargetProcess)
        {
            process = TargetProcess;
        }

        public uint GetBaseAddress(uint StartingAddress)
        {
            return (uint)process.MainModule.BaseAddress + StartingAddress;
        }

        public byte[] ReadMemoryBytes(uint MemoryAddress, uint Bytes)
        {
            byte[] data = new byte[Bytes];
            ReadProcessMemory(process.Handle, MemoryAddress, data, data.Length, IntPtr.Zero);
            return data;
        }

        public T ReadMemory<T>(uint MemoryAddress)
        {
            byte[] data = ReadMemoryBytes(MemoryAddress, (uint)Marshal.SizeOf(typeof(T)));

            T t;
            GCHandle PinnedStruct = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { t = (T)Marshal.PtrToStructure(PinnedStruct.AddrOfPinnedObject(), typeof(T)); }
            catch (Exception ex) { throw ex; }
            finally { PinnedStruct.Free(); }

            return t;
        }

        public bool WriteMemory<T>(uint MemoryAddress, T Value)
        {
            IntPtr bw = IntPtr.Zero;

            int sz = ObjectType.GetSize<T>();
            byte[] data = ObjectType.GetBytes<T>(Value);
            bool result = WriteProcessMemory(process.Handle, MemoryAddress, data, sz, out bw);
            return result && bw != IntPtr.Zero;
        }

        public void Close()
        {
            CloseHandle(process.Handle);
        }

        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            uint lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            uint lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
            );

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);
        #endregion
    }
}

namespace Memory.Win64
{
    class MemoryHelper64
    {
        System.Diagnostics.Process process;


        public MemoryHelper64(System.Diagnostics.Process TargetProcess)
        {
            process = TargetProcess;
        }

        public long getAdressByDll(string dll)
        {
            ProcessModule myModule = process.Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName == dll);

            IntPtr baseAddress = myModule.BaseAddress;
            return baseAddress.ToInt64();

        }

        public ulong GetBaseAddress(ulong StartingAddress, string dll)
        {
            return (ulong)getAdressByDll(dll) + StartingAddress;
        }

        public byte[] ReadMemoryBytes(ulong MemoryAddress, int Bytes)
        {
            byte[] data = new byte[Bytes];
            ReadProcessMemory(process.Handle, MemoryAddress, data, data.Length, IntPtr.Zero);
            return data;
        }

        public T ReadMemory<T>(ulong MemoryAddress)
        {
            byte[] data = ReadMemoryBytes(MemoryAddress, Marshal.SizeOf(typeof(T)));

            T t;
            GCHandle PinnedStruct = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { t = (T)Marshal.PtrToStructure(PinnedStruct.AddrOfPinnedObject(), typeof(T)); }
            catch (Exception ex) { throw ex; }
            finally { PinnedStruct.Free(); }

            return t;
        }

        public bool WriteMemory<T>(ulong MemoryAddress, T Value)
        {
            IntPtr bw = IntPtr.Zero;

            int sz = ObjectType.GetSize<T>();
            byte[] data = ObjectType.GetBytes<T>(Value);
            bool result = WriteProcessMemory(process.Handle, MemoryAddress, data, sz, out bw);
            return result && bw != IntPtr.Zero;
        }

        public void Close()
        {
            CloseHandle(process.Handle);
        }

        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
            );

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);
        #endregion
    }
}

namespace Memory.Utils
{
    static class MemoryUtils
    {
        public static uint OffsetCalculator(Win32.MemoryHelper32 TargetMemory, uint BaseAddress, int[] Offsets)
        {
            var address = BaseAddress;
            foreach (uint offset in Offsets)
            {
                address = TargetMemory.ReadMemory<uint>(address) + offset;
            }
            return address;
        }

        public static ulong OffsetCalculator(Win64.MemoryHelper64 TargetMemory, ulong BaseAddress, int[] Offsets)
        {
            var address = BaseAddress;
            foreach (uint offset in Offsets)
            {
                address = TargetMemory.ReadMemory<ulong>(address) + offset;
            }
            return address;
        }
    }

    public static class ObjectType
    {
        public static int GetSize<T>()
        {
            return Marshal.SizeOf(typeof(T));
        }

        public static byte[] GetBytes<T>(T Value)
        {
            string typename = typeof(T).ToString();
            Console.WriteLine(typename);
            switch (typename)
            {
                case "System.Single":
                    return BitConverter.GetBytes((float)Convert.ChangeType(Value, typeof(float)));
                case "System.Int32":
                    return BitConverter.GetBytes((int)Convert.ChangeType(Value, typeof(int)));
                case "System.Int64":
                    return BitConverter.GetBytes((long)Convert.ChangeType(Value, typeof(long)));
                case "System.Double":
                    return BitConverter.GetBytes((double)Convert.ChangeType(Value, typeof(double)));
                case "System.Byte":
                    return BitConverter.GetBytes((byte)Convert.ChangeType(Value, typeof(byte)));
                case "System.String":
                    return Encoding.Unicode.GetBytes((string)Convert.ChangeType(Value, typeof(string)));
                default:
                    return new byte[0];
            }
        }
    }
}