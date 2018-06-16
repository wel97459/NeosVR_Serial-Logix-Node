using System;
using System.IO.Ports;
using System.Linq;
using FrooxEngine;

namespace Serial
{
    [Category("LogiX/Test")]
    [NodeName("Serial")]
    public class Serial : LogixNode
    {
        //Setup inspector interface 
        public readonly Sync<string> ComPort;
        public readonly Sync<int> BaudRate;
        public readonly Sync<bool> ComEnabled;
        private readonly SyncRef<User> Owner;

        //Setup logix node inputs and outputs
        public readonly Impulse NewData;
        public readonly Input<string> StrInput;
        public readonly Output<string> StrOutput;

        //Setup a sync for the income data from the serial port
        readonly Sync<string> OutputTemp;

        //Setup serial port object
        static SerialPort _serialPort;

        //Keep up with what state the checkbox was in... this my need to be a sync
        bool ComEnabled_last;

        [ImpulseTarget]
        public void Run()
        {
            //Check to see if the serial port is open, and send the string on the node input
            if (_serialPort == null || _serialPort.IsOpen == false) return;
            _serialPort.Write(StrInput.EvaluateRaw(""));
        }

        protected override void OnAttach()
        {
            //Setup the default values
            ComEnabled.Value = false;
            ComPort.Value = "COM1";
            BaudRate.Value = 9600;
            OutputTemp.Value = "";
            //We only want the owner of the node to connect there serial port
            Owner.Value = World.LocalUser.ReferenceID;
        }

        protected override void OnEvaluate()
        {
            //update the output value of the incoming string from the serial port
            StrOutput.Value = OutputTemp.Value;
        }

        protected override void OnCommonUpdate()
        {
            //Pass the callback to the base so the output are updated
            base.OnCommonUpdate();
            //Check that the serial port is connected
            if (_serialPort == null || _serialPort.IsOpen == false) return;
            //If there is data in the serial port buffer copy it to the OutputTemp
            if (_serialPort.BytesToRead > 0)
            {
                OutputTemp.Value = _serialPort.ReadExisting();
                //Trigger the new data impluse output
                this.NewData.Trigger();
            }
        }

        protected override void OnChanges()
        {
            //Pass the callback to the base so the outputs are updated on the node
            base.OnChanges();
            //Check if this is the owner of the node
            if (Owner.Value == World.LocalUser.ReferenceID)
            {
                if (ComEnabled.Value != ComEnabled_last)
                {
                    if (ComEnabled.Value)
                    {
                        //Open the serial port
                        _serialPort = new SerialPort(ComPort.Value, BaudRate.Value, Parity.None, 8, StopBits.One);
                        _serialPort.Open();
                    }
                    else
                    {
                        _serialPort.Close();
                    }
                    ComEnabled_last = ComEnabled.Value;
                }
            }
            else
            {
                ComEnabled.Value = ComEnabled_last;
            }
        }
    }
}
