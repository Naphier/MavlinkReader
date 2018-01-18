using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MavLink;

namespace MavLinkReader
{
    public partial class MavLinkReaderForm : Form
    {

		MavlinkDataHandler mavlinkDataHandler;
        
        StreamWriter Sw = null;


        public MavLinkReaderForm()
        {
            InitializeComponent();
            Baud.SelectedIndex = 3;

			mavlinkDataHandler = new MavlinkDataHandler("COM8");

			mavlinkDataHandler.OnPacketCountGreaterThanZero += () =>
			{
				if (MavLinkReaderForm.ActiveForm != null)
					MavLinkReaderForm.ActiveForm.Invalidate();
			};

			mavlinkDataHandler.OnAtmosphericData += (pAbs, pDelta, temp, total) =>
			{
				if (Sw == null)
				{
					Sw = new StreamWriter(
						Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + 
						"\\Baro.csv");
					Sw.Write("\"Item\", \"Pressure\", \"Temp\", \"Difference\"\r\n");
				}
				String S = String.Format("{0}, {1}, {2}, {3}\r\n", total, pAbs, temp, pDelta);
				Sw.Write(S);
			};
        }

        private void Data(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
			/*
            int x = Serial.BytesToRead;
            byte[] b = new byte[x];
            for (int i=0;i<x;i++)
                b[i] = (byte)Serial.ReadByte();
            Mv.ParseBytes(b);
			*/
        }

        private void ReadData(object sender, EventArgs e)
        {
			/*
            Serial.BaudRate = int.Parse(Baud.Text);
            Serial.PortName = Comm.Text;
            Serial.Open();
			*/
        }

        private void Dismiss(object sender, FormClosingEventArgs e)
        {
			mavlinkDataHandler.Dismiss();
			/*
            if (Serial.IsOpen)
                Serial.Close();
				*/
        }

        private void Update(object sender, PaintEventArgs e)
        {
			if (mavlinkDataHandler == null)
				return;

            Mode.Text = mavlinkDataHandler.Mode;

			BVolts.Text = String.Format("{0:f}", 
				mavlinkDataHandler.MilliVoltage / 1000.0f);
			BPercent.Text = String.Format("{0:f}%", mavlinkDataHandler.BatteryRemaining);
			Current.Text = String.Format("{0:f}", mavlinkDataHandler.CurrentBattery / 100.0f);
			Status.Text = (mavlinkDataHandler.Armed ? "Armed" : "Not Armed");

			Roll.Text = String.Format("{0:f}", mavlinkDataHandler.Roll);
			Pitch.Text = String.Format("{0:f}", mavlinkDataHandler.Pitch);
			Yaw.Text = String.Format("{0:f}", mavlinkDataHandler.Yaw);

			GpsFix.Text = mavlinkDataHandler.GpsFixType.ToString();
			Latitude.Text = String.Format("{0:00.000000}", mavlinkDataHandler.Latitude);
			Longitude.Text = String.Format("{0:00.000000}", mavlinkDataHandler.Longitude);
			Satellites.Text = String.Format("{0}", mavlinkDataHandler.VisibleSatellites);
			Altitude.Text = String.Format("{0:f}", mavlinkDataHandler.Altitude);
			Heading.Text = String.Format("{0:f}", mavlinkDataHandler.Heading);

			/*
			BVolts.Text = String.Format("{0:f}", (float)Ss.voltage_battery / 1000.0f);
            BPercent.Text = String.Format("{0:d}%", Ss.battery_remaining);
            Current.Text = String.Format("{0:f}", (float)Ss.current_battery / 100.0f);

            if ((Hb.base_mode & (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_SAFETY_ARMED) != 0)
                Status.Text = "Armed";
            else
                Status.Text = "Not Armed";
				
			Roll.Text = String.Format("{0:f}", At.roll*180/3.1415926);
            Pitch.Text = String.Format("{0:f}", At.pitch*180/3.1415926);
            Yaw.Text = String.Format("{0:f}", At.yaw*180/3.1415926);
			GpsFix.Text = String.Format("{0:d}", Gps.fix_type);
			Latitude.Text = String.Format("{0:00.000000}", (float)Gps.lat / 10000000.0f);
            Longitude.Text = String.Format("{0:00.000000}", (float)Gps.lon / 10000000.0f);
			Satellites.Text = String.Format("{0:d}", Gps.satellites_visible);
			Altitude.Text = String.Format("{0:f}", Vfr.alt);
            Heading.Text = String.Format("{0:d}", Vfr.heading);
			*/
		}

		private void RequestMav()
        {
			mavlinkDataHandler.RequestMav();
			/*
            Ds.message_rate = 2;
            Ds.on_off = 1;
            Ds.stream_id = (byte)MAV_DATA_STREAM.MAV_DATA_STREAM_ALL;

            MavlinkPacket p = new MavlinkPacket();
			*/
		}
    }
}
