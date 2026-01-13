// <copyright file="StorageLocation.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Enums;

/// <summary>
/// Specifies where the conversion output is stored.
/// </summary>
public enum StorageLocation
{
    /// <summary>
    /// Output is stored in the database.
    /// </summary>
    Database = 0,

    /// <summary>
    /// Output is stored in cloud storage (S3-compatible).
    /// </summary>
    CloudStorage = 1,
}
