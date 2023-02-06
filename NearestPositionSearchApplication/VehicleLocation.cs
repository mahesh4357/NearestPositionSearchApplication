using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestPositionSearchApplication
{
    public class VehicleLocation
    {
        public float Latitude;
        public float Longitude;
        public Int32 PositionID;
        private byte[] VehicleRegistration; /* Work natively with byte array to speed up reading from disk */
        public UInt64 RecordedTimeUTC;

        public string GetVehicleRegistration()
        {
            return System.Text.Encoding.ASCII.GetString(VehicleRegistration);
        }

        /* Constructor decodes binary data stream, starting at provided Offset, increments Offset when done */
        public VehicleLocation(byte[] BytesParam, ref int ReadOffsetParam)
        {
            PositionID = BitConverter.ToInt32(BytesParam, ReadOffsetParam);
            ReadOffsetParam += sizeof(Int32);

            int NextNullIndex = ReadOffsetParam;

            do { } while (BytesParam[++NextNullIndex] != 0); /* Find the next null */

            VehicleRegistration = new byte[NextNullIndex - ReadOffsetParam];

            Array.Copy(BytesParam, ReadOffsetParam, VehicleRegistration, 0, NextNullIndex - ReadOffsetParam);

            ReadOffsetParam += (NextNullIndex - ReadOffsetParam) + 1;

            Latitude = BitConverter.ToSingle(BytesParam, ReadOffsetParam);
            ReadOffsetParam += sizeof(float);

            Longitude = BitConverter.ToSingle(BytesParam, ReadOffsetParam);
            ReadOffsetParam += sizeof(float);

            RecordedTimeUTC = BitConverter.ToUInt64(BytesParam, ReadOffsetParam);
            ReadOffsetParam += sizeof(UInt64);
        }
    }
}
