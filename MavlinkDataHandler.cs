using System;
using System.IO.Ports;
using MavLink;

namespace MavLinkReader
{
	public class MavlinkDataHandler
	{
		String[] SMode = {
			"Stabilize", "Acrobatic", "Alt Hold", "Auto", "Guided", "Loiter", "RTL", "Circle",
			"Position ", "Land", "OF_Loiter", "Drift", "None", "Sport", "Flip", "Auto Tune", "Pos Hold"
		};

		Mavlink Mv = new Mavlink();
		Msg_heartbeat Hb = new Msg_heartbeat();
		Msg_sys_status Ss = new Msg_sys_status();
		Msg_power_status Ps = new Msg_power_status();
		Msg_attitude At = new Msg_attitude();
		Msg_gps_raw_int Gps = new Msg_gps_raw_int();
		Msg_vfr_hud Vfr = new Msg_vfr_hud();
		Msg_data_stream Ds = new Msg_data_stream();
		Msg_raw_pressure Rp = new Msg_raw_pressure();
		Msg_scaled_pressure Sp = new Msg_scaled_pressure();
	
		public delegate void VoidDelegate();
		public VoidDelegate OnPacketCountGreaterThanZero;

		public int PressureAbsolute { get; private set; }
		public int PressureDifference { get; private set; }
		public int Temperature { get; private set; }
		public int Total { get; private set; }
		public delegate void IntQuadDelegate(int a, int b, int c, int d);
		public IntQuadDelegate OnAtmosphericData;

		public string Mode
		{
			get
			{
				if (Hb != null && Hb.custom_mode < SMode.Length)
				{
					return SMode[Hb.custom_mode];
				}
				else
				{
					return "!Invalid!";
				}
			}
		}

		public bool Armed
		{
			get
			{
				if (Hb != null &&
					(Hb.base_mode & (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_SAFETY_ARMED) != 0)
				{
					return true;
				}
				else
					return false;
			}
		}

		public float MilliVoltage
		{
			get
			{
				if (Ss == null)
					return 0;

				return Ss.voltage_battery;
			}
		}

		public float BatteryRemaining
		{
			get
			{
				if (Ss == null)
					return 0;

				return Ss.battery_remaining;
			}
		}

		public float CurrentBattery
		{
			get
			{
				if (Ss == null)
					return 0;

				return Ss.current_battery;
			}

		}

		public float Roll
		{
			get
			{
				if (At == null)
					return 0;

				return At.roll * 180f / (float)Math.PI;
			}
		}

		public float Pitch
		{
			get
			{
				if (At == null)
					return 0;

				return At.pitch * 180f / (float)Math.PI;
			}
		}

		public float Yaw
		{
			get
			{
				if (At == null)
					return 0;

				return At.yaw * 180f / (float)Math.PI;
			}
		}

		public enum GpsFixTypes { NoFix, NoFix2, Fix2D, Fix3D, DGPS, RTK, ERROR }

		public GpsFixTypes GpsFixType
		{
			get
			{
				if (Gps == null)
					return GpsFixTypes.ERROR;

				return (GpsFixTypes)Gps.fix_type;
			}
		}

		public float Latitude
		{
			get
			{
				if (Gps == null)
					return 0;

				return Gps.lat / 10000000.0f;
			}
		}

		public float Longitude
		{
			get
			{
				if (Gps == null)
					return 0;

				return Gps.lon / 10000000.0f;
			}
		}

		public int VisibleSatellites
		{
			get
			{
				if (Gps == null)
					return 0;

				return Gps.satellites_visible;
			}
		}

		public float Altitude
		{
			get
			{
				if (Vfr == null)
					return 0;

				return Vfr.alt;
			}
		}

		public float Heading
		{
			get
			{
				if (Vfr == null)
					return 0;

				return Vfr.heading;
			}
		}

		private SerialPort serialPort;

		public MavlinkDataHandler(string port, int baud = 56700)
		{
			Mv.PacketReceived += OnPacketRecieved;

			serialPort = new SerialPort();
			serialPort.BaudRate = baud;
			serialPort.PortName = port;
			serialPort.DataReceived += DataRecieved;
			serialPort.Open();
		}

		private void DataRecieved(object sender, SerialDataReceivedEventArgs e)
		{
			int x = serialPort.BytesToRead;
			byte[] b = new byte[x];
			for (int i = 0; i < x; i++)
				b[i] = (byte)serialPort.ReadByte();
			Mv.ParseBytes(b);
		}

		private void OnPacketRecieved(object sender, MavlinkPacket e)
		{
			uint x = Mv.PacketsReceived;
			MavlinkMessage m = e.Message;

			if (m.GetType() == Hb.GetType())
				Hb = (Msg_heartbeat)e.Message;

			if (m.GetType() == Ss.GetType())
				Ss = (Msg_sys_status)e.Message;

			if (m.GetType() == Ps.GetType())
				Ps = (Msg_power_status)e.Message;

			if (m.GetType() == At.GetType())
				At = (Msg_attitude)e.Message;

			if (m.GetType() == Gps.GetType())
				Gps = (Msg_gps_raw_int)e.Message;

			if (m.GetType() == Vfr.GetType())
				Vfr = (Msg_vfr_hud)e.Message;

			if (m.GetType() == Rp.GetType())
			{
				Rp = (Msg_raw_pressure)e.Message;

			}

			if (m.GetType() == Sp.GetType())
			{
				Sp = (Msg_scaled_pressure)e.Message;
				PressureAbsolute = (int)(Sp.press_abs * 1000f);
				Temperature = Sp.temperature;
				PressureDifference = (int)(Sp.press_diff * 1000f);
				Total++;

				if (OnAtmosphericData != null)
				{
					OnAtmosphericData.Invoke(PressureAbsolute, PressureDifference, Temperature, Total);
				}
			}

			if (x > 0 && OnPacketCountGreaterThanZero != null)
			{
				OnPacketCountGreaterThanZero.Invoke();
			}
		}

		public void Dismiss()
		{
			if (serialPort.IsOpen)
				serialPort.Close();
		}

		// not sure where/how this is used
		public void RequestMav()
		{
			Ds.message_rate = 2;
			Ds.on_off = 1;
			Ds.stream_id = (byte)MAV_DATA_STREAM.MAV_DATA_STREAM_ALL;

			MavlinkPacket p = new MavlinkPacket();


		}
	}
}
