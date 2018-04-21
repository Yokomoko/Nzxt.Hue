using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Nzxt.Hue.Core
{
    public class SerialDriver : IDisposable
    {
        private SerialDriver()
        {
            //Nothing to do.
        }

        public SerialDriver(string portName, int baudRate) : this()
        {
            this.OpenDevice(portName, baudRate);
        }

        public SerialPort SerialPort { get; private set; }

        public byte[] Data { get; private set; }

        public int Count { get; private set; }

        private void OpenDevice(string portName, int baudRate)
        {
            this.SerialPort = new SerialPort(portName, baudRate);
            this.SerialPort.DataReceived += this.OnDataReceived;
            this.SerialPort.ErrorReceived += this.OnErrorReceived;
            this.SerialPort.Open();
            this.DetectRate();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Data = new byte[this.SerialPort.BytesToRead];
            this.Count = this.SerialPort.Read(this.Data, 0, this.Data.Length);
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DetectRate()
        {
            var rates = new[]
            {
                //4800,
                //9600,
                256000
            };
            foreach (var rate in rates)
            {
                if (this.DetectRate(rate, 3000))
                {
                    return;
                }
            }
            throw new NotImplementedException();
        }

        private bool DetectRate(int baudRate, int timeout)
        {
            var data = new byte[]
            {
                192
            };
            this.SerialPort.BaudRate = baudRate;
            this.SerialPort.DiscardInBuffer();
            this.SerialPort.DiscardOutBuffer();
            using (var reset = new ManualResetEvent(false))
            {
                var handler = new SerialDataReceivedEventHandler((sender, e) =>
                {
                    reset.Set();
                });
                this.SerialPort.DataReceived += handler;
                try
                {
                    this.Write(data);
                    return reset.WaitOne(timeout);
                }
                finally
                {
                    this.SerialPort.DataReceived -= handler;
                }
            }
        }

        public void Write(byte[] data)
        {
            this.SerialPort.Write(data, 0, data.Length);
        }

        public bool Read(ref byte[] data)
        {
            if (this.Data == null)
            {
                return false;
            }
            else if (this.Data.Length == 1)
            {
                data[0] = this.Data[0];
            }
            else if (this.Data.Length == 5)
            {
                if (this.Data[0] / 64 == 3)
                {
                    Array.Copy(this.Data, data, this.Data.Length);
                }
            }
            this.Data = null;
            this.Count = 0;
            return true;
        }

        public void Close()
        {
            this.SerialPort.Close();
        }

        public void Dispose()
        {
            this.Close();
        }

        public static IEnumerable<string> GetPortNames()
        {
            return SerialPort.GetPortNames();
        }
    }
}
