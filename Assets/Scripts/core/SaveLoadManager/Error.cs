using System;
using UnityEngine;

public enum ErrorCode
{
    // Success
    Success = 200, // HTTP-style success code

    // File-related errors
    FileNotFound = 404, // File does not exist
    FileAccessDenied = 403, // Permission issues
    FileWriteError = 5001, // Failed to write to file
    FileReadError = 5002, // Failed to read from file
    FileCorrupted = 4221, // File is corrupted or invalid

    // Serialization/Deserialization errors
    SerializationError = 4001, // Failed to serialize data to JSON
    DeserializationError = 4002, // Failed to deserialize JSON to object

    // Data validation errors
    InvalidData = 4222, // Data is invalid or does not meet requirements
    MissingData = 4223, // Required data is missing

    // General errors
    UnknownError = 5000, // Generic error for unexpected issues
    NullReferenceError = 5003, // Null reference encountered
    ArgumentError = 4003 // Invalid argument passed to a method
}

public class Error
{
    public string Message { get; private set; }
    public ErrorCode MessageCode { get; private set; }
    public Exception Exception { get; private set; }

    public Error(string message = "", Exception exception = null, ErrorCode errorCode = ErrorCode.UnknownError)
    {
        Message = message;
        Exception = exception;
        MessageCode = errorCode;
    }

    public string GetFormattedErrorMessage()
    {
        string errorType = MessageCode switch
        {
            ErrorCode.Success => "Success",
            ErrorCode.FileNotFound => "File Not Found",
            ErrorCode.FileAccessDenied => "Access Denied",
            ErrorCode.FileCorrupted => "File Corrupted",
            ErrorCode.SerializationError => "Serialization Error",
            ErrorCode.DeserializationError => "Deserialization Error",
            ErrorCode.InvalidData => "Invalid Data",
            ErrorCode.MissingData => "Missing Data",
            ErrorCode.NullReferenceError => "Null Reference Error",
            ErrorCode.ArgumentError => "Invalid Argument",
            _ => "Unknown Error"
        };

        return $"Error: {errorType} (Code: {(int)MessageCode}). Message: {Message}";
    }
}