using System;
using System.Threading;

namespace Nzxt.Hue.Core
{
    public class Manager
    {
        public static readonly byte[] LightingChannels = new byte[]
        {
            1,
            2
        };

        private Manager()
        {
            this.ReadData = new byte[2];
            this.ReadReplyData = new byte[5];
            this.WriteData = new byte[7];
            this.WriteReplyData = new byte[1];
        }

        public byte[] ReadData { get; private set; }

        public byte[] ReadReplyData { get; private set; }

        public byte[] WriteData { get; private set; }

        public byte[] WriteReplyData { get; private set; }

        public Manager(Device device) : this()
        {
            this.Device = device;
        }

        public Device Device { get; private set; }

        public int Id { get; private set; }

        public void Start()
        {
            this.GetOrAddDeviceId();
        }

        protected virtual void GetOrAddDeviceId()
        {
            this.ReadData[0] = 141;
            this.ReadData[1] = 1;
            this.Device.Write(this.ReadData);
            this.Read(this.ReadReplyData, true);
            if (this.ReadReplyData[1] == 0 && this.ReadReplyData[2] == 0)
            {
                this.AddDeviceId();
            }
            this.ConfirmDeviceId();
        }

        protected virtual void AddDeviceId()
        {
            var id = this.CreateDeviceId();
            this.WriteData[0] = 77;
            this.WriteData[1] = 0;
            this.WriteData[2] = 192;
            this.WriteData[3] = Convert.ToByte(id / 256);
            this.WriteData[4] = Convert.ToByte(id % 256);
            this.WriteData[5] = 0;
            this.WriteData[6] = 0;
            this.Device.Write(this.WriteData);
            this.Read(this.WriteReplyData, true);
            this.GetOrAddDeviceId();
        }

        protected virtual int CreateDeviceId()
        {
            return new Random().Next(300, 60000);
        }

        protected virtual void ConfirmDeviceId()
        {
            this.Id = this.ReadReplyData[1] * 256 + this.ReadReplyData[2];
        }

        public bool Read(byte[] data, bool required)
        {
            if (required)
            {
                for (var a = 0; a <= 20; a++)
                {
                    if (this.Read(data, false))
                    {
                        return true;
                    }
                    Thread.Sleep(10);
                }
                return false;
            }
            else
            {
                return this.Device.Read(data);
            }
        }

        public void SetHubState(bool value)
        {
            this.WriteData[0] = 70;
            this.WriteData[1] = 0;
            this.WriteData[2] = 192;
            this.WriteData[3] = 0;
            this.WriteData[4] = 0;
            if (value)
            {
                this.WriteData[5] = 0;
                this.WriteData[6] = 255;
            }
            else
            {
                this.WriteData[5] = 255;
                this.WriteData[6] = 0;
            }
            this.Device.Write(this.WriteData);
            this.Read(this.WriteReplyData, true);
        }

        public void SetLightingColor(byte red, byte green, byte blue)
        {
            foreach (var channel in LightingChannels)
            {
                this.SetLightingColor(channel, red, green, blue);
            }
        }

        private void SetLightingColor(byte channel, byte red, byte green, byte blue)
        {
            this.ReadData[0] = 141;
            this.ReadData[1] = channel;
            this.Device.Write(this.ReadData);
            this.Read(this.ReadReplyData, true);
            this.SetLightingColor(channel, ReadReplyData[4] * 10, red, green, blue);
        }

        private void SetLightingColor(byte channel, int count, byte red, byte green, byte blue)
        {
            var data = new byte[125];
            data[0] = 75;
            data[1] = channel;
            data[2] = 0;
            data[3] = 0;
            data[4] = 0;
            for (var a = 0; a < count; a++)
            {
                data[5 + (a * 3)] = green;
                data[6 + (a * 3)] = red;
                data[7 + (a * 3)] = blue;
            }
            this.Device.Write(data);
            this.Read(this.WriteReplyData, true);
        }
    }
}
