using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using vJoyInterfaceWrap;

namespace VJoyFeeder
{
    class Program
    {
        static vJoy joystick;
        static uint id = 1;
        static int minValue = 0; 
        static int maxValue = 32000;
        static bool SetupVJoy()
        {

            joystick = new vJoy();
            if (!joystick.vJoyEnabled())
            {
                Console.Error.WriteLine("VJoy driver not enabled");
                return false;
            }            

            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return false;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return false;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return false;
            };


            if (!joystick.AcquireVJD(id))
            {
                return false;
            }
            return true;
        }
        static void Main(string[] args)
        {
            SetupVJoy();
            var r = new SBUSReader.SBUSReader(File.ReadAllLines("port.txt")[0]);
            
            sw = Stopwatch.StartNew();
            sw.Start();

            r.ChannelUpdateReceived += R_ChannelUpdateReceived;
            Console.WriteLine("Running");
            Console.ReadLine();

        }

        static Stopwatch sw;

        private static void R_ChannelUpdateReceived(object sender, SBUSReader.ChannelUpdateReceivedEventArgs e)
        {
            if (sw.ElapsedMilliseconds > 0)
            {
                Console.Title = (1000 / (sw.ElapsedMilliseconds )).ToString() + "fps";
            }
            sw.Restart();

            joystick.SetAxis((int)(e.channels.GetChannel(0) * maxValue), id, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis((int)(e.channels.GetChannel(1) * maxValue), id, HID_USAGES.HID_USAGE_Y);
            joystick.SetAxis((int)(e.channels.GetChannel(2) * maxValue), id, HID_USAGES.HID_USAGE_Z);
            joystick.SetAxis((int)(e.channels.GetChannel(3) * maxValue), id, HID_USAGES.HID_USAGE_RX);
            joystick.SetAxis((int)(e.channels.GetChannel(4) * maxValue), id, HID_USAGES.HID_USAGE_RY);
            joystick.SetAxis((int)(e.channels.GetChannel(5) * maxValue), id, HID_USAGES.HID_USAGE_RZ);
            joystick.SetAxis((int)(e.channels.GetChannel(6) * maxValue), id, HID_USAGES.HID_USAGE_SL0);
            joystick.SetAxis((int)(e.channels.GetChannel(7) * maxValue), id, HID_USAGES.HID_USAGE_SL1);

            joystick.SetBtn(e.channels.GetChannel(8) > 0.5f, id, 1);
            joystick.SetBtn(e.channels.GetChannel(9) > 0.5f, id, 2);
            joystick.SetBtn(e.channels.GetChannel(10) > 0.5f, id, 3);
            joystick.SetBtn(e.channels.GetChannel(11) > 0.5f, id, 4);
            joystick.SetBtn(e.channels.GetChannel(12) > 0.5f, id, 5);
            joystick.SetBtn(e.channels.GetChannel(13) > 0.5f, id, 6);
            joystick.SetBtn(e.channels.GetChannel(14) > 0.5f, id, 7);
            joystick.SetBtn(e.channels.GetChannel(15) > 0.5f, id, 8);
        }

    }
}
