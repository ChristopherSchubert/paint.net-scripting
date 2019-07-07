// Name:Intelligent Palette Oil Painting
// Submenu:Chris
// Author:
// Title:Intelligent Palette Oil Painting
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
CheckboxControl Amount1 = false; // [0,1] Retain Transparency
CheckboxControl Amount2 = true; // [0,1] {!Amount1} Use Original Transparency As Guideline
IntSliderControl Amount3 = 127; // [1,255] {Amount2} Reject Alpha Below
IntSliderControl Amount4 = 3; // [1,100] Blur Radius
IntSliderControl Amount5 = 100; // [0,200] Saturation Adjustment
IntSliderControl Amount6 = 0; // [-50,50] Contrast Adjustment
IntSliderControl Amount7 = 3; // [1,100] Color Sample Efficiency
IntSliderControl Amount8 = 5; // [1,100] Color Frequency Equalization
IntSliderControl Amount9 = 10; // [0,100,5] Low Color Limit
IntSliderControl Amount10 = 245; // [128,255,5] High Color Limit
IntSliderControl Amount11 = 3; // [1,5] Color Palette Length
IntSliderControl Amount12 = 20; // [2,20] Color Palette Hue Segmentation
CheckboxControl Amount13 = false; // [0,1] Show Palette
CheckboxControl Amount14 = false; // [0,1] Show Original
CheckboxControl Amount15 = false; // [0,1] Show Half Effect
CheckboxControl Amount16 = true; // [0,1] Do Post Processing
IntSliderControl Amount17 = 4; // [1,7] {Amount16} Post Processing Segments
CheckboxControl Amount18 = true; // [0,1] {Amount16} Post Processing Dark Is Lower
CheckboxControl Amount19 = true; // [0,1] {Amount16} Do Post Processing Oil Painting
CheckboxControl Amount20 = false; // [0,1] {Amount19} Process Oil Painting In Segments
IntSliderControl Amount21 = 3; // [1,8] {Amount19} Post Processing Oil Painting Brush Width
CheckboxControl Amount22 = true; // [0,1] {Amount16} Do Post Processing Relief
CheckboxControl Amount23 = false; // [0,1] {Amount16} Override Relief Darken Blend
BinaryPixelOp Amount24 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Darken); // {Amount23} Override Relief Blend Mode
IntSliderControl Amount25 = 3; // [1,7] {Amount22} Maxmimum Relief Applications
IntSliderControl Amount26 = 45; // [-180,180] {Amount22} Post Processing Relief Angle Start
IntSliderControl Amount27 = 90; // [-180,180] {Amount22} Post Processing Relief Angle End
IntSliderControl Amount28 = 1; // [0,3] {Amount16} Final Gloss
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var workSurface1 = new Surface(src.Size);
    var workSurface2 = new Surface(src.Size);
    var workSurface3 = new Surface(src.Size);
        
    var retainTransparency = (bool)Amount1;
    var useOriginalTransparencyAsGuideline = (bool)Amount2;
    var rejectAlphaBelow = (int)Amount3;
    var blurRadius = (int)Amount4;
    var colorSampleEfficiency = (int)Amount7;
    var colorFrequencyEqualization = (int)Amount8;
    var lowColorLimit = (int)Amount9;
    var highColorLimit = (int)Amount10;
    var colorPaletteLength = (int)Amount11;
    var colorPaletteHueSegmentation = (int)Amount12;
    var showPalette = (bool)Amount13;
    var showOriginal = (bool)Amount14;
    var showHalf = (bool)Amount15;
    var postProcessingSegments = (int)Amount17;
    var darkIsLower = (bool)Amount18;
    var doPostProcessing = (bool)Amount16;

    var doOilPainting = (bool)Amount19;
    var doOilPaintingInSegments = (bool)Amount20;
    var oilBrushWidth = (int)Amount21;
    var doRelief = (bool)Amount22;
    var doOverrideReliefBlendMode = (bool)Amount23;
    var reliefBlendMode = doOverrideReliefBlendMode ? Amount24 : new UserBlendOps.DarkenBlendOp();
    var maximumReliefApplications = (int)Amount25;
    var reliefAngleStart = (int)Amount26;
    var reliefAngleEnd = (int)Amount27;
    var finalGloss = (int)Amount28;
   
    var saturationAdjustment = (int)Amount5;
    var contrastAdjustment = (int)Amount6;

    if (showOriginal)
    {
        dst.CopySurface(src);
        return;
    }
    
    if (IsCancelRequested) return;
    
    workSurface1.CopySurface(src);
    
    if (!retainTransparency && useOriginalTransparencyAsGuideline)
    {
        for(var y = 0; y < workSurface1.Height; y++)
        {
            for(var x = 0; x < workSurface1.Width; x++)
            {
                if (workSurface1[x,y].A < rejectAlphaBelow)
                {
                    workSurface1[x,y] = ColorBgra.TransparentBlack;
                }
            }
        }   
    }

    if (blurRadius > 0)
    {
        GaussianBlur(workSurface2, workSurface1, rect, blurRadius);
        ApplyAlphaThresholdToSurface(workSurface2,0,0);
        workSurface1.CopySurface(workSurface2);
    }
    
    if (saturationAdjustment != 0)
    {
        HueAndSaturation(workSurface2, workSurface1, rect, 0, saturationAdjustment);
        workSurface1.CopySurface(workSurface2);
    }
    
    if (contrastAdjustment != 0)
    {
        BrightnessAndContrast(workSurface2, workSurface1, rect, 0, contrastAdjustment);
        workSurface1.CopySurface(workSurface2);
    }
 
    if (IsCancelRequested) return;
    
    var palette = new IntelligentColorPalette();
    palette.InitializePalette(workSurface1, colorSampleEfficiency, colorFrequencyEqualization,
    lowColorLimit, highColorLimit, colorPaletteLength, colorPaletteHueSegmentation);

    if (IsCancelRequested) return;
    
    if (showPalette)
    {
        if (showHalf)
        {
            dst.CopySurface(src);
        }

        palette.CopyPaletteToSurface(dst, showHalf);
        return;
    }
    
    if (IsCancelRequested) return;
    
    palette.ReplaceSurfaceColorsWithPalette(workSurface2, workSurface1);
    
    if (doPostProcessing)
    {
        if (doOilPainting && !doOilPaintingInSegments)
        {   
            OilPainting(workSurface3, workSurface2, rect, oilBrushWidth, 50);
            workSurface2.CopySurface(workSurface3);
        }
            
        workSurface1 = new Surface(dst.Size);
        
        for(var postProcessingSegmentIndex = 0; postProcessingSegmentIndex < postProcessingSegments; postProcessingSegmentIndex++)
        {
            if (IsCancelRequested) return;
            
            var toneBandIdentifier = postProcessingSegmentIndex;
            if (!darkIsLower)
            {
                toneBandIdentifier = (postProcessingSegments-1)-postProcessingSegmentIndex;
            }
            
            var segmentLowToneBand = toneBandIdentifier * (255f / postProcessingSegments);
            var segmentHighToneBand = (1+toneBandIdentifier) * (255f / postProcessingSegments);
            
            Debug.WriteLine(segmentLowToneBand.ToString() + " to " + segmentHighToneBand.ToString());
                    
            var toneBandSurface = GetSurfaceFromToneBand(workSurface2, Convert.ToInt32(segmentLowToneBand), Convert.ToInt32(segmentHighToneBand));
            
            if (IsCancelRequested) return;
            
            if (doOilPainting && doOilPaintingInSegments)
            {
                OilPainting(workSurface3, toneBandSurface, rect, oilBrushWidth, 50);
                toneBandSurface.CopySurface(workSurface3);
            }
            
            if (IsCancelRequested) return;
            
            if (doRelief && postProcessingSegments - 1 - postProcessingSegmentIndex <= maximumReliefApplications)
            {
                var angle = reliefAngleStart + (((postProcessingSegmentIndex+1f)/(float)postProcessingSegments)*(reliefAngleEnd - reliefAngleStart));
                
                Relief(workSurface3, toneBandSurface, rect, (double)angle);
                BlackToTransparent(workSurface3);
                ApplySourceToDestination(toneBandSurface, workSurface3, reliefBlendMode, false,false,false);
                //toneBandSurface.CopySurface(workSurface3);
                FixReliefBorders(toneBandSurface, rect);
            }
            
            ApplySourceToDestination(workSurface1, toneBandSurface, new UserBlendOps.NormalBlendOp(), false,false,false);
        }
        
        GaussianBlur(workSurface2, workSurface1, rect, finalGloss);
        workSurface1.CopySurface(workSurface2);
    }
    else
    {
        workSurface1.CopySurface(workSurface2);
    }
    
    if (IsCancelRequested) return;
    
    dst.CopySurface(workSurface1);
    
    if (retainTransparency)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int x = rect.Left; x < rect.Right; x++)
            {
                dst[x,y] = ColorBgra.FromBgra(
                dst[x,y].B, 
                dst[x,y].G, 
                dst[x,y].R, 
                src[x,y].A);
            }
        }
    }  
    
    if (IsCancelRequested) return;

    if (showHalf)
    {
        for (int y = 0; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int x = (rect.Right-rect.Left)/2; x < rect.Right; x++)
            {
                dst[x,y] = src[x,y];
            }
        }
    }    
}

    
    
void GaussianBlur(Surface dst, Surface src, Rectangle rect, int radius)
{
    GaussianBlurEffect effect = new GaussianBlurEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, radius);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

void Relief(Surface dst, Surface src, Rectangle rect, double angle)
{ 
    ReliefEffect effect = new ReliefEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(ReliefEffect.PropertyNames.Angle, angle);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

void OilPainting(Surface dst, Surface src, Rectangle rect, int brushSize, int coarseness)
{
    OilPaintingEffect effect = new OilPaintingEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(OilPaintingEffect.PropertyNames.BrushSize, brushSize); 
    parameters.SetPropertyValue(OilPaintingEffect.PropertyNames.Coarseness, coarseness);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

void BrightnessAndContrast(Surface dst, Surface src, Rectangle rect, int brightness, int contrast)
{
    BrightnessAndContrastAdjustment effect = new BrightnessAndContrastAdjustment();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, brightness); 
    parameters.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, contrast);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

void HueAndSaturation(Surface dst, Surface src, Rectangle rect, int hue, int saturation)
{
    HueAndSaturationAdjustment effect = new HueAndSaturationAdjustment();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Hue, hue); 
    parameters.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Saturation, saturation);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

private static Random rng = new Random();  

public static void Shuffle<T>(IList<T> list)  
{  
    int n = list.Count;  
    while (n > 1) {  
        n--;  
        int k = rng.Next(n + 1);  
        T value = list[k];  
        list[k] = list[n];  
        list[n] = value;  
    }  
}

private void FixReliefBorders(Surface dst, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        var farLeft = rect.Left;
        var farRight = rect.Right-1;
        
        dst[farLeft,y] = dst[farLeft+2,y];
        dst[farLeft+1,y] = dst[farLeft+2,y];
        
        dst[farRight,y] = dst[farRight-2,y];
        dst[farRight-1,y] = dst[farRight-2,y];
    }
    
    for (int x = rect.Left; x < rect.Right; x++)
    {
        if (IsCancelRequested) return;
        
        var farTop = rect.Top;
        var farBottom = rect.Bottom-1;
        
        dst[x,farTop] = dst[x,farTop+2];
        dst[x,farTop+1] = dst[x,farTop+2];
        
        dst[x,farBottom] = dst[x,farBottom-2];
        dst[x,farBottom-1] = dst[x,farBottom-2];
    }
}

private void BlackToTransparent(Surface source)
{
    for(var y = 0; y < source.Height; y++)
    {
        for(var x = 0; x < source.Width; x++)
        {
            if (source[x,y] == ColorBgra.Black)
            {
                source[x,y] = ColorBgra.TransparentBlack;
            }
        }
    }
}
private void ApplyAlphaThresholdToSurface(Surface source, int thresholdBelowBecomes0, int thresholdAboveBecomes255)
{    
    for(var y = 0; y < source.Height; y++)
    {
        for(var x = 0; x < source.Width; x++)
        {
            if (IsCancelRequested)
            {
                return;
            }
            
            if (source[x,y].A < thresholdBelowBecomes0)
            {
                source[x,y] = ColorBgra.FromBgra(
                source[x,y].B,
                source[x,y].G,
                source[x,y].R,
                byte.MinValue);
            }
            else if (source[x,y].A > thresholdAboveBecomes255)
            {
                source[x,y] = ColorBgra.FromBgra(
                source[x,y].B,
                source[x,y].G,
                source[x,y].R,
                byte.MaxValue);
            }
            
        }
    }
}

private Surface GetSurfaceFromToneBand(Surface source, int toneStart, int toneEnd)
{
    var surface = new Surface(source.Size);
    
     for(var y = 0; y < source.Height; y++)
    {
        for(var x = 0; x < source.Width; x++)
        {
            if (IsCancelRequested)
            {
                return surface;
            }
            
            var tone = ((source[x,y].R + source[x,y].R + source[x,y].R)/3);
            if (tone >= toneStart && tone <= toneEnd)
            {
                surface[x,y] = source[x,y];
            }
            else 
            {
                surface[x,y] = ColorBgra.TransparentBlack;
            }
        }
    }
    
    return surface;
}

private void ApplySourceToDestination(Surface destination, Surface source, BinaryPixelOp pixelOp, bool allowBlack, bool allowWhite, bool allowTransparent)
{
    for(var y = 0; y < destination.Height; y++)
    {
        for(var x = 0; x < destination.Width; x++)
        {
            if (IsCancelRequested)
            {
                return;
            }
            
            if (!allowBlack && source[x,y] == ColorBgra.Black) continue;
            if (!allowWhite && source[x,y] == ColorBgra.White) continue;
            if (!allowTransparent && source[x,y].A == 0) continue;
                        
            destination[x,y] = pixelOp.Apply(destination[x,y], source[x,y]);
        }
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
                var color = surface[x,y];
                color.A = 255;
                
                var toneBand = ((surface[x,y].R + surface[x,y].G + surface[x,y].B) / 3);
                
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
        
        for(var i = 0; i < toneBandCount; i++)
        {
            prePaletteColorsByToneBand[i] = new List<ColorBgra>();
        }
        
        foreach(var colorCount in colorCounts)
        {
            var band = (colorCount.Key.R + colorCount.Key.G + colorCount.Key.B) / 3f;
            var normalized = band / 255f;
            var final = normalized * toneBandCount;
            var bandIndex = (int)(final);
                        
            if (bandIndex == toneBandCount)
            {
                bandIndex -=1;
            }
            
            if (colorCount.Value < colorFrequencyEqualizer)
            {
                prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                continue;
            }
            else
            {
                var newColorCount = colorCount.Value / colorFrequencyEqualizer;

                for(var i = 0; i < newColorCount; i++)
                {
                    prePaletteColorsByToneBand[bandIndex].Add(colorCount.Key);
                }
            }
        }
        
        var toneBandPalettes = new ColorBgra[toneBandCount][];
        
        for(var toneBandIndex = 0; toneBandIndex < toneBandPalettes.Length; toneBandIndex++)
        {
            toneBandPalettes[toneBandIndex] = GetColorPalette(prePaletteColorsByToneBand[toneBandIndex].ToArray(), colorPaletteLength);
        }
        
        colorPaletteByHueSegment = new ColorBgra[colorPaletteHueSegmentation][];
        
        for(var i = 0; i < colorPaletteByHueSegment.Length; i++)
        {
            colorPaletteByHueSegment[i] = new ColorBgra[toneBandCount];
        }
        
        for(var hueIndex = 0; hueIndex < colorPaletteByHueSegment.Length; hueIndex++)
        {            
            for(var toneBandIndex = 0; toneBandIndex < toneBandCount; toneBandIndex++)
            {
                foreach(var color in toneBandPalettes[toneBandIndex])
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

        var width = useHalf ? surface.Width/2 : surface.Width;
        var height = surface.Height;

        for(var x = 0; x < width; x++)
        {
            for(var y = 0; y < height; y++)
            {
                var xPercentage = (float)x / (float)width;
                var yPercentage = (float)y / (float)height;

                var hueIndex = (int)(yPercentage * colorPaletteByHueSegment.Length);
                if (hueIndex == colorPaletteByHueSegment.Length) hueIndex -= 1;
                
                var colorIndex = (int)(xPercentage * colorPaletteByHueSegment[hueIndex].Length);
                if (colorIndex == colorPaletteByHueSegment[hueIndex].Length) colorIndex -= 1;

                surface[x,y] = colorPaletteByHueSegment[hueIndex][colorIndex];
            }
        }
    }

    public void ReplaceSurfaceColorsWithPalette(Surface destination, Surface source)
    { 
        if (colorPaletteByHueSegment == null)
        {
            return;
        }

        for(var x = 0; x < source.Width; x++)
        {
            for(var y = 0; y < source.Height; y++)
            {
                if (source[x,y].A == 0) continue;
                
                var nearestPaletteColor = GetNearestPaletteColor(source[x,y]);
                
                if (nearestPaletteColor == ColorBgra.TransparentBlack)
                {
                    ForceSetColorAsPaletteColor(source[x,y]);
                    nearestPaletteColor = source[x,y];
                }
                
                destination[x,y] = nearestPaletteColor;
            }
        }
    }

    public ColorBgra GetNearestPaletteColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);
        
        var toneBand = (color.R + color.G + color.B)/3f;

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
        
        var toneBand = (color.R + color.G + color.B)/3f;

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

        for(var i = 0; i < numberOfHueSegments; i++)
        {
            if (color.Hue > segmentSize*i && color.Hue <= segmentSize*(i+1))
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

        foreach(var color in colors)
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

        colorList.Sort((x,y) => {
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

        foreach(var color in colors)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
        }

        return new ColorBgra(){
            R = (byte)(sumR/colors.Length),
            G = (byte)(sumG/colors.Length),
            B = (byte)(sumB/colors.Length),
            A = 255
        };
    }

    private ColorBgra[] GetColorPalette(ColorBgra[] colors, int iterations)
    {
        var firstPartitions = Partition(colors);
        var partitions = new List<ColorBgra[]>();

        partitions.Add(firstPartitions.Item1);
        partitions.Add(firstPartitions.Item2);

        for(var i = 0; i < iterations; i++)
        {
            var results = new List<ColorBgra[]>();
            foreach(var partition in partitions)
            {
                var result = Partition(partition);
                results.Add(result.Item1);
                results.Add(result.Item2);
            }

            partitions = results;
        }

        var palette = new ColorBgra[partitions.Count];

        for(var i = 0; i < palette.Length; i++)
        {
            palette[i] = Average(partitions[i]);
        }

        return palette;
    }
}
