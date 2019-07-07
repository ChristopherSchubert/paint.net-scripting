// Name:Intelligent Palette
// Submenu:Chris
// Author:
// Title:Intelligent Palette
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode

CheckboxControl Amount1 = false; // Show Original
CheckboxControl Amount2 = false; // Show Half
IntSliderControl Amount3 = 3; // [1,100] Color Sample Efficiency
IntSliderControl Amount4 = 5; // [1,100] Color Frequency Equalization
IntSliderControl Amount5 = 10; // [0,100,5] Low Color Limit
IntSliderControl Amount6 = 235; // [128,255,5] High Color Limit
IntSliderControl Amount7 = 3; // [1,5] Color Palette Length
IntSliderControl Amount8 = 20; // [2,20] Color Palette Hue Segmentation

#endregion

void PreRenderInternal(Surface dst, Surface src)
{
    showOriginal = Amount1;
    showHalf = Amount2;
}

bool showOriginal;
bool showHalf;

void RenderInternal(Surface dst, Surface src, Rectangle rect)
{
    var colorSampleEfficiency = (int)Amount3;
    var colorFrequencyEqualization = (int)Amount4;
    var lowColorLimit = (int)Amount5;
    var highColorLimit = (int)Amount6;
    var colorPaletteLength = (int)Amount7;
    var colorPaletteHueSegmentation = (int)Amount8;
  
    var palette = new IntelligentColorPalette();
    palette.InitializePalette(src, colorSampleEfficiency, colorFrequencyEqualization, lowColorLimit, highColorLimit, colorPaletteLength, colorPaletteHueSegmentation);

    if (IsCancelRequested) return;
    
    palette.CopyPaletteToSurface(working, showHalf);
    return;
}

void PostRenderInternal(Surface dst, Surface src, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (showHalf && x > dst.Width/2)
            {
                dst[x,y] = src[x,y];
            }
            else 
            {
                dst[x,y] = working[x,y];
            }
        }
    }
}


Surface working = null;
Rectangle selection = default(Rectangle);
int selectionCenterX;
int selectionCenterY;
ColorBgra primaryColor;
ColorBgra secondaryColor;
IReadOnlyList<ColorBgra> defaultColors;
IReadOnlyList<ColorBgra> currentColors;

void PreRender(Surface dst, Surface src)
{
    try
    {
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
        
        PreRenderInternal(dst,src);
    } 
    catch(Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    try
    {
        if (showOriginal)
        {
            dst.CopySurface(src, rect);
            return;
        }
        
        RenderInternal(dst,src,rect);
        PostRenderInternal(dst,src,rect);
    } 
    catch(Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
}


protected override void OnDispose(bool disposing)
{
    if (disposing)
    {
        // Release any surfaces or effects you've created.
        if (working != null) working.Dispose();
        working = null;
    }

    base.OnDispose(disposing);
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