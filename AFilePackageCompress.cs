using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

class AFilePackageCompress
{
	public static void CompressToFile(AFileCompressionMethod compressMethod, String savePath, Byte[] buffer)
	{
        Boolean ret = AFilePackage_CompressToFile(compressMethod, savePath, buffer, buffer.Length);
        if (!ret)
			throw new Exception("failed to CompressToFile to: " + savePath);
	}

	public static void CompressToFileAndCalcMd5(AFileCompressionMethod compressMethod, String savePath, Byte[] buffer, out String compressedMd5, out UInt64 compressedLength)
	{
		Byte[] md5Bytes = new byte[33];
        Boolean ret = AFilePackage_CompressToFileAndCalcMd5(compressMethod, savePath, buffer, buffer.Length, md5Bytes, out compressedLength, null);
        if (!ret)
			throw new Exception("failed to CompressToFile to: " + savePath);
		compressedMd5 = Encoding.UTF8.GetString(md5Bytes, 0, 32);
	}

	public static void CalcCompressedMd5(AFileCompressionMethod compressMethod, Byte[] buffer, out String compressedMd5, out UInt64 compressedLength)
	{
		Byte[] md5Bytes = new byte[33];
        Boolean ret = AFilePackage_CompressToFileAndCalcMd5(compressMethod, null, buffer, buffer.Length, md5Bytes, out compressedLength, null);
        if (!ret)
			throw new Exception("failed to CompressAndCalcMd5");
		compressedMd5 = Encoding.UTF8.GetString(md5Bytes, 0, 32);
	}

	[DllImport("AzureMobile.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern bool AFilePackage_CompressToFileAndCalcMd5(AFileCompressionMethod compressMethod, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPArray)]byte[] buffer, int length, Byte[] compressedMd5, out UInt64 compressedLength, Byte[] compressedData);

    [DllImport("AzureMobile.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool AFilePackage_CompressToFile(AFileCompressionMethod compressMethod, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPArray)]byte[] buffer, int length);
}
