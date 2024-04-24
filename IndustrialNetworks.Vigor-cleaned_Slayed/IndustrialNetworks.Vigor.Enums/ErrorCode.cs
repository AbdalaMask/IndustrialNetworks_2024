namespace NetStudio.Vigor.Enums;

public enum ErrorCode
{
	CommunicationIsNormal = 0,
	Error_CheckSum = 2,
	Error_NumberOfDataBystesIsZero = 4,
	Error_NumberOfDataBitsExceedsTheRange = 6,
	Error_ASCIIConversion = 8,
	Error_FunctionCodeIsNotExisted = 49
}
