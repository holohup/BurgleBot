using System.ComponentModel;
using System.Text;
using BurgleBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BurgleBot.Plugins.DataProcessor;

public sealed class DataProcessorPlugin(ISemanticKernelService kernelService)
{
    [KernelFunction, Description("Converts an image to an ASCII picture")]
    public async Task<string> ConvertImageToAscii([Description("URL of the image")] string imageUrl)
    {
        int maxWidth = 80;
        var result = string.Empty;
        try
        {
            // Download the image as a byte array
            using HttpClient client = new HttpClient();
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

            // Load the image using ImageSharp
            using MemoryStream ms = new MemoryStream(imageBytes);
            using Image<Rgba32> image = Image.Load<Rgba32>(ms);

            // Calculate new dimensions to preserve the aspect ratio.
            // A height factor of 0.5 adjusts for the typical non-square shape of console characters.
            int newWidth = maxWidth;
            int newHeight = (int)(image.Height * ((float)newWidth / image.Width) * 0.5f);

            // Resize the image
            image.Mutate(x => x.Resize(newWidth, newHeight));

            // Define ASCII characters from dark to light.
            string asciiChars = "@%#*+=-:. ";
            
            StringBuilder asciiArt = new StringBuilder();

            // Loop through each pixel to convert to ASCII based on brightness.
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // Retrieve the pixel color
                    Rgba32 pixel = image[x, y];

                    // Compute brightness using a weighted formula for perceived luminance.
                    float brightness = (0.2126f * pixel.R + 0.7152f * pixel.G + 0.0722f * pixel.B) / 255f;

                    // Map brightness to an ASCII character.
                    int charIndex = (int)(brightness * (asciiChars.Length - 1));
                    asciiArt.Append(asciiChars[charIndex]);
                }
                asciiArt.AppendLine();
                result = asciiArt.ToString();
            }
        }        
        catch (Exception ex)
        {
            result = "Error: " + ex.Message;
        }
        return result;
    }
    

    [KernelFunction, Description("Describes an image")]
    public async Task<string> DescribeImage([Description("URL of the image")] string imageUrl)
    {
        var result = await kernelService.AskLlmAboutImage(
            "You are a professional forensic detective. Your job is to describe the image in as many details as you can. Describe this image:",
            imageUrl
        );
        var chatHistory = new ChatHistory();
        return result;
    }
}
