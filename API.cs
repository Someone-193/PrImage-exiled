using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using UnityEngine;
namespace PrImage
{
    public static class ImageRendererAPI
    {
        public static void RenderImage(object target, string filePath, float maxWidth = 4f, float maxHeight = 4f, float distance = 2f, float duration = 30f, int targetWidth = 32, int targetHeight = 32)
        {
            Timing.RunCoroutine(RenderImageCoroutine(target, filePath, maxWidth, maxHeight, distance, duration, targetWidth, targetHeight));
        }

        private static IEnumerator<float> RenderImageCoroutine(object target, string filePath, float maxWidth, float maxHeight, float distance, float duration, int targetWidth, int targetHeight)
        {
            filePath = Paths.Exiled + filePath;
            if (!File.Exists(filePath))
            {
                Log.Warn($"No image was found under: {filePath}");
                yield break;
            }

            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D original = new Texture2D(2, 2);
            if (!original.LoadImage(imageData))
            {
                Log.Warn("Image couldn't be loaded.");
                yield break;
            }

            Texture2D scaled = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

            for (int x = 0; x < targetWidth; x++)
            {
                for (int y = 0; y < targetHeight; y++)
                {
                    float u = x / (float)targetWidth;
                    float v = y / (float)targetHeight;

                    Color color = original.GetPixelBilinear(u, v);

                    if (Plugin.Instance.Config.useColorQuantization)
                        color = QuantizeColor(color);

                    scaled.SetPixel(x, y, color);
                }
            }
            scaled.Apply();

            float pixelScaleX = maxWidth / targetWidth;
            float pixelScaleY = maxHeight / targetHeight;
            float scale = Mathf.Min(pixelScaleX, pixelScaleY);

            Vector3 forwardOrigin;
            Quaternion rotation = Quaternion.identity;

            if (target is Exiled.API.Features.Player player)
            {
                forwardOrigin = player.CameraTransform.position + player.CameraTransform.forward * distance;
                Vector3 directionToPlayer = forwardOrigin - player.CameraTransform.position;
                directionToPlayer.y = 0;
                rotation = Quaternion.LookRotation(directionToPlayer);
            }
            else if (target is Vector3 position)
            {
                forwardOrigin = position + Vector3.forward * distance;
                Vector3 directionToPosition = forwardOrigin - position;
                directionToPosition.y = 0;
                rotation = Quaternion.LookRotation(directionToPosition);
            }
            else if (target is Room room)
            {
                forwardOrigin = room.Position + Vector3.up * 2f;
                Vector3 directionToRoom = forwardOrigin - room.Position;
                directionToRoom.y = 0;
                rotation = Quaternion.LookRotation(directionToRoom);
            }
            else
            {
                Log.Debug("Invalid object type. Valid types: Player, Vector3, Room");
                yield break;
            }

            Vector3 offset = new Vector3((targetWidth * scale) / 2f, (targetHeight * scale) / 2f, 0);
            bool[,] used = new bool[targetWidth, targetHeight];

            int primitivesCreated = 0; // <== Zähler hinzufügen

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    if (used[x, y])
                        continue;

                    Color startColor = scaled.GetPixel(x, y);
                    if (startColor.a < 0.1f)
                        continue;

                    int width = 1;
                    int height = 1;

                    while (x + width < targetWidth && !used[x + width, y] && scaled.GetPixel(x + width, y) == startColor)
                        width++;

                    bool canExpand = true;
                    while (y + height < targetHeight && canExpand)
                    {
                        for (int k = 0; k < width; k++)
                        {
                            if (used[x + k, y + height] || scaled.GetPixel(x + k, y + height) != startColor)
                            {
                                canExpand = false;
                                break;
                            }
                        }
                        if (canExpand) height++;
                    }

                    for (int dx = 0; dx < width; dx++)
                        for (int dy = 0; dy < height; dy++)
                            used[x + dx, y + dy] = true;

                    Vector3 localOffset = new Vector3((x + width / 2f) * scale, (y + height / 2f) * scale, 0) - offset;
                    Vector3 worldPos = forwardOrigin + rotation * localOffset;

                    Primitive quad = Primitive.Create(PrimitiveType.Quad);
                    quad.Position = worldPos;
                    quad.Scale = new Vector3(scale * width, scale * height, scale * 0.01f);
                    quad.Color = startColor;

                    Quaternion fixedRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
                    quad.Rotation = fixedRotation;
                    quad.Flags = AdminToys.PrimitiveFlags.Visible;

                    if (duration > 0)
                        Timing.RunCoroutine(DestroyAfterDelay(quad, duration));

                    primitivesCreated++; // <== Zähler erhöhen
                }

                yield return Timing.WaitForOneFrame;
            }

            Log.Debug($"Total primitives created: {primitivesCreated}"); // <== Ausgabe am Ende
        }

        private static Color QuantizeColor(Color c, int quantizeSteps = 6)
        {
            float step = 1f / (quantizeSteps - 1);
            float r = Mathf.Round(c.r / step) * step;
            float g = Mathf.Round(c.g / step) * step;
            float b = Mathf.Round(c.b / step) * step;
            return new Color(r, g, b, c.a);
        }

        private static IEnumerator<float> DestroyAfterDelay(Primitive primitive, float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            primitive.Destroy();
        }
    }
}
