// Name:Template Project
// Submenu:Chris
// Author:
// Title:Template Project
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
CheckboxControl Amount1 = false; // [0,1] Show Original
CheckboxControl Amount2 = false; // [0,1] Show Half
CheckboxControl Amount3 = false; // [0,1] Retain Transparency
CheckboxControl Amount4 = true; // [0,1] {!Amount1} Use Original Transparency As Guideline
IntSliderControl Amount5 = 127; // [1,255] {Amount2} Reject Alpha Below
CheckboxControl Amount6 = true; // [0,1] Do Initial Blend
DoubleSliderControl Amount7 = 0.05; // [0,1] Blend Jitter
CheckboxControl Amount8 = true; // [0,1] Use Shorter Edge For Jitter Size
IntSliderControl Amount9 = 8; // [1,100] Main Blur Radius
IntSliderControl Amount10 = 105; // [0,200] Saturation Adjustment
IntSliderControl Amount11 = 1; // [-50,50] Contrast Adjustment
IntSliderControl Amount12 = 10; // [1,100] Color Sample Efficiency
IntSliderControl Amount13 = 10; // [1,100] Color Frequency Equalization
IntSliderControl Amount14 = 10; // [0,100,5] Low Color Limit
IntSliderControl Amount15 = 235; // [128,255,5] High Color Limit
IntSliderControl Amount16 = 2; // [1,5] Color Palette Length
IntSliderControl Amount17 = 20; // [2,20] Color Palette Hue Segmentation
IntSliderControl Amount18 = 4; // [1,8] Post Processing Oil Painting Brush Width
ListBoxControl Amount19 = 0; // Debug Mode|None|Show Base Layer|Show Blend Layer|Show Blend Issues|Show Base Vs Blend
CheckboxControl Amount20 = true; // [0,1] Patch Blending Issues
CheckboxControl Amount21 = false; // [0,1] AntiAlias Blending Layer
ReseedButtonControl Amount22 = 0; // [255] Reprocess Edges
IntSliderControl Amount23 = 12; // [1,100] Edge Blur Radius
DoubleSliderControl Amount24 = 0.50; // [0,1] Tile Size

#endregion

TrackingProperty<bool> showOriginal = new TrackingProperty<bool>();
TrackingProperty<bool> showHalf = new TrackingProperty<bool>();
TrackingProperty<bool> retainTransparency = new TrackingProperty<bool>();
TrackingProperty<bool> useOriginalTransparencyAsGuideline = new TrackingProperty<bool>();
TrackingProperty<int> rejectAlphaBelow = new TrackingProperty<int>();
TrackingProperty<bool> doInitialBlend = new TrackingProperty<bool>();
TrackingProperty<int> edgeBlurRadius = new TrackingProperty<int>();
TrackingProperty<int> mainBlurRadius = new TrackingProperty<int>();
TrackingProperty<int> saturationAdjustment = new TrackingProperty<int>();
TrackingProperty<int> contrastAdjustment = new TrackingProperty<int>();
TrackingProperty<int> colorSampleEfficiency = new TrackingProperty<int>();
TrackingProperty<int> colorFrequencyEqualization = new TrackingProperty<int>();
TrackingProperty<int> lowColorLimit = new TrackingProperty<int>();
TrackingProperty<int> highColorLimit = new TrackingProperty<int>();
TrackingProperty<int> colorPaletteLength = new TrackingProperty<int>();
TrackingProperty<int> colorPaletteHueSegmentation = new TrackingProperty<int>();
TrackingProperty<int> oilBrushWidth = new TrackingProperty<int>();
TrackingProperty<double> blendJitterAmount = new TrackingProperty<double>();
TrackingProperty<bool> useShorterEdgeForJitterSize = new TrackingProperty<bool>();
TrackingProperty<bool> patchBlendingIssues = new TrackingProperty<bool>();
TrackingProperty<bool> antiAliasBlendLayer = new TrackingProperty<bool>();
IntelligentColorPalette palette;
TrackingProperty<int> debugMode = new TrackingProperty<int>(); //ListBoxControl Amount17 = 0; // Debug Mode|None|Show Base Layer|Show Blend Layer|Show Blend Issues|Show Base Vs Blend
TrackingProperty<int> reseed = new TrackingProperty<int>();
TrackingProperty<double> tileSize = new TrackingProperty<double>();


void PreRenderInternal(Surface dst, Surface src)
{
    showOriginal.Value = Amount1;
    showHalf.Value = Amount2;
    retainTransparency.Value = (bool)Amount3;
    useOriginalTransparencyAsGuideline.Value = (bool)Amount4;
    rejectAlphaBelow.Value = (int)Amount5;
    doInitialBlend.Value = (bool)Amount6;
    blendJitterAmount.Value = (double)Amount7;
    useShorterEdgeForJitterSize.Value = (bool)Amount8;
    mainBlurRadius.Value = (int)Amount9;
    edgeBlurRadius.Value = (int)Amount23;
    saturationAdjustment.Value = (int)Amount10;
    contrastAdjustment.Value = (int)Amount11;
    colorSampleEfficiency.Value = (int)Amount12;
    colorFrequencyEqualization.Value = (int)Amount13;
    lowColorLimit.Value = (int)Amount14;
    highColorLimit.Value = (int)Amount15;
    colorPaletteLength.Value = (int)Amount16;
    colorPaletteHueSegmentation.Value = (int)Amount17;
    oilBrushWidth.Value = (int)Amount18;
    debugMode.Value = (int)Amount19;
    patchBlendingIssues.Value = (bool)Amount20;
    antiAliasBlendLayer.Value = (bool)Amount21;
    reseed.Value = (int)Amount22;
    tileSize.Value = (double)Amount24;
}

unsafe void RenderInternal(Surface dst, Surface src, Rectangle rect)
{


    if (retainTransparency)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
            ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
            
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (IsCancelRequested) return;

                *dstPtr = ColorBgra.FromBgra(
                    dst[x, y].B,
                    dst[x, y].G,
                    dst[x, y].R,
                    src[x, y].A);
                    
                srcPtr++;
                dstPtr++;
            }
        }
    }
}

unsafe void PostRenderInternal(Surface dst, Surface src, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;

            if (showHalf && x > dst.Width / 2)
            {
                *dstPtr = *srcPtr;
            }
            
            srcPtr++;
            dstPtr++;
        }
    }
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, float opacity, params BlendModes[] args)
{
    var byteOpacity = (byte)(int)(((float)byte.MaxValue) * opacity);

    BlendSurfaces(destination, leftHand, rightHand, byteOpacity, args);
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, byte opacity, params BlendModes[] args)
{
    try
    {
        methodStopwatch.Restart();

        for (var y = 0; y < destination.Height; y++)
        {
            for (var x = 0; x < destination.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return;
                }

                var lhs = leftHand[x, y];
                var rhs = rightHand[x, y];
                rhs.A = opacity;

                var result = lhs;

                foreach (var arg in args)
                {
                    result = BlendedPixel(result, rhs, arg);
                }

                destination[x, y] = result;
            }
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("BlendSurfaces: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void MirrorSurface(Surface dst, Surface src, bool mirrorHorizontally, bool mirrorVertically)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (!mirrorHorizontally && !mirrorVertically)
        {
            if (dst == src)
            {
                return;
            }

            dst.CopySurface(src);
        }

        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (IsCancelRequested) return;

                var srcX = x;
                var srcY = y;

                if (mirrorHorizontally)
                {
                    srcX = (src.Width - 1) - x;
                }

                if (mirrorVertically)
                {
                    srcY = (src.Height - 1) - y;
                }

                target[x, y] = src[srcX, srcY];
            }
        }

        if (dst == src)
        {
            dst.CopySurface(target);
        }

    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("MirrorSurfaces: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void JitterSurface(Surface dst, Surface src, bool jitterHorizontally, bool jitterVertically, int maxJitter, int jitterWidth)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();

        if (!jitterHorizontally && !jitterVertically)
        {
            if (dst == src)
            {
                return;
            }

            dst.CopySurface(src);
        }

        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        if (jitterHorizontally)
        {
            for (int y = 0; y < src.Height; y += jitterWidth)
            {
                var jitterAmount = rng.Next(-maxJitter, maxJitter);

                for (var x = 0; x < src.Width; x++)
                {
                    var targetX = x + jitterAmount;

                    if (targetX < 0)
                    {
                        targetX += src.Width;
                    }
                    else if (targetX >= src.Width)
                    {
                        targetX -= src.Width;
                    }

                    for (var subY = 0; subY < jitterWidth; subY++)
                    {
                        if (y + subY >= src.Height)
                        {
                            continue;
                        }
                        target[x, y + subY] = src[targetX, y + subY];
                    }
                }
            }
        }

        if (jitterVertically)
        {
            for (int x = 0; x < src.Width; x += jitterWidth)
            {
                var jitterAmount = rng.Next(-maxJitter, maxJitter);

                for (var y = 0; y < src.Height; y++)
                {
                    var targetY = y + jitterAmount;

                    if (targetY < 0)
                    {
                        targetY += src.Height;
                    }
                    else if (targetY >= src.Height)
                    {
                        targetY -= src.Height;
                    }

                    for (var subX = 0; subX < jitterWidth; subX++)
                    {
                        if (x + subX >= src.Width)
                        {
                            continue;
                        }

                        target[x + subX, y] = src[x + subX, targetY];
                    }
                }
            }
        }


        if (dst == src)
        {
            dst.CopySurface(target);
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("JitterSurface: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void FullyQuarterSurfaceAroundSelectionCenter(Surface dst, Surface src)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        var target = dst;
        if (dst == src)
        {
            target = new Surface(dst.Size);
        }

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (IsCancelRequested) return;

                var srcX = x;
                var srcY = y;

                if (x < selectionCenterX && y < selectionCenterY)
                {
                    srcX += selectionCenterX;
                    srcY += selectionCenterY;

                }
                else if (x >= selectionCenterX && y < selectionCenterY)
                {
                    srcX -= selectionCenterX;
                    srcY += selectionCenterY;
                }
                else if (x < selectionCenterX && y >= selectionCenterY)
                {
                    srcX += selectionCenterX;
                    srcY -= selectionCenterY;
                }
                else // (x >= selectionCenterX && y >= selectionCenterY)
                {
                    srcX -= selectionCenterX;
                    srcY -= selectionCenterY;
                }

                target[x, y] = src[srcX, srcY];
            }
        }

        if (dst == src)
        {
            dst.CopySurface(target);
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("FullyQuarterSurfaceAroundSelectionCenter: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private float Normalize(int thisValue, int minimum, int maximum)
{
    return (((float)(thisValue - minimum)) / ((float)(maximum - minimum)));
}

void Tile(Surface dst, Surface src, Rectangle rect, double tileSize)
{
    if (IsCancelRequested) return;
    
    RotateZoomEffect effect = new RotateZoomEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

    parameters.SetPropertyValue(RotateZoomEffect.PropertyNames.Tiling, true);
    parameters.SetPropertyValue(RotateZoomEffect.PropertyNames.Zoom, tileSize);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

    effect.Render(new Rectangle[1] { rect }, 0, 1);
}

void GaussianBlur(Surface dst, Surface src, Rectangle rect, int radius)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        GaussianBlurEffect effect = new GaussianBlurEffect();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, radius);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("GaussianBlur: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void Relief(Surface dst, Surface src, Rectangle rect, double angle)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        ReliefEffect effect = new ReliefEffect();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(ReliefEffect.PropertyNames.Angle, angle);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("Relief: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void OilPainting(Surface dst, Surface src, Rectangle rect, int brushSize, int coarseness)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        OilPaintingEffect effect = new OilPaintingEffect();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(OilPaintingEffect.PropertyNames.BrushSize, brushSize);
        parameters.SetPropertyValue(OilPaintingEffect.PropertyNames.Coarseness, coarseness);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("OilPainting: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void BrightnessAndContrast(Surface dst, Surface src, Rectangle rect, int brightness, int contrast)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        BrightnessAndContrastAdjustment effect = new BrightnessAndContrastAdjustment();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, brightness);
        parameters.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, contrast);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("BrightnessAndContrast: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

void HueAndSaturation(Surface dst, Surface src, Rectangle rect, int hue, int saturation)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        HueAndSaturationAdjustment effect = new HueAndSaturationAdjustment();
        PropertyCollection properties = effect.CreatePropertyCollection();
        PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);

        parameters.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Hue, hue);
        parameters.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Saturation, saturation);
        effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));

        effect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("HueAndSaturation: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private ColorBgra GetAverageBGRColor(ColorBgra colorOne, ColorBgra colorTwo)
{
    var avgR = (byte)(int)(((float)colorOne.R + colorTwo.R) / 2f);
    var avgG = (byte)(int)(((float)colorOne.G + colorTwo.G) / 2f);
    var avgB = (byte)(int)(((float)colorOne.B + colorTwo.B) / 2f);

    return ColorBgra.FromBgr(avgB, avgG, avgR);
}

private byte GetTone(ColorBgra color)
{
    var colorSum = (float)(color.R + color.G + color.B);

    return (byte)(int)(colorSum / 3f);
}

private static Random rng = new Random();

public void Shuffle<T>(IList<T> list)
{
    if (IsCancelRequested) return;

    int n = list.Count;
    while (n > 1)
    {
        n--;
        int k = rng.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
    }
}

private void ApplyAlphaThresholdToSurface(Surface source, int thresholdBelowBecomes0, int thresholdAboveBecomes255)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return;
                }

                if (source[x, y].A < thresholdBelowBecomes0)
                {
                    source[x, y] = ColorBgra.FromBgra(
                        source[x, y].B,
                        source[x, y].G,
                        source[x, y].R,
                        byte.MinValue);
                }
                else if (source[x, y].A > thresholdAboveBecomes255)
                {
                    source[x, y] = ColorBgra.FromBgra(
                        source[x, y].B,
                        source[x, y].G,
                        source[x, y].R,
                        byte.MaxValue);
                }

            }
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("ApplyAlphaThresholdToSurface: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

private Surface GetSurfaceFromToneBand(Surface source, int toneStart, int toneEnd)
{
    var surface = new Surface(source.Size);

    try
    {
        methodStopwatch.Restart();
        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                if (IsCancelRequested)
                {
                    return surface;
                }

                var tone = ((source[x, y].R + source[x, y].R + source[x, y].R) / 3);
                if (tone >= toneStart && tone <= toneEnd)
                {
                    surface[x, y] = source[x, y];
                }
                else
                {
                    surface[x, y] = ColorBgra.TransparentBlack;
                }
            }
        }

        return surface;
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("GetSurfaceFromToneBand: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

public class IntelligentColorPalette
{
    private ColorBgra[][] colorPaletteByHueSegment;

    private int lowToneBandLimit = 0;
    private int highToneBandLimit = 255;

    public void InitializePalette(Surface surface, int colorSampleEfficiency,
        int colorFrequencyEqualizer, int lowColorLimit, int highColorLimit, int colorPaletteLength, int colorPaletteHueSegmentation)
    {
        var alwaysZero = 0;

        var colorCounts = new Dictionary<ColorBgra, int>();
        lowToneBandLimit = lowColorLimit;
        highToneBandLimit = highColorLimit;

        ColorBgra CurrentPixel;
        for (int y = 0; y < surface.Height; y += colorSampleEfficiency)
        {
            for (int x = 0; x < surface.Width; x += colorSampleEfficiency)
            {
                var color = surface[x, y];
                color.A = 255;

                var toneBand = ((surface[x, y].R + surface[x, y].G + surface[x, y].B) / 3);

                if (toneBand < lowColorLimit || toneBand > highColorLimit)
                {
                    continue;
                }

                if (!colorCounts.ContainsKey(color))
                {
                    colorCounts.Add(color, 1);
                }
                else
                {
                    colorCounts[color] += 1;
                }
            }
        }

        var toneBandCount = 2 + (int)Math.Pow(2, colorPaletteLength);

        var prePaletteColorsByToneBand = new List<ColorBgra>[toneBandCount];

        for (var i = 0; i < toneBandCount; i++)
        {
            prePaletteColorsByToneBand[i] = new List<ColorBgra>();
        }

        foreach (var colorCount in colorCounts)
        {
            var band = (colorCount.Key.R + colorCount.Key.G + colorCount.Key.B) / 3f;
            var normalized = band / 255f;
            var final = normalized * toneBandCount;
            var bandIndex = (int)(final);

            if (bandIndex == toneBandCount)
            {
                bandIndex -= 1;
            }

            if (colorCount.Value < colorFrequencyEqualizer)
            {
                prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                continue;
            }
            else
            {
                var newColorCount = colorCount.Value / colorFrequencyEqualizer;

                for (var i = 0; i < newColorCount; i++)
                {
                    prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                }
            }
        }

        var toneBandPalettes = new ColorBgra[toneBandCount][];

        for (var toneBandIndex = 0; toneBandIndex < toneBandPalettes.Length; toneBandIndex++)
        {
            toneBandPalettes[toneBandIndex] = GetColorPalette(prePaletteColorsByToneBand[toneBandIndex].ToArray(), colorPaletteLength);
        }

        colorPaletteByHueSegment = new ColorBgra[colorPaletteHueSegmentation][];

        for (var i = 0; i < colorPaletteByHueSegment.Length; i++)
        {
            colorPaletteByHueSegment[i] = new ColorBgra[toneBandCount];
        }

        for (var hueIndex = 0; hueIndex < colorPaletteByHueSegment.Length; hueIndex++)
        {
            for (var toneBandIndex = 0; toneBandIndex < toneBandCount; toneBandIndex++)
            {
                foreach (var color in toneBandPalettes[toneBandIndex])
                {
                    var colorHueSegmentIndex = GetHueSegmentFromHsvColor(HsvColor.FromColor(color.ToColor()), colorPaletteHueSegmentation);

                    if (colorHueSegmentIndex == hueIndex)
                    {
                        colorPaletteByHueSegment[hueIndex][toneBandIndex] = color;
                        break;
                    }
                }
            }
        }
    }

    public void CopyPaletteToSurface(Surface surface, bool useHalf)
    {
        if (colorPaletteByHueSegment == null)
        {
            return;
        }

        var width = useHalf ? surface.Width / 2 : surface.Width;
        var height = surface.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var xPercentage = (float)x / (float)width;
                var yPercentage = (float)y / (float)height;

                var hueIndex = (int)(yPercentage * colorPaletteByHueSegment.Length);
                if (hueIndex == colorPaletteByHueSegment.Length) hueIndex -= 1;

                var colorIndex = (int)(xPercentage * colorPaletteByHueSegment[hueIndex].Length);
                if (colorIndex == colorPaletteByHueSegment[hueIndex].Length) colorIndex -= 1;

                surface[x, y] = colorPaletteByHueSegment[hueIndex][colorIndex];
            }
        }
    }

    public void ReplaceSurfaceColorsWithPalette(Surface destination, Surface source, Rectangle rect)
    {
        if (colorPaletteByHueSegment == null)
        {
            return;
        }

        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                if (source[x, y].A == 0) continue;

                var nearestPaletteColor = GetNearestPaletteColor(source[x, y]);

                if (nearestPaletteColor == ColorBgra.TransparentBlack)
                {
                    ForceSetColorAsPaletteColor(source[x, y]);
                    nearestPaletteColor = source[x, y];
                }

                destination[x, y] = nearestPaletteColor;
            }
        }
    }

    public ColorBgra GetNearestPaletteColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }
        else if (hueSegment >= colorPaletteByHueSegment.Length)
        {
            throw new NotSupportedException("Can't get hue index " + hueSegment.ToString() + " as there are only " + colorPaletteByHueSegment.Length.ToString() + " hues.");
        }

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);

        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }
        else if (colorIndex > palette.Length)
        {
            throw new NotSupportedException("Can't get color index " + colorIndex.ToString() + " as there are only " + palette.Length.ToString() + " palettes.");
        }

        return palette[colorIndex];
    }

    private void ForceSetColorAsPaletteColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }
        else if (hueSegment >= colorPaletteByHueSegment.Length)
        {
            throw new NotSupportedException("Can't get hue index " + hueSegment.ToString() + " as there are only " + colorPaletteByHueSegment.Length.ToString() + " hues.");
        }

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);

        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }
        else if (colorIndex > palette.Length)
        {
            throw new NotSupportedException("Can't get color index " + colorIndex.ToString() + " as there are only " + palette.Length.ToString() + " palettes.");
        }

        palette[colorIndex] = color;
    }

    private int GetHueSegmentFromHsvColor(HsvColor color, int numberOfHueSegments)
    {
        if (color.Hue == 0)
        {
            return 1;
        }
        else if (color.Hue == 360)
        {
            return numberOfHueSegments;
        }

        var segmentSize = 360f / (float)numberOfHueSegments;

        for (var i = 0; i < numberOfHueSegments; i++)
        {
            if (color.Hue > segmentSize * i && color.Hue <= segmentSize * (i + 1))
            {
                return i;
            }
        }

        return 0;
    }

    private ColorBgra Spread(ColorBgra[] colors)
    {
        var minimum = new ColorBgra();
        minimum.R = 255;
        minimum.G = 255;
        minimum.B = 255;
        var maximum = new ColorBgra();
        var result = new ColorBgra();

        foreach (var color in colors)
        {
            if (color.R < minimum.R) { minimum.R = color.R; }
            if (color.R > maximum.R) { maximum.R = color.R; }
            if (color.G < minimum.G) { minimum.G = color.G; }
            if (color.G > maximum.G) { maximum.G = color.G; }
            if (color.B < minimum.B) { minimum.B = color.B; }
            if (color.B > maximum.B) { maximum.B = color.B; }
        }

        result.R = (byte)(maximum.R - minimum.R);
        result.G = (byte)(maximum.G - minimum.G);
        result.B = (byte)(maximum.B - minimum.B);

        return result;
    }

    private Tuple<ColorBgra[], ColorBgra[]> Partition(ColorBgra[] colors)
    {
        var spread = Spread(colors);

        var colorList = colors.ToList();

        colorList.Sort((x, y) =>
        {
            if (spread.R >= spread.G && spread.R >= spread.B)
            {
                return y.R.CompareTo(x.R);
            }
            else if (spread.G >= spread.R && spread.G >= spread.B)
            {
                return y.G.CompareTo(x.G);
            }
            else
            {
                return y.B.CompareTo(x.B);
            }
        });

        var result = new Tuple<ColorBgra[], ColorBgra[]>
            (
                colorList.Take(colorList.Count / 2).ToArray(),
                colorList.Skip(colorList.Count / 2).ToArray()
            );

        return result;
    }

    private ColorBgra Average(ColorBgra[] colors)
    {
        if (colors.Length == 0)
        {
            return new ColorBgra();
        }

        var sumR = 0;
        var sumG = 0;
        var sumB = 0;

        foreach (var color in colors)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
        }

        return new ColorBgra()
        {
            R = (byte)(sumR / colors.Length),
            G = (byte)(sumG / colors.Length),
            B = (byte)(sumB / colors.Length),
            A = 255
        };
    }

    private ColorBgra[] GetColorPalette(ColorBgra[] colors, int iterations)
    {
        var firstPartitions = Partition(colors);
        var partitions = new List<ColorBgra[]>();

        partitions.Add(firstPartitions.Item1);
        partitions.Add(firstPartitions.Item2);

        for (var i = 0; i < iterations; i++)
        {
            var results = new List<ColorBgra[]>();
            foreach (var partition in partitions)
            {
                var result = Partition(partition);
                results.Add(result.Item1);
                results.Add(result.Item2);
            }

            partitions = results;
        }

        var palette = new ColorBgra[partitions.Count];

        for (var i = 0; i < palette.Length; i++)
        {
            palette[i] = Average(partitions[i]);
        }

        return palette;
    }
}

public class FloodFillColorSet
{
    public ColorBgra initialColor;
    public ColorBgra currentColor;
}

void FloodFill(int x, int y, Surface dst, Rectangle rect, Func<FloodFillColorSet, bool> comparison, Func<FloodFillColorSet, ColorBgra> fillOperation)
{
    var stack = new Stack<Tuple<int, int>>();

    stack.Push(new Tuple<int, int>(x, y));

    Tuple<int, int> next;

    var set = new FloodFillColorSet();
    set.initialColor = dst[x, y];

    //while(false)
    while (stack.Count > 0)
    {
        if (IsCancelRequested) return;

        next = stack.Pop();

        x = next.Item1;
        y = next.Item2;

        if ((x < 0) || (x >= rect.Right)) continue;
        if ((y < 0) || (y >= rect.Bottom)) continue;


        set.currentColor = dst[x, y];

        if (comparison(set))
        {
            if (IsCancelRequested) return;

            dst[x, y] = fillOperation(set);

            stack.Push(new Tuple<int, int>(x + 1, y));
            stack.Push(new Tuple<int, int>(x, y + 1));
            stack.Push(new Tuple<int, int>(x - 1, y));
            stack.Push(new Tuple<int, int>(x, y - 1));

        }
    }
}

public enum BlendModes
{
    Normal,
    Additive,
    Average,
    Blue,
    Color,
    ColorBurn,
    ColorDodge,
    Cyan,
    Darken,
    Difference,
    Divide,
    Exclusion,
    Glow,
    GrainExtract,
    GrainMerge,
    Green,
    HardLight,
    HardMix,
    Hue,
    Lighten,
    LinearBurn,
    LinearDodge,
    LinearLight,
    Luminosity,
    Magenta,
    Multiply,
    Negation,
    Overlay,
    Phoenix,
    PinLight,
    Red,
    Reflect,
    Saturation,
    Screen,
    SignedDifference,
    SoftLight,
    Stamp,
    VividLight,
    Yellow
}

private BinaryPixelOp normalOp = null;

private ColorBgra BlendedPixel(ColorBgra lhs, ColorBgra rhs, BlendModes blendMode)
{
    if (normalOp == null)
    {
        normalOp = new UserBlendOps.NormalBlendOp();
    }

    switch (blendMode)
    {
        case BlendModes.Normal:
            return normalOp.Apply(lhs, rhs);

        case BlendModes.Additive:
            byte r1 = Int32Util.ClampToByte(lhs.R + rhs.R);
            byte g1 = Int32Util.ClampToByte(lhs.G + rhs.G);
            byte b1 = Int32Util.ClampToByte(lhs.B + rhs.B);
            byte a1 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b1, g1, r1, a1);

        case BlendModes.Average:
            byte r2 = Int32Util.ClampToByte((lhs.R + rhs.R) / 2);
            byte g2 = Int32Util.ClampToByte((lhs.G + rhs.G) / 2);
            byte b2 = Int32Util.ClampToByte((lhs.B + rhs.B) / 2);
            byte a2 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b2, g2, r2, a2);

        case BlendModes.Blue:
            byte a3 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(rhs.B, lhs.G, lhs.R, a3);

        //case BlendModes.Clone:
        //    byte r3 = 0;
        //    byte g3 = 0;
        //    byte b3 = 0;
        //    byte a4 = normalOp.Apply(lhs, rhs).A;
        //    return ColorBgra.FromBgra(b3, g3, r3, a4);

        case BlendModes.Color:
            HsvColor hsvColor1 = HsvColor.FromColor(lhs);
            HsvColor hsvColor2 = HsvColor.FromColor(rhs);
            Color color1 = new HsvColor()
            {
                Hue = hsvColor2.Hue,
                Saturation = hsvColor2.Saturation,
                Value = hsvColor1.Value
            }.ToColor();
            byte a5 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(color1.B, color1.G, color1.R, a5);

        case BlendModes.ColorBurn:
            byte r4 = rhs.R == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R << 8) / rhs.R);
            byte g4 = rhs.G == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G << 8) / rhs.G);
            byte b4 = rhs.B == (byte)0 ? (byte)0 : Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B << 8) / rhs.B);
            byte a6 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b4, g4, r4, a6);

        case BlendModes.ColorDodge:
            byte r5 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.R << 8) / (byte.MaxValue - rhs.R));
            byte g5 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.G << 8) / (byte.MaxValue - rhs.G));
            byte b5 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte((lhs.B << 8) / (byte.MaxValue - rhs.B));
            byte a7 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b5, g5, r5, a7);

        case BlendModes.Cyan:
            byte a8 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(rhs.B, rhs.G, lhs.R, a8);

        case BlendModes.Darken:
            byte r6 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R));
            byte g6 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G));
            byte b6 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B));
            byte a9 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b6, g6, r6, a9);

        case BlendModes.Difference:
            byte r7 = Int32Util.ClampToByte(Math.Abs(lhs.R - rhs.R));
            byte g7 = Int32Util.ClampToByte(Math.Abs(lhs.G - rhs.G));
            byte b7 = Int32Util.ClampToByte(Math.Abs(lhs.B - rhs.B));
            byte a10 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b7, g7, r7, a10);

        case BlendModes.Divide:
            byte r8 = Int32Util.ClampToByte(lhs.R * byte.MaxValue / (rhs.R + 1));
            byte g8 = Int32Util.ClampToByte(lhs.G * byte.MaxValue / (rhs.G + 1));
            byte b8 = Int32Util.ClampToByte(lhs.B * byte.MaxValue / (rhs.B + 1));
            byte a11 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b8, g8, r8, a11);

        case BlendModes.Exclusion:
            byte r9 = Int32Util.ClampToByte(lhs.R + rhs.R - 2 * lhs.R * rhs.R / byte.MaxValue);
            byte g9 = Int32Util.ClampToByte(lhs.G + rhs.G - 2 * lhs.G * rhs.G / byte.MaxValue);
            byte b9 = Int32Util.ClampToByte(lhs.B + rhs.B - 2 * lhs.B * rhs.B / byte.MaxValue);
            byte a12 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b9, g9, r9, a12);

        //case BlendModes.Freeze:
        //    byte r10 = rhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.R), 2.0)) / (double)rhs.R) : (byte)0;
        //    byte g10 = rhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.G), 2.0)) / (double)rhs.G) : (byte)0;
        //    byte b10 = rhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - lhs.B), 2.0)) / (double)rhs.B) : (byte)0;
        //    byte a13 = normalOp.Apply(lhs, rhs).A;
        //    return ColorBgra.FromBgra(b10, g10, r10, a13);

        case BlendModes.Glow:
            byte r11 = Int32Util.ClampToByte(lhs.R == byte.MaxValue ? byte.MaxValue : rhs.R * rhs.R / (byte.MaxValue - lhs.R));
            byte g11 = Int32Util.ClampToByte(lhs.G == byte.MaxValue ? byte.MaxValue : rhs.G * rhs.G / (byte.MaxValue - lhs.G));
            byte b11 = Int32Util.ClampToByte(lhs.B == byte.MaxValue ? byte.MaxValue : rhs.B * rhs.B / (byte.MaxValue - lhs.B));
            byte a14 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b11, g11, r11, a14);

        case BlendModes.Green:
            byte a15 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(lhs.B, rhs.G, lhs.R, a15);

        case BlendModes.GrainExtract:
            byte r12 = Int32Util.ClampToByte(lhs.R - rhs.R + 128);
            byte g12 = Int32Util.ClampToByte(lhs.G - rhs.G + 128);
            byte b12 = Int32Util.ClampToByte(lhs.B - rhs.B + 128);
            byte a16 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b12, g12, r12, a16);

        case BlendModes.GrainMerge:
            byte r13 = Int32Util.ClampToByte(lhs.R + rhs.R - 128);
            byte g13 = Int32Util.ClampToByte(lhs.G + rhs.G - 128);
            byte b13 = Int32Util.ClampToByte(lhs.B + rhs.B - 128);
            byte a17 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b13, g13, r13, a17);

        case BlendModes.HardLight:
            byte a18 = normalOp.Apply(lhs, rhs).A;
            byte r14 = rhs.R > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.R) * (byte.MaxValue - 2 * (rhs.R - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.R * rhs.R / byte.MaxValue);
            byte g14 = rhs.G > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.G) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.G * rhs.G / byte.MaxValue);
            byte b14 = rhs.B > (byte)128 ? Int32Util.ClampToByte(byte.MaxValue - (byte.MaxValue - lhs.B) * (byte.MaxValue - 2 * (rhs.G - 128)) / byte.MaxValue) : Int32Util.ClampToByte(2 * lhs.B * rhs.B / byte.MaxValue);
            return ColorBgra.FromBgra(b14, g14, r14, a18);

        case BlendModes.HardMix:
            byte a19 = normalOp.Apply(lhs, rhs).A;
            byte r15 = rhs.R >= byte.MaxValue - lhs.R ? byte.MaxValue : (byte)0;
            byte g15 = rhs.G >= byte.MaxValue - lhs.G ? byte.MaxValue : (byte)0;
            byte b15 = rhs.B >= byte.MaxValue - lhs.B ? byte.MaxValue : (byte)0;
            return ColorBgra.FromBgra(b15, g15, r15, a19);

        //case BlendModes.Heat:
        //    byte r16 = lhs.R != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.R), 2.0)) / (double)lhs.R) : (byte)0;
        //    byte g16 = lhs.G != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.G), 2.0)) / (double)lhs.G) : (byte)0;
        //    byte b16 = lhs.B != (byte)0 ? DoubleUtil.ClampToByte(((double)byte.MaxValue - Math.Pow((double)(byte.MaxValue - rhs.B), 2.0)) / (double)lhs.B) : (byte)0;
        //    byte a20 = normalOp.Apply(lhs, rhs).A;
        //    return ColorBgra.FromBgra(b16, g16, r16, a20);

        case BlendModes.Hue:
            HsvColor hsvColor3 = HsvColor.FromColor(lhs);
            HsvColor hsvColor4 = HsvColor.FromColor(rhs);
            Color color2 = new HsvColor()
            {
                Hue = hsvColor4.Hue,
                Saturation = hsvColor3.Saturation,
                Value = hsvColor3.Value
            }.ToColor();
            byte a21 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(color2.B, color2.G, color2.R, a21);

        //case BlendModes.Interpolation:
        //    byte r17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.R) - 64.0 * Math.Cos(Math.PI * (double)rhs.R));
        //    byte g17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.G) - 64.0 * Math.Cos(Math.PI * (double)rhs.G));
        //    byte b17 = DoubleUtil.ClampToByte(128.0 - 64.0 * Math.Cos(Math.PI * (double)lhs.B) - 64.0 * Math.Cos(Math.PI * (double)rhs.B));
        //    byte a22 = normalOp.Apply(lhs, rhs).A;
        //    return ColorBgra.FromBgra(b17, g17, r17, a22);

        case BlendModes.Lighten:
            byte r18 = Int32Util.ClampToByte((int)Math.Max(lhs.R, rhs.R));
            byte g18 = Int32Util.ClampToByte((int)Math.Max(lhs.G, rhs.G));
            byte b18 = Int32Util.ClampToByte((int)Math.Max(lhs.B, rhs.B));
            byte a23 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b18, g18, r18, a23);

        case BlendModes.LinearBurn:
            byte r19 = Int32Util.ClampToByte(lhs.R + rhs.R - byte.MaxValue);
            byte g19 = Int32Util.ClampToByte(lhs.G + rhs.G - byte.MaxValue);
            byte b19 = Int32Util.ClampToByte(lhs.B + rhs.B - byte.MaxValue);
            byte a24 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b19, g19, r19, a24);

        case BlendModes.LinearDodge:
            byte r20 = Int32Util.ClampToByte(lhs.R + rhs.R);
            byte g20 = Int32Util.ClampToByte(lhs.G + rhs.G);
            byte b20 = Int32Util.ClampToByte(lhs.B + rhs.B);
            byte a25 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b20, g20, r20, a25);

        case BlendModes.LinearLight:
            byte r21 = Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue);
            byte g21 = Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue);
            byte b21 = Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue);
            byte a26 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b21, g21, r21, a26);

        case BlendModes.Luminosity:
            HsvColor hsvColor5 = HsvColor.FromColor(lhs);
            HsvColor hsvColor6 = HsvColor.FromColor(rhs);
            Color color3 = new HsvColor()
            {
                Hue = hsvColor5.Hue,
                Saturation = hsvColor5.Saturation,
                Value = hsvColor6.Value
            }.ToColor();
            byte a27 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(color3.B, color3.G, color3.R, a27);

        case BlendModes.Magenta:
            byte a28 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(rhs.B, lhs.G, rhs.R, a28);

        case BlendModes.Multiply:
            byte r22 = Int32Util.ClampToByte(lhs.R * rhs.R / byte.MaxValue);
            byte g22 = Int32Util.ClampToByte(lhs.G * rhs.G / byte.MaxValue);
            byte b22 = Int32Util.ClampToByte(lhs.B * rhs.B / byte.MaxValue);
            byte a29 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b22, g22, r22, a29);

        case BlendModes.Negation:
            byte r23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.R - rhs.R));
            byte g23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.G - rhs.G));
            byte b23 = Int32Util.ClampToByte(byte.MaxValue - Math.Abs(byte.MaxValue - lhs.B - rhs.B));
            byte a30 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b23, g23, r23, a30);

        case BlendModes.Overlay:
            byte r24 = lhs.R < (byte)128 ? Int32Util.ClampToByte(2 * rhs.R * lhs.R / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.R) * (byte.MaxValue - lhs.R) / byte.MaxValue);
            byte g24 = lhs.G < (byte)128 ? Int32Util.ClampToByte(2 * rhs.G * lhs.G / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.G) * (byte.MaxValue - lhs.G) / byte.MaxValue);
            byte b24 = lhs.B < (byte)128 ? Int32Util.ClampToByte(2 * rhs.B * lhs.B / byte.MaxValue) : Int32Util.ClampToByte(byte.MaxValue - 2 * (byte.MaxValue - rhs.B) * (byte.MaxValue - lhs.B) / byte.MaxValue);
            byte a31 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b24, g24, r24, a31);

        case BlendModes.Phoenix:
            byte r25 = Int32Util.ClampToByte((int)Math.Min(lhs.R, rhs.R) - (int)Math.Max(lhs.R, rhs.R) + byte.MaxValue);
            byte g25 = Int32Util.ClampToByte((int)Math.Min(lhs.G, rhs.G) - (int)Math.Max(lhs.G, rhs.G) + byte.MaxValue);
            byte b25 = Int32Util.ClampToByte((int)Math.Min(lhs.B, rhs.B) - (int)Math.Max(lhs.B, rhs.B) + byte.MaxValue);
            byte a32 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b25, g25, r25, a32);

        case BlendModes.PinLight:
            byte r26 = 0;
            byte g26 = 0;
            byte b26 = 0;
            if (lhs.R < 2 * rhs.R - byte.MaxValue)
                r26 = Int32Util.ClampToByte(2 * rhs.R - byte.MaxValue);
            if (lhs.G < 2 * rhs.G - byte.MaxValue)
                g26 = Int32Util.ClampToByte(2 * rhs.G - byte.MaxValue);
            if (lhs.B < 2 * rhs.B - byte.MaxValue)
                b26 = Int32Util.ClampToByte(2 * rhs.B - byte.MaxValue);
            if (lhs.R > 2 * rhs.R - byte.MaxValue && lhs.R < 2 * rhs.R)
                r26 = lhs.R;
            if (lhs.G > 2 * rhs.G - byte.MaxValue && lhs.G < 2 * rhs.G)
                g26 = lhs.G;
            if (lhs.B > 2 * rhs.B - byte.MaxValue && lhs.B < 2 * rhs.B)
                b26 = lhs.B;
            if (lhs.R > 2 * rhs.R)
                r26 = Int32Util.ClampToByte(2 * rhs.R);
            if (lhs.G > 2 * rhs.G)
                g26 = Int32Util.ClampToByte(2 * rhs.G);
            if (lhs.B > 2 * rhs.B)
                b26 = Int32Util.ClampToByte(2 * rhs.B);
            byte a33 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b26, g26, r26, a33);

        case BlendModes.Red:
            byte a34 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(lhs.B, lhs.G, rhs.R, a34);

        case BlendModes.Reflect:
            byte r27 = rhs.R == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.R * lhs.R / (byte.MaxValue - rhs.R));
            byte g27 = rhs.G == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.G * lhs.G / (byte.MaxValue - rhs.G));
            byte b27 = rhs.B == byte.MaxValue ? byte.MaxValue : Int32Util.ClampToByte(lhs.B * lhs.B / (byte.MaxValue - rhs.B));
            byte a35 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b27, g27, r27, a35);

        case BlendModes.Saturation:
            HsvColor hsvColor7 = HsvColor.FromColor(lhs);
            HsvColor hsvColor8 = HsvColor.FromColor(rhs);
            Color color4 = new HsvColor()
            {
                Hue = hsvColor7.Hue,
                Saturation = hsvColor8.Saturation,
                Value = hsvColor7.Value
            }.ToColor();
            byte a36 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(color4.B, color4.G, color4.R, a36);

        case BlendModes.Screen:
            byte r28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.R) * (byte.MaxValue - rhs.R) >> 8));
            byte g28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.G) * (byte.MaxValue - rhs.G) >> 8));
            byte b28 = Int32Util.ClampToByte(byte.MaxValue - ((byte.MaxValue - lhs.B) * (byte.MaxValue - rhs.B) >> 8));
            byte a37 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b28, g28, r28, a37);

        case BlendModes.SignedDifference:
            byte r29 = Int32Util.ClampToByte((lhs.R - rhs.R) / 2 + 128);
            byte g29 = Int32Util.ClampToByte((lhs.G - rhs.G) / 2 + 128);
            byte b29 = Int32Util.ClampToByte((lhs.B - rhs.B) / 2 + 128);
            byte a38 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b29, g29, r29, a38);

        //case BlendModes.SoftBurn:
        //    byte a39 = normalOp.Apply(lhs, rhs).A;
        //    byte r30 = lhs.R + rhs.R >= byte.MaxValue ? (rhs.R != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.R)) / rhs.R) : (byte)0) : (lhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.R / (byte.MaxValue - lhs.R)) : (byte)0);
        //    byte g30 = lhs.G + rhs.G >= byte.MaxValue ? (rhs.G != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.G)) / rhs.G) : (byte)0) : (lhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.G / (byte.MaxValue - lhs.G)) : (byte)0);
        //    byte b30 = lhs.B + rhs.B >= byte.MaxValue ? (rhs.B != (byte)0 ? Int32Util.ClampToByte((1 - 128 * (byte.MaxValue - lhs.B)) / rhs.B) : (byte)0) : (lhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * rhs.B / (byte.MaxValue - lhs.B)) : (byte)0);
        //    return ColorBgra.FromBgra(b30, g30, r30, a39);

        //case BlendModes.SoftDodge:
        //    byte a40 = normalOp.Apply(lhs, rhs).A;
        //    byte r31 = lhs.R + rhs.R >= byte.MaxValue ? (lhs.R != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.R)) / lhs.R) : (byte)0) : (rhs.R != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.R / (byte.MaxValue - rhs.R)) : byte.MaxValue);
        //    byte g31 = lhs.G + rhs.G >= byte.MaxValue ? (lhs.G != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.G)) / lhs.G) : (byte)0) : (rhs.G != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.G / (byte.MaxValue - rhs.G)) : byte.MaxValue);
        //    byte b31 = lhs.B + rhs.B >= byte.MaxValue ? (lhs.B != (byte)0 ? Int32Util.ClampToByte((byte.MaxValue - 128 * (byte.MaxValue - rhs.B)) / lhs.B) : (byte)0) : (rhs.B != byte.MaxValue ? Int32Util.ClampToByte(128 * lhs.B / (byte.MaxValue - rhs.B)) : byte.MaxValue);
        //    return ColorBgra.FromBgra(b31, g31, r31, a40);

        case BlendModes.SoftLight:
            byte r32 = DoubleUtil.ClampToByte(rhs.R < 128 ? (float)(2 * ((lhs.R >> 1) + 64)) * ((float)rhs.R / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.R >> 1) + 64))) * (double)(byte.MaxValue - rhs.R) / byte.MaxValue));
            byte g32 = DoubleUtil.ClampToByte(rhs.G < 128 ? (float)(2 * ((lhs.G >> 1) + 64)) * ((float)rhs.G / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.G >> 1) + 64))) * (double)(byte.MaxValue - rhs.G) / byte.MaxValue));
            byte b32 = DoubleUtil.ClampToByte(rhs.B < 128 ? (float)(2 * ((lhs.B >> 1) + 64)) * ((float)rhs.B / (float)byte.MaxValue) : (float)(byte.MaxValue - (double)(2 * (byte.MaxValue - ((lhs.B >> 1) + 64))) * (double)(byte.MaxValue - rhs.B) / byte.MaxValue));
            byte a41 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b32, g32, r32, a41);

        case BlendModes.Stamp:
            byte r33 = Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue);
            byte g33 = Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue);
            byte b33 = Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue);
            byte a42 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b33, g33, r33, a42);

        case BlendModes.VividLight:
            byte a43 = normalOp.Apply(lhs, rhs).A;
            byte r34 = rhs.R <= (byte)128 ? Int32Util.ClampToByte(lhs.R + 2 * rhs.R - byte.MaxValue) : Int32Util.ClampToByte(lhs.R + 2 * (rhs.R - 128));
            byte g34 = rhs.G <= (byte)128 ? Int32Util.ClampToByte(lhs.G + 2 * rhs.G - byte.MaxValue) : Int32Util.ClampToByte(lhs.G + 2 * (rhs.G - 128));
            byte b34 = rhs.B <= (byte)128 ? Int32Util.ClampToByte(lhs.B + 2 * rhs.B - byte.MaxValue) : Int32Util.ClampToByte(lhs.B + 2 * (rhs.B - 128));
            return ColorBgra.FromBgra(b34, g34, r34, a43);

        case BlendModes.Yellow:
            byte a44 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(lhs.B, rhs.G, rhs.R, a44);

        default:
            return normalOp.Apply(lhs, rhs);
    }
}

public class TrackingProperty<T>
{
    private bool wasLastChangeEffective = true;

    private T stored;

    public T Value
    {
        get
        {
            return stored;
        }
        set
        {
            if (stored == null && value == null)
            {
                wasLastChangeEffective = false;
            }
            else if (stored != null && stored.Equals(value))
            {
                wasLastChangeEffective = false;
            }
            else
            {
                wasLastChangeEffective = true;
                stored = value;
            }
        }
    }

    public bool HasChanged
    {
        get
        {
            return wasLastChangeEffective;
        }
    }

    public void Set(T value)
    {
        Value = value;
    }

    // User-defined conversion from Digit to double
    public static implicit operator T(TrackingProperty<T> p)
    {
        return p.Value;
    }
}
Stopwatch processStopwatch = new Stopwatch();
Stopwatch methodStopwatch = new Stopwatch();

Surface working = null;
Surface working2 = null;
Surface working3 = null;
Surface working4 = null;
Rectangle selection = default(Rectangle);
int selectionCenterX;
int selectionCenterY;
ColorBgra primaryColor;
ColorBgra secondaryColor;
IReadOnlyList<ColorBgra> defaultColors;
IReadOnlyList<ColorBgra> currentColors;

void PreRender(Surface dst, Surface src)
{
    Debug.WriteLine("");
    Debug.WriteLine("");
    Debug.WriteLine("");
    Debug.WriteLine("");
    Debug.WriteLine("----------------------------------------------");
    Debug.WriteLine("----------------------------------------------");
    Debug.WriteLine("----------------------------------------------");
    Debug.WriteLine("----------------------------------------------");
    Debug.WriteLine("");
    Debug.WriteLine("");
    
    try
    {
        processStopwatch.Restart();

        selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

        selectionCenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
        selectionCenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

        primaryColor = (ColorBgra)EnvironmentParameters.PrimaryColor;
        secondaryColor = (ColorBgra)EnvironmentParameters.PrimaryColor;

        defaultColors = PaintDotNet.ServiceProviderExtensions.GetService<IPalettesService>(Services).DefaultPalette;
        currentColors = PaintDotNet.ServiceProviderExtensions.GetService<IPalettesService>(Services).CurrentPalette;

        if (working == null)
        {
            working = new Surface(src.Size);
        }
        if (working2 == null)
        {
            working2 = new Surface(src.Size);
        }
        if (working3 == null)
        {
            working3 = new Surface(src.Size);
        }
        if (working4 == null)
        {
            working4 = new Surface(src.Size);
        }

        PreRenderInternal(dst, src);

        preRenderException = false;
    }
    catch (Exception x)
    {
        preRenderException = true;
        Debug.WriteLine(x);
        return;
    }
    finally
    {
        processStopwatch.Stop();
        Debug.WriteLine("PreRender: " + processStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

bool preRenderException = false;

void Render(Surface dst, Surface src, Rectangle rect)
{
    if (preRenderException)
    {
        return;
    }

    try
    {
        processStopwatch.Restart();

        if (showOriginal)
        {
            dst.CopySurface(src, rect);
            return;
        }

        RenderInternal(dst, src, rect);
        PostRenderInternal(dst, src, rect);
    }
    catch (Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
    finally
    {
        processStopwatch.Stop();
        Debug.WriteLine("Render: " + processStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

protected override void OnDispose(bool disposing)
{
    if (disposing)
    {
        // Release any surfaces or effects you've created.
        if (working != null) working.Dispose();
        if (working2 != null) working2.Dispose();
        if (working3 != null) working3.Dispose();
        if (working4 != null) working4.Dispose();
        working = null;
        working2 = null;
        working3 = null;
        working4 = null;
    }

    base.OnDispose(disposing);
}

