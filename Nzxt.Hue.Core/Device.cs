using System;
using System.Linq;
using System.Management;

namespace Nzxt.Hue.Core
{
    public class Device : IDisposable
    {
        public const string VENDOR = "04D8";

        public const string PRODUCT = "00DF";

        public Device(string vendor = VENDOR, string product = PRODUCT)
        {
            this.Vendor = vendor;
            this.Product = product;
            this.PortName = GetPortName(vendor, product);
            this.BaudRate = 4800;
            this.Open();
        }

        public string Vendor { get; private set; }

        public string Product { get; private set; }

        public string PortName { get; private set; }

        public int BaudRate { get; private set; }

        public SerialDriver Driver { get; private set; }

        protected virtual void Open()
        {
            this.Driver = new SerialDriver(this.PortName, this.BaudRate);
        }

        public bool Read(byte[] data)
        {
            return this.Driver.Read(ref data);
        }

        public void Write(byte[] data)
        {
            this.Driver.Write(data);
        }

        public void Dispose()
        {
            this.Driver.Close();
        }

        private static string GetPortName(string vendor, string product)
        {
            var path = string.Format("VID_{0}&PID_{1}&MI_00", vendor, product);
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                var managementObjects = searcher.Get()
                    .Cast<ManagementBaseObject>()
                    .Where(managementObject => managementObject["PNPDeviceID"].ToString().Contains(path))
                    .ToArray();
                foreach (var portName in SerialDriver.GetPortNames())
                {
                    foreach (var managementObject in managementObjects)
                    {
                        if (managementObject["DeviceID"].ToString().Equals(portName))
                        {
                            return portName;
                        }
                    }
                }
            }
            throw new InvalidOperationException(string.Format("Device not found: \"{0}\":\"{1}\".", vendor, product));
        }
    }
}
