using System.IO.Ports;

namespace LyncRingNotify
{
    class Vibrator
    {
        private SerialPort _serial;

        public Vibrator(string comPort)
        {
            _serial = new SerialPort(comPort, 115200);
            _serial.Open();
        }

        public void Vibrate(int duty)
        {
            _serial.Write("v" + duty + ".");
        }

        public void Off()
        {
            if (_serial.IsOpen)
            {
                _serial.Write("v0.");
            }
        }

        public void Close()
        {
            if (_serial.IsOpen)
            {
                _serial.Close();
            }
        }
    }
}
