using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

enum WriteMode{
	TENS = 1,
	Motor = 2,
	Encoder = 3
}

public class Arduino_Access : MonoBehaviour {
	private static SerialPort SP = new SerialPort("COM4", 9600);
	public bool WaitingForRead;
	private int Mode;
	public byte[] Response;

	// TENS Message = Mode [1 BYTE] + Electrode Index [1 BYTE] + Finger Index [1 BYTE] + Finger Pad Index [1 BYTE] + Sensation [1 BYTE] + Intensity [4 BYTES]
	public void TENS_Write(float Intensity, int ElectrodeIndex, int FingerIndex, int FingerPadIndex, int Sensation){
		Mode = (byte)WriteMode.TENS;
		byte[] Message = new byte[9]{(byte)Mode ,(byte)ElectrodeIndex, (byte)FingerIndex, (byte)FingerPadIndex, (byte)Sensation, 0, 0, 0, 0 };
		Buffer.BlockCopy(BitConverter.GetBytes(Intensity),0,Message,5,4);
		WriteMessage (Message);
	}

	// Motor Message = Mode [1 BYTE] + FingerIndex [1 BYTES] + ConstrainOn [1 BYTE]
	public void Motor_Write(int FingerIndex, bool Enable){
		Mode = (byte)WriteMode.Motor;
		byte[] Message = new byte[3]{(byte)Mode, (byte)FingerIndex, Convert.ToByte(Enable)};
		WriteMessage (Message);
	}

	// Read Message = Mode [1 BYTE];
	public void Encoders_Read (){
		Mode = (byte)WriteMode.Encoder;
		byte[] Message = new byte[1]{ (byte)Mode };
		WriteMessage (Message);
	}

	private void WriteMessage (byte[] Message){
        byte[] Buff = new byte[Message.Length + 1];
		Buff[0] = (byte)Message.Length;	
		Buffer.BlockCopy(Message, 0, Buff, 1, Message.Length);
        if (!SP.IsOpen)
        {
            OpenConnection();
        }
		SP.Write (Buff, 0, (Message.Length + 1));
        WaitingForRead = true;
	}

	public void ReadResponse(){
		int ResponseSize;
		if (Mode == (byte)WriteMode.Encoder) {
			ResponseSize = 20;
		}											// Response should be Encoder Values [20 BYTES]
		else {
			ResponseSize = 1;						// Response should be ACK or NACK
		}
		byte[] Response = new byte[ResponseSize];
		SP.Read(Response,0,ResponseSize);
		WaitingForRead = false;
	}
		
	public void OpenConnection(){
		SP.Open ();
		SP.ReadTimeout = 20;
	}

	public void CloseConnection(){
		SP.Close ();
	}


}
