using OpenCvSharp.Extensions;
using Bitmap = System.Drawing.Bitmap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;
using OpenCvSharp;
using Tesseract;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;

namespace AlbionNavigator.Utils;

public static class MapDataParser
{
    public record struct SampleRegionData(string Source, string Target, string Timeout);
    public static SampleRegionData Parse(Bitmap sampleSrc)
    {
        // polyfill for dev environment since it loads all DLLs in memory and results in Assembly.Location being null 
        if (OS.HasFeature("editor")) TesseractEnviornment.CustomSearchPath = Path.Combine(Directory.GetCurrentDirectory(), @".godot\mono\temp\bin\Debug");
        
        var (sample, template) = PrepareSample(sampleSrc);
        using (sample)
        using (template)
        {
            var (source, target, timeout) = TemplateMatch(sample, template);
            using (source)
            using (target)
            using (timeout)
            {
                return new SampleRegionData(
                    ExtractText(source),
                    ExtractText(target),
                    ExtractTimeout(timeout)
                );            
            }
        }
    }

    private static (Mat sample, Mat template) PrepareSample(Bitmap sampleSrc)
    {
        using var rawSample = sampleSrc.ToMat();
        var scale = 1920.0 / rawSample.Width;

        using var downChanneledSample = new Mat();
        Cv2.CvtColor(rawSample, downChanneledSample, ColorConversionCodes.BGRA2BGR);

        var sample = new Mat();
        Cv2.Resize(downChanneledSample, sample, new Size(downChanneledSample.Width * scale, downChanneledSample.Height * scale));
        var template = Cv2.ImRead(GetAssetPath("res://Assets/Parsing/portal-pass-icon-fhd-2.png"));

        return (sample, template);
    }
    
    private static (Mat Source, Mat Target, Mat Timeout) TemplateMatch(Mat sample, Mat template)
    {
        using var result = new Mat();
        
        Cv2.MatchTemplate(sample, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);

        if (maxVal < .8) throw new InvalidImage("Could not match template image - portal frame missing or obstructed");
        return (
            CropMat(sample, new Rect(476, 28, 220, 35)),
            CropMat(sample, new Rect(maxLoc.X - 138, maxLoc.Y - 27, 150, 20)),
            CropMat(sample, new Rect(maxLoc.X, maxLoc.Y + 18, 65, 18))
        );
    }

    private static string OcrRead(Mat sample, string whitelist = "")
    {
        using var engine = new TesseractEngine(GetAssetPath("res://Assets/Parsing/tessdata"), "eng", EngineMode.Default);
        if (whitelist.Length > 0) engine.SetVariable("tessedit_char_whitelist", whitelist);
        
        using var pix = Pix.LoadFromMemory(sample.ToBytes());
        using var page = engine.Process(pix, PageSegMode.SingleLine);
        return page.GetText().Trim();
    }

    private static string ExtractText(Mat sample) => OcrRead(sample);
    private static string ExtractTimeout(Mat sample)
    {
        var isTimerUnderAnHour = IsTextRed(sample);
        
        using var resized = new Mat();
        Cv2.Resize(sample, resized, new Size(sample.Width * 4, sample.Height * 4));
        
        using var invertedSample = new Mat();
        Cv2.BitwiseNot(resized, invertedSample);

        using var graySample = new Mat();
        Cv2.CvtColor(invertedSample, graySample, ColorConversionCodes.BGR2GRAY);
        
        using var invBinary = new Mat();
        Cv2.Threshold(graySample, invBinary, isTimerUnderAnHour ? 170 : 150, 255, ThresholdTypes.BinaryInv);

        using var binary = new Mat();
        Cv2.BitwiseNot(invBinary, binary);

        using var trimmed = ImageCleaner.RemoveSmallDarkObjectsByArea(binary, .28);
        
        using var mask = new Mat();
        Cv2.BitwiseNot(trimmed, mask);
        if (isTimerUnderAnHour)
        {
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.Dilate(mask, mask, kernel, iterations: 1);            
        }
        else
        {
            using var verticalKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 2));
            Cv2.Erode(mask, mask, verticalKernel, iterations: 1);  
            using var horizonalKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 1));
            Cv2.Erode(mask, mask, horizonalKernel, iterations: 2);  
        }

        using var invertedMask = new Mat();
        Cv2.BitwiseNot(mask, invertedMask);
        
        var parsedSegments = new List<string>();
        foreach (var r in TextSegmenter.FindTextSegments(invertedMask, 0))
        {
            using var e = CropMat(invertedMask, r);
            using var padded = new Mat();
            Cv2.CopyMakeBorder(e, padded, 0, 0, 10, 10, BorderTypes.Constant, Scalar.White);
            
            parsedSegments.Add(OcrRead(padded, "1234567890"));
        }

        var result = $"{parsedSegments[0].PadLeft(2,'0')}:{parsedSegments[1].PadLeft(2,'0')}";
        result = isTimerUnderAnHour ? $"00:{result}" : $"{result}:00";

        return result;
    } 
    
    private static Mat CropMat(Mat source, Rect rect) => source[rect].Clone();
    
    private static bool IsTextRed(Mat image, double redRatioThreshold = 0.1)
    {
        // Convert BGR to HSV (better for color detection)
        using var hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);

        // Define two red hue ranges (red wraps around 0° and 180° in HSV)
        var lowerRed1 = new Scalar(0, 100, 100);
        var upperRed1 = new Scalar(10, 255, 255);

        var lowerRed2 = new Scalar(160, 100, 100);
        var upperRed2 = new Scalar(180, 255, 255);

        // Threshold for red regions
        using var mask1 = new Mat();
        using var mask2 = new Mat();
        Cv2.InRange(hsv, lowerRed1, upperRed1, mask1);
        Cv2.InRange(hsv, lowerRed2, upperRed2, mask2);

        // Combine both masks
        using var redMask = new Mat();
        Cv2.BitwiseOr(mask1, mask2, redMask);

        // Calculate ratio of red pixels to total pixels
        double redPixels = Cv2.CountNonZero(redMask);
        double totalPixels = image.Rows * image.Cols;
        var ratio = redPixels / totalPixels;

        return ratio > redRatioThreshold;
    }

    private static string GetAssetPath(string localPath)
    {
        return OS.HasFeature("editor") ? ProjectSettings.GlobalizePath(localPath) : OS.GetExecutablePath().GetBaseDir().PathJoin(localPath.Replace("res://", ""));
    }
}

public class InvalidImage(string message) : Exception(message);

public static class ImageCleaner
{
    public static Mat RemoveSmallDarkObjectsByArea(Mat binary, double areaRatioThreshold = 0.3)
    {
        using var invertedBinary = new Mat();
        Cv2.BitwiseNot(binary, invertedBinary);
        
        // 3. Connected components
        var labels = new Mat();
        var stats = new Mat();
        var centroids = new Mat();
        var nLabels = Cv2.ConnectedComponentsWithStats(
            invertedBinary, labels, stats, centroids,
            PixelConnectivity.Connectivity8, MatType.CV_32S
        );

        // 4. Find largest component area
        var maxArea = 0;
        for (var i = 1; i < nLabels; i++)
        {
            var area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
            if (area > maxArea)
                maxArea = area;
        }

        var minArea = (int)(maxArea * areaRatioThreshold);

        // 5. Build mask with large components only
        Mat mask = Mat.Zeros(binary.Size(), MatType.CV_8UC1);
        var temp = new Mat();

        for (var i = 1; i < nLabels; i++)
        {
            var area = stats.Get<int>(i, (int)ConnectedComponentsTypes.Area);
            if (area < minArea) continue;
            // Mask this component
            Cv2.Compare(labels, i, temp, CmpType.EQ);
            Cv2.BitwiseOr(mask, temp, mask);
        }

        // 6. Copy large dark components onto white background
        Mat result = Mat.Ones(binary.Size(), binary.Type()) * 255; // fill all white
        binary.CopyTo(result, mask);

        // Cleanup
        labels.Dispose();
        stats.Dispose();
        centroids.Dispose();
        mask.Dispose();
        temp.Dispose();

        return result;
    }
}

public static class TextSegmenter
{
    public static List<Rect> FindTextSegments(Mat src, int gapThreshold = 2)
    {
        // 1. Grayscale + binary
        var gray = new Mat();
        if (src.Channels() > 1)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        else
            gray = src.Clone();

        var binary = new Mat();
        Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        // 2. Compute vertical projection (sum of black pixels per column)
        var projection = new int[binary.Cols];
        for (var x = 0; x < binary.Cols; x++)
        {
            var colSum = 0;
            for (var y = 0; y < binary.Rows; y++)
            {
                if (binary.At<byte>(y, x) == 0) colSum++; // black pixel
            }
            projection[x] = colSum;
        }

        // 3. Find gaps
        var segments = new List<Rect>();
        var start = -1;
        for (var x = 0; x < projection.Length; x++)
        {
            switch (projection[x])
            {
                case > 0 when start < 0:
                    start = x; // start of a blob
                    break;
                case <= 0 when start >= 0:
                {
                    var width = x - start;
                    if (width > gapThreshold) // ignore tiny noise columns
                    {
                        segments.Add(new Rect(start, 0, width, binary.Rows));
                    }
                    start = -1;
                    break;
                }
            }
        }
        // handle last segment
        if (start >= 0)
        {
            segments.Add(new Rect(start, 0, binary.Cols - start, binary.Rows));
        }

        gray.Dispose();
        binary.Dispose();
        return GetDigitRects(segments);
    }

    private static List<Rect> GetDigitRects(List<Rect> segments)
    {
        var result = new List<Rect>();

        switch (segments.Count)
        {
            case < 5:
                throw new InvalidImage($"Could not parse portal timer - insufficient segments: {segments.Count} < 5");
            case 5:
                result.Add(segments[0]);
                result.Add(MergeRects(segments[2], segments[3]));
                break;
            default:
                result.Add(MergeRects(segments[0], segments[1]));
                result.Add(MergeRects(segments[3], segments[4]));
                break;
        }

        return result;
    }

    private static Rect MergeRects(Rect a, Rect b) => Rect.FromLTRB(int.Min(a.Left, b.Left), int.Min(a.Top, b.Top), int.Max(a.Right, b.Right), int.Max(a.Bottom, b.Bottom));
}

