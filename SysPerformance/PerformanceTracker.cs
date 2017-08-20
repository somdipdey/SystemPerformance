using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;

namespace SystemPerformance
{

    /*This class tracks the performance of the computer on which the assemblies (or this assembly) is/are executing.
     PerformanceTracker uses different components of PerformanceCounter to track performance of different components of the computer*/
    public class PerformanceTracker
    {
        PerformanceCounter cpuCounter;
        PerformanceCounter cpuPrivilegedTime;
        PerformanceCounter cpuInterruptTime;
        PerformanceCounter cpuDPCTime;
        PerformanceCounter ramCounter;
        PerformanceCounter memCommittedBytes;
        PerformanceCounter memCommitLimit;
        PerformanceCounter memCommittedBytesInUse;
        PerformanceCounter memPoolPagedBytes;
        PerformanceCounter memPoolNonPagedBytes;
        PerformanceCounter memCacheBytes;
        PerformanceCounter pagingFile;
        PerformanceCounter diskQueueLength;
        PerformanceCounter diskReadBytePSec;
        PerformanceCounter diskWriteBytePSec;
        PerformanceCounter diskAvgSecRead;
        PerformanceCounter diskAvgSecWrite;
        PerformanceCounter diskCounter;
        PerformanceCounter cpuHandleCounter;
        PerformanceCounter cpuThreadCounter;
        PerformanceCounter sysContextSwitchPSec;
        PerformanceCounter sysCallsPSec;
        PerformanceCounter sysCpuQueueLength;

        public PerformanceTracker(int MiliSeconds = 100, bool EnergySaver = true, float CPU_Critical_Limit = 90, float RAM_Critical_limit = 90)
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuPrivilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
            cpuInterruptTime = new PerformanceCounter("Processor", "% Interrupt Time", "_Total");
            cpuDPCTime = new PerformanceCounter("Processor", "% DPC Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            memCommittedBytes = new PerformanceCounter("Memory", "Committed Bytes");
            memCommitLimit = new PerformanceCounter("Memory", "Commit Limit");
            memCommittedBytesInUse = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            memPoolPagedBytes = new PerformanceCounter("Memory", "Pool Paged Bytes");
            memPoolNonPagedBytes = new PerformanceCounter("Memory", "Pool Nonpaged Bytes");
            memCacheBytes = new PerformanceCounter("Memory", "Cache Bytes");
            pagingFile = new PerformanceCounter("Paging File", "% Usage", "_Total");
            diskQueueLength = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total");
            diskReadBytePSec = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            diskWriteBytePSec = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
            diskAvgSecRead = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
            diskAvgSecWrite = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
            diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            cpuHandleCounter = new PerformanceCounter("Process", "Handle Count", "_Total");
            cpuThreadCounter = new PerformanceCounter("Process", "Thread Count", "_Total");
            sysContextSwitchPSec = new PerformanceCounter("System", "Context Switches/sec");
            sysCallsPSec = new PerformanceCounter("System", "System Calls/sec");
            sysCpuQueueLength = new PerformanceCounter("System", "Processor Queue Length");

            cpu_Critcal_Limit = CPU_Critical_Limit;
            ram_Critcal_Limit = RAM_Critical_limit;

            Current_Assembly_Location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Current_Assembly_CodeBase_Location = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            if ((EnergySaver) && (MiliSeconds >= 100))
                Start(15000);
            else if ((EnergySaver) && (MiliSeconds <= 100))
                Start(15000);
            else if (!EnergySaver)
                Start(MiliSeconds);
            else
                Start(MiliSeconds);
        }

        public void Start(int MiliSeconds = 100)
        {
            Thread newThread = new Thread(() => Monitor(MiliSeconds));
            newThread.IsBackground = true;
            newThread.Priority = System.Threading.ThreadPriority.Lowest;
            newThread.Start();
        }

        public void Stop()
        {
            Environment.Exit(Environment.ExitCode);
        }

        /*Set publicly accessible Variables---->>*/
        public float Current_CPU_Usage {get; set;}
        public float Available_RAM { get; set; }
        public float Total_SYS_RAM_In_MB { get; set; }
        public float Total_SYS_RAM_In_Bytes { get; set; }
        public float RAM_Used { get; set; }
        public float Percent_RAM_Used { get; set; }
        public bool System_Performing_Critical { get; set; }
        public bool CPU_Performing_Critical { get; set; }
        public bool RAM_Performing_Critical { get; set; }
        public float SystemCalls_ByCPU_PerSec { get; set; }
        public float Get_NumThreads_EachProcessorServing { get; set; }
        public float NumThreads_CreatedByProcess_Last { get; set;}
        public float Avg_DiskRead_PerSec { get; set; }
        public float Avg_DiskWrite_PerSec { get; set; }
        public float DiskRead_BytesPerSec { get; set; }
        public float DiskWrite_BytesPerSec { get; set; }
        public List<string> Total_DiskSpace { get; set; }
        public List<string> Percent_DiskSpace_Available { get; set; }

        public DiskInfo Individual_Drive_Info { get; set; }
        public List<DiskInfo> Ready_Drive_Info { get; set; }

        public string Current_Assembly_Location { get; set; }
        public string Current_Assembly_CodeBase_Location { get; set; }
        
        private float cpu_Critcal_Limit {get; set;}
        private float ram_Critcal_Limit { get; set; }




        private void Monitor(int MiliSeconds)
        {
            if (MiliSeconds < 100)
                MiliSeconds = 100;

            while (true)
            {

                Current_CPU_Usage = getCurrentCpuUsage();
                Available_RAM = getAvailableRAM();
                Total_SYS_RAM_In_MB = getTotalRAMInMB();
                Total_SYS_RAM_In_Bytes = getTotalRAMInBytes();
                RAM_Used = getUsedRAM();
                Percent_RAM_Used = getRAMUsedInPercent();
                System_Performing_Critical = IsSystemPerformingCritical();
                CPU_Performing_Critical = IsCPUPerformingCritical();
                RAM_Performing_Critical = IsRAMPerformingCritical();
                SystemCalls_ByCPU_PerSec = getSystemCallsPerSec();
                Get_NumThreads_EachProcessorServing = getSystemProcessorQueueLength();
                NumThreads_CreatedByProcess_Last = getCPUThreadCount();
                Avg_DiskRead_PerSec = getAvgDiskReadPerSec();
                Avg_DiskWrite_PerSec = getAvgDiskWritePerSec();
                DiskRead_BytesPerSec = getDiskReadBytesPerSec();
                DiskWrite_BytesPerSec = getDiskWriteBytesPerSec();
                Total_DiskSpace = TotalDiskSpace();
                Percent_DiskSpace_Available = DiskSpaceAvailable();
                Ready_Drive_Info = Ready_DiskInfo();
                //For Testing Purposes-->
                //Console.WriteLine("Current_CPU_Usage: " + Current_CPU_Usage + "%");
                //Console.WriteLine("Available_RAM: " + Available_RAM + " MB");
                //Console.WriteLine("Total_SYS_RAM: " + Total_SYS_RAM + " MB");
                //Console.WriteLine("RAM_Used: " + RAM_Used + " MB");
                //Console.WriteLine("Percent_RAM_Used: " + Percent_RAM_Used + "%");
                //<--Testing purpose ends here
                Thread.Sleep(MiliSeconds);
            }
        }

        private float getRAMUsedInPercent()
        {
            return ((getUsedRAM() / getTotalRAMInMB()) * 100);
        }

        private float getUsedRAM()
        {
            return (getTotalRAMInMB() - getAvailableRAM());
        }

        /*PerformanceCounter("Processor", "% Processor Time", "_Total");
        The Processor\% Processor Time counter determines the percentage of time the processor is busy by measuring the percentage of time the thread of the Idle process is running and then subtracting that from 100 percent. 
         *This measurement is the amount of processor utilization*/
        private float getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        /*PerformanceCounter("Processor", "% Interrupt Time", "_Total");
        The rate, in average number of interrupts in incidents per second, at which the processor received and serviced hardware interrupts. 
         *It does not include deferred procedure calls, which are counted separately.*/
        private float getAvgInterruptsPerSec()
        {
            return cpuInterruptTime.NextValue();
        }

        /*PerformanceCounter("Processor", "% DPC Time", "_Total");
        The percentage of time that the processor spent receiving and servicing deferred procedure calls during the sample interval. 
         *Deferred procedure calls are interrupts that run at a lower priority than standard interrupts.*/
        private float getCPUDeferredProcedureCalls()
        {
            return cpuInterruptTime.NextValue();
        }

        /*PerformanceCounter("Processor", "% Privileged Time", "_Total");
        The percentage of non-idle processor time spent in privileged mode. Privileged mode is a processing mode designed for operating system components and hardware-manipulating drivers. 
         *It allows direct access to hardware and all memory. 
         *The alternative, user mode, is a restricted processing mode designed for applications, environment subsystems, and integral subsystems. 
         *The operating system switches application threads to privileged mode to gain access to operating system services. 
         *This includes time spent servicing interrupts and deferred procedure calls (DPCs). A high rate of privileged time might be caused by a large number of interrupts generated by a failing device. 
         *This counter displays the average busy time as a percentage of the sample time.*/
        private float getCPUPrivilegedTime()
        {
            return cpuPrivilegedTime.NextValue();
        }

        /*PerformanceCounter("Memory", "Available MBytes", null);
        This measures the amount of physical memory, in megabytes, available for running processes. 
         * If this value is less than 5 percent of the total physical RAM, that means there is insufficient memory, and that can increase paging activity.*/
        private float getAvailableRAM()
        {
            return ramCounter.NextValue();
        }

        /*PerformanceCounter("Memory", "Committed Bytes", null);
         it shows the amount of virtual memory, in bytes, that can be committed without having to extend the paging file(s). Committed memory is physical memory which has space reserved on the disk paging files. 
         * There can be one or more paging files on each physical drive. 
         * If the paging file(s) are expanded, this limit increases accordingly.*/
        private float getVirtualMemForCommit()
        {
            return memCommittedBytes.NextValue();
        }

        /*PerformanceCounter("Memory", "Commit Limit", null);

        it shows the amount of virtual memory, in bytes, that can be committed without having to extend the paging file(s). Committed memory is physical memory which has space reserved on the disk paging files. 
         * There can be one or more paging files on each physical drive. 
         * If the paging file(s) are expanded, this limit increases accordingly.*/
        private float getVirtualMemCommitLimit()
        {
            return memCommitLimit.NextValue();
        }

        /*PerformanceCounter("Memory", "% Committed Bytes In Use", null);

        it shows the ratio of Memory\ Committed Bytes to the Memory\ Commit Limit. Committed memory is physical memory in use for which space has been reserved in the paging file so that it can be written to disk. 
         * The commit limit is determined by the size of the paging file. 
         * If the paging file is enlarged, the commit limit increases, and the ratio is reduced.*/
        private float getMemCommittedBytesInUse()
        {
            return memCommittedBytesInUse.NextValue();
        }

        /*PerformanceCounter("Memory", "Pool Paged Bytes", null);

        it shows the size, in bytes, of the paged pool. 
         * Memory\ Pool Paged Bytes is calculated differently than Process\ Pool Paged Bytes, so it might not equal Process(_Total )\ Pool Paged Bytes.*/
        private float getMemPoolPagedBytes()
        {
            return memPoolPagedBytes.NextValue();
        }

        /*PerformanceCounter("Memory", "Pool Nonpaged Bytes", null);

        it shows the size, in bytes, of the nonpaged pool. 
         * Memory\ Pool Nonpaged Bytes is calculated differently than Process\ Pool Nonpaged Bytes, so it might not equal Process(_Total )\ Pool Nonpaged Bytes.*/
        private float getMemPoolNonPagedBytes()
        {
            return memPoolNonPagedBytes.NextValue();
        }

        /*PerformanceCounter("Memory", "Cache Bytes", null);

        it shows the sum of the values of System Cache Resident Bytes, System Driver Resident Bytes, System Code Resident Bytes, and Pool Paged Resident Bytes.*/
        private float getMemCacheBytes()
        {
            return memCacheBytes.NextValue();
        }

        /*PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");

        PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");

        it captures the total number of bytes sent to the disk (write) and retrieved from the disk (read) during write or read operations.*/

        private float getDiskReadBytesPerSec()
        {
            return diskReadBytePSec.NextValue();
        }

        private float getDiskWriteBytesPerSec()
        {
            return diskWriteBytePSec.NextValue();
        }

        /*PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");

        PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");

        it captures the average time, in seconds, of a read/write of data from/to the disk.*/
        private float getAvgDiskReadPerSec()
        {
            return diskAvgSecRead.NextValue();
        }

        private float getAvgDiskWritePerSec()
        {
            return diskAvgSecWrite.NextValue();
        }


        private float getAvaliableDiskSpace()
        {
            return diskCounter.NextValue();
        }


        /*PerformanceCounter("System", "Context Switches/sec", null);

         A context switch occurs when the kernel switches the processor from one thread to another—for example, when a thread with a higher priority than the running thread becomes ready. 
         * Context switching activity is important for several reasons. 
         * A program that monopolizes the processor lowers the rate of context switches because it does not allow much processor time for the other processes' threads. 
         * A high rate of context switching means that the processor is being shared repeatedly—for example, by many threads of equal priority. 
         * A high context-switch rate often indicates that there are too many threads competing for the processors on the system. 
         * The System\Context Switches/sec counter reports systemwide context switches.*/
        private float getSystemContextSwitchesPerSec()
        {
            return sysContextSwitchPSec.NextValue();
        }


        /*PerformanceCounter("Process", "Thread Count", "_Total");

        The number of threads created by the process. This counter does not indicate which threads are busy and which are idle. It displays the last observed value, not an average.*/
        private float getCPUThreadCount()
        {
            return cpuThreadCounter.NextValue();
        }


        /*PerformanceCounter("Process", "Handle Count", "_Total");

        the value reports the number of handles that processes opened for objects they create. Handles are used by programs to identify resources that they must access. 
         * The value of this counter tends to rise during a memory leak.*/
        private float getCPUHandleCount()
        {
            return cpuHandleCounter.NextValue();
        }


        /*PerformanceCounter("System", "System Calls/sec", null);

        This is the number of system calls being serviced by the CPU per second. By comparing the Processor's Interrupts/sec with the System Calls/sec we can get a picture of how much effort the system requires to respond to attached hardware. 
         * On a healthy system, the Interrupts per second should be negligible in comparison to the number of System Calls per second. 
         * When the system has to repeatedly call interrupts for service, it's indicative of a hardware failure.*/
        private float getSystemCallsPerSec()
        {
            return sysCallsPSec.NextValue();
        }

        /*PerformanceCounter("System", "Processor Queue Length", null);

        The System\Processor Queue Length counter is a rough indicator of the number of threads each processor is servicing.*/
        private float getSystemProcessorQueueLength()
        {
            return sysCpuQueueLength.NextValue();
        }


        private float getTotalRAMInMB()//returns in MBytes
        {
            ulong installedMemory = 0;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                installedMemory = memStatus.ullTotalPhys;
            }
            return (installedMemory / 1000000);
        }


        private float getTotalRAMInBytes()//returns in Bytes
        {
            ulong installedMemory = 0;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                installedMemory = memStatus.ullTotalPhys;
            }
            return installedMemory;
        }

        private bool IsSystemPerformingCritical(float CPU_Limit = 90, float RAM_Limit = 90)
        {
            if (CPU_Limit != cpu_Critcal_Limit)
                CPU_Limit = cpu_Critcal_Limit;

            if (RAM_Limit != ram_Critcal_Limit)
                RAM_Limit = ram_Critcal_Limit;

            float percent_RAM_Used = getRAMUsedInPercent();
            float total_CPU_Used = getCurrentCpuUsage();

            if ((percent_RAM_Used > RAM_Limit) && (total_CPU_Used > CPU_Limit))
                return true;

            return false;
        }

        private bool IsCPUPerformingCritical(float CPU_Limit = 90)
        {
            if (CPU_Limit != cpu_Critcal_Limit)
                CPU_Limit = cpu_Critcal_Limit;

            float total_CPU_Used = getCurrentCpuUsage();

            if ((total_CPU_Used > CPU_Limit))
                return true;

            return false;
        }


        private bool IsRAMPerformingCritical(float RAM_Limit = 90)
        {
            if (RAM_Limit != ram_Critcal_Limit)
                RAM_Limit = ram_Critcal_Limit;

            float percent_RAM_Used = getRAMUsedInPercent();

            if ((percent_RAM_Used > RAM_Limit))
                return true;

            return false;
        }

        private List<string> TotalDiskSpace()
        {
            List<string> DiskInfo = new List<string>();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                //There are more attributes you can use.
                //Check the MSDN link for a complete example.
                if (drive.IsReady) DiskInfo.Add(drive.Name + " :: " + drive.TotalSize);
            }

            return DiskInfo;
        }

        private List<string> DiskSpaceAvailable()
        {
            List<string> DiskInfo = new List<string>();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                //There are more attributes you can use.
                //Check the MSDN link for a complete example.
                if (drive.IsReady) {
                    long availableSpace = ((drive.TotalFreeSpace / drive.TotalSize) * 100);
                    DiskInfo.Add(drive.Name + " :: " + availableSpace + "%");
                }
                
            }

            return DiskInfo;
        }


        private List<DiskInfo> Ready_DiskInfo()
        {
            List<DiskInfo> DiskInfo = new List<DiskInfo>();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                //There are more attributes you can use.
                //Check the MSDN link for a complete example.
                if (drive.IsReady)
                {
                    DiskInfo disk = new DiskInfo
                    {
                        AvailableFreeSpace = drive.AvailableFreeSpace,
                        DriveFormat = drive.DriveFormat,
                        DriveType = drive.DriveType,
                        Name = drive.Name,
                        RootDirectory = drive.RootDirectory,
                        TotalFreeSpace = drive.TotalFreeSpace,
                        TotalSize = drive.TotalSize,
                        VolumeLabel = drive.VolumeLabel
                    };
                    DiskInfo.Add(disk);
                }

            }

            return DiskInfo;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);


        //class for tracking DiskUsage
        public class DiskInfo
        {
            public float AvailableFreeSpace { get; set; }

            public string DriveFormat { get; set; }

            public DriveType DriveType { get; set; }

            public string Name { get; set; }

            public DirectoryInfo RootDirectory { get; set; }

            public float TotalFreeSpace { get; set; }

            public float TotalSize { get; set; }

            public string VolumeLabel { get; set; }
        }
    }

}
