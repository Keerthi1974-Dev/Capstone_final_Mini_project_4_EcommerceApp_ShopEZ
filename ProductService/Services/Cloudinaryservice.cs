using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ProductService.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Width(400).Height(400).Crop("fill").Gravity("auto"),
                Folder = "ecommerce-shopez"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null) throw new Exception(result.Error.Message);
            return result.SecureUrl.ToString();
        }
    }
}