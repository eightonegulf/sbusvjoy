using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace SBUSReader
{
    enum SBUSReadStatus
    {
        WaitingForHeader,
        ReadingData,
        ReadingDone
    }

    public class ChannelUpdateReceivedEventArgs : EventArgs
    {
        public ChannelUpdateReceivedEventArgs(Channels channels)
        {
            this.channels = channels;
        }
        public Channels channels;
    }

    public class SBUSReader
    {
        private Channels channels;
        SerialPort port;

        public event EventHandler<ChannelUpdateReceivedEventArgs> ChannelUpdateReceived;

        public SBUSReader(string Port)
        {
            channels = new Channels(16);

            port = new SerialPort(Port, 100000);       
            port.DataBits = 8;
            port.Parity = Parity.Even;
            port.StopBits = StopBits.Two;
            port.DataReceived += Port_DataReceived;
            port.Open();
        }

        ~SBUSReader()
        {
            port.Close();
        }

        SBUSReadStatus sbusReadStatus = SBUSReadStatus.WaitingForHeader;
        int sbusReadIndex = 0;
        byte[] sbusFrame = new byte[24];
        static readonly UInt32 mask11bit = 0x7ff;
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            while (sp.BytesToRead > 0)
            {
                int b = sp.ReadByte();
                if(b>=0 && b<=255)ReadByte((byte)b);
            }
        }


        private void Failure()
        {
            Console.WriteLine("Frame error");
        }
        private void PrintFrame()
        {
            foreach(byte b in sbusFrame)
            {
                Console.Write(b);
                Console.Write(" ");
            }
            Console.WriteLine("");
        }

        private void FrameToChannel()
        {
            int bitsPerChannel = 11;
            int offsetInSubframe = 8;

            for (int c = 0; c < 16; c++)
            {
                int startBit = c * bitsPerChannel + offsetInSubframe;
                bool threeBytes = startBit % 8 > 5;
                int shiftL = startBit % 8;
                int shiftR = 8 - shiftL;
                int byteIndex = startBit / 8;

                if (threeBytes)
                {
                    channels.SetChannel(c, (UInt16)((sbusFrame[byteIndex] >> shiftL | sbusFrame[byteIndex + 1] << shiftR | sbusFrame[byteIndex + 2] << (shiftR + 8)) & mask11bit));
                }
                else
                {
                    channels.SetChannel(c, (UInt16)((sbusFrame[byteIndex] >> shiftL | sbusFrame[byteIndex+1] << shiftR) & mask11bit));
                }
            }
            //channels.Print();
            
            OnChannelUpdateReceived(new ChannelUpdateReceivedEventArgs(channels));
        }

        protected virtual void OnChannelUpdateReceived(ChannelUpdateReceivedEventArgs e)
        {
            EventHandler<ChannelUpdateReceivedEventArgs> handler = ChannelUpdateReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ReadByte(byte b)
        {
            switch (sbusReadStatus)
            {
                case SBUSReadStatus.WaitingForHeader:
                    if(b == 0x0F)
                    {
                        sbusReadStatus = SBUSReadStatus.ReadingData;
                        sbusFrame = new byte[25];
                        sbusFrame[0] = b;
                        sbusReadIndex = 1;
                    }
                    break;
                case SBUSReadStatus.ReadingData:
                    sbusFrame[sbusReadIndex++] = b;
                    if(sbusReadIndex == 24)
                    {
                        sbusReadStatus = SBUSReadStatus.ReadingDone;
                    }
                    break;
                case SBUSReadStatus.ReadingDone:
                    if (b == 0x00)
                    {
                        //Succesfully read frame
                        FrameToChannel();
                    }
                    else
                    {
                        //Failed to read frame
                        Failure();
                    }
                    sbusReadStatus = SBUSReadStatus.WaitingForHeader;
                    
                    break;
                default:
                    throw new Exception("status not supported");
            }
        }
    }
}
