using System;
using System.IO.Ports;
using FrooxEngine;

namespace Serial
{
    [Category("LogiX/Test")]
    [NodeName("Serial")]
    public class Serial : LogixNode
    {
        public readonly Impulse NewData;

        public readonly Sync<string> ComPort;
        public readonly Sync<int> BaudRate;
        public readonly Sync<bool> ComEnabled;

        public readonly Input<bool> Condition;

        public readonly Output<string> Data;
        public readonly Output<bool> Test;

        readonly Sync<bool> boolOutput;
        readonly Sync<string> dataTemp;

        static SerialPort _serialPort;

        bool ComEnabled_last;

        [ImpulseTarget]
        public void Run()
        {
            if (_serialPort == null || _serialPort.IsOpen == false) return;
            if (this.Condition.Evaluate(false) == true)
            {
                _serialPort.Write("[");
            }
            else
            {
                _serialPort.Write("]");
            }
        }

        protected override void OnEvaluate()
        {
            Data.Value = dataTemp.Value;
            Test.Value = boolOutput.Value;
        }

        protected override void OnAttach()
        {
            ComEnabled.Value = false;
            ComPort.Value = "COM3";
            BaudRate.Value = 9600;
            boolOutput.Value = false;
            dataTemp.Value = "";
        }

        protected override void OnCommonUpdate()
        {
            base.OnCommonUpdate();
            if (_serialPort == null || _serialPort.IsOpen == false) return;
            if (_serialPort.BytesToRead > 0) 
            {
                //Debug.Log("Serial Port Has Data:");
                //Debug.Log(_serialPort.ReadExisting());

                dataTemp.Value = _serialPort.ReadExisting();
                if (dataTemp.Value[0] == '{')
                {
                    boolOutput.Value = true;
                }
                else if(dataTemp.Value[0] == '}')
                {
                    boolOutput.Value = false;
                }
            this.NewData.Trigger();
            }
        }

        protected override void OnChanges()
        {
            base.OnChanges();
            if (ComEnabled.Value != ComEnabled_last) {
                if (ComEnabled.Value)
                {
                    _serialPort = new SerialPort(ComPort.Value, BaudRate.Value, Parity.None, 8, StopBits.One);
                    _serialPort.Open();
                } else
                {
                    _serialPort.Close();
                }
                ComEnabled_last = ComEnabled.Value;
            }
        }
    }
}
