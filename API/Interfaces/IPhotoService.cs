using System;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
    
    // Task<DeletionResult> DeletePhotoAsync(string publicId, string folder);
    // Task<ImageUploadResult> UpdatePhotoAsync(IFormFile file, string publicId);
}
