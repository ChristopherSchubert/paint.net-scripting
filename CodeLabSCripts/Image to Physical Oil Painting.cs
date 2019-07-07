// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
CheckboxControl Amount1 = true; // [0,1] Retain Transparency
IntSliderControl Amount2 = 10; // [1,100] Blue Radius
IntSliderControl Amount3 = 20; // [1,100] Color Sample Efficiency
IntSliderControl Amount4 = 5; // [1,100] Color Equality
IntSliderControl Amount5 = 3; // [1,5] Color Palette Length
IntSliderControl Amount6 = 10; // [0,127] Low Color Limit
IntSliderControl Amount7 = 235; // [128,255] High Color Limit
CheckboxControl Amount8 = false; // [0,1] Show Palette
CheckboxControl Amount9 = false; // [0,1] Show Original
CheckboxControl Amount10 = false; // [0,1] Equalize Tone Grouping
CheckboxControl Amount11 = false; // [0,1] Reverse Tone Assignment
ListBoxControl Amount12 = 0; // Tone Priority|None | Darken | Light | Random
IntSliderControl Amount13 = 3; // [1,30] Oil Painting Brush Size
DoubleSliderControl Amount14 = 45; // [-180,180] Relief Angle 1
DoubleSliderControl Amount15 = 135; // [-180,180] Relief Angle 2
CheckboxControl Amount16 = false; // [0,1] Use Relief
CheckboxControl Amount17 = true; // [0,1] Make Seamless
IntSliderControl Amount18 = 96; // [0,256] Blend Start
IntSliderControl Amount19 = 96; // [0,256] Blend Distance
BinaryPixelOp Amount20 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Blend Type
CheckboxControl Amount21 = false; // [0,1] Fade Original Color Out
CheckboxControl Amount22 = true; // [0,1] Fade Swapped Color In
CheckboxControl Amount23 = false; // [0,1] Show Half Effect
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var tmpSurface1 = new Surface(src.Size);
    var tmpSurface2 = new Surface(src.Size);
    
    var retainTransparency = (bool)Amount1;
    var blurRadius = (int)Amount2;
    var colorSampleEfficiency = (int)Amount3;
    var colorEquality = (int)Amount4;
    var colorPaletteLength = (int)Amount5;
    var lowColorLimit = (int)Amount6;
    var highColorLimit = (int)Amount7;
    var showPalette = (bool)Amount8;
    var showOriginal = (bool)Amount9;
    var equalizeToneGrouping = (bool)Amount10;
    var reverseToneAssignment = (bool)Amount11;
    var tonePriority = (int)Amount12;
    var oilBrushSize = (int)Amount13;
    var reliefAngle1 = (double)Amount14;
    var reliefAngle2 = (double)Amount15;
    var useRelief = (bool)Amount16;
    
    var makeSeamless = (bool)Amount17;
    var blendStart = (int)Amount18;
    var blendDistance = (int)Amount19;
    var blendOp = (BinaryPixelOp)Amount20;
    var fadeOriginalColorOut = (bool)Amount21;
    var fadeSwappedColorIn = (bool)Amount22;
    var showHalfEffect = (bool)Amount23;
  
    if (showOriginal)
    {
        dst.CopySurface(src);
        return;
    }
    
    CreateSeamlessSurfaceFromOriginalSurface(dst, src, rect, blendStart, blendDistance, blendOp, fadeOriginalColorOut, fadeSwappedColorIn);
    if (blurRadius > 0)
    {
        GaussianBlur(tmpSurface1, src, rect, blurRadius);
    }
    else 
    {
        tmpSurface1.CopySurface(src);
    }

    var colorCounts = new Dictionary<ColorBgra, int>();
    var toneBandCounts = new Dictionary<int, int>();

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y += colorSampleEfficiency)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x += colorSampleEfficiency)
        {
            var color = src[x,y];
            color.A = 255;
            
            var toneBand = GetToneBand(src[x,y]);
            
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
            
            if (!toneBandCounts.ContainsKey(toneBand))
            {
                toneBandCounts.Add(toneBand, 1);
            }
            else
            {
                toneBandCounts[toneBand] += 1;
            }
        }
    }
    

    var colorList = new List<ColorBgra>();

    foreach(var color in colorCounts)
    {
        if (color.Value < colorEquality)
        {
            colorList.Add(color.Key);
            continue;
        }
        else
        {
            var newColorCount = color.Value / colorEquality;

            for(var i = 0; i < newColorCount; i++)
            {
                colorList.Add(color.Key);
            }
        }
    }
    
    var palette = GetAndTreatColorPalette(colorList, colorPaletteLength, tonePriority, reverseToneAssignment);
    var paletteColorAmouhnt = palette.Length;
    

    var width = rect.Right - rect.Left;
    
    var toneBandPaletteAssignments = GetToneBandPaletteAssignments(equalizeToneGrouping, paletteColorAmouhnt, toneBandCounts);
      
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;

        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (showPalette)
            {
                var index = (x / (width/palette.Length));

                if (index < 0) { index = 0; }
                if (index >= palette.Length) { index = palette.Length-1; }

                dst[x,y] = palette[index];
            }
            else 
            {
                var toneBand = GetToneBand(tmpSurface1[x,y]);
                
                if (toneBand < 0)
                {
                    toneBand = 0;
                }
                else if (toneBand >= toneBandPaletteAssignments.Length)
                {
                    toneBand = toneBandPaletteAssignments.Length-1;
                }
                
                var paletteIndex = toneBandPaletteAssignments[toneBand];
                
                tmpSurface2[x,y] = palette[paletteIndex];
            }
        }
    }
    
    
    if (showPalette)
    {
        return;
    }
    
    if (IsCancelRequested) return;
    
    OilPainting(tmpSurface1, tmpSurface2, rect, oilBrushSize, 50);
    
    if (IsCancelRequested) return;
    
    if (useRelief)
    {    
    
        if (IsCancelRequested) return;
    
        Relief(tmpSurface2, tmpSurface1, rect, reliefAngle1);
        
        if (IsCancelRequested) return;
        
        Relief(tmpSurface1, tmpSurface2, rect, reliefAngle2);

        FixReliefBorders(tmpSurface1, rect);
    }
    
    if (IsCancelRequested) return;
    
    if (makeSeamless)
    {
        CreateSeamlessSurfaceFromOriginalSurface(tmpSurface2, tmpSurface1, rect, blendStart, blendDistance, blendOp, fadeOriginalColorOut, fadeSwappedColorIn);
    }
    else
    {
        tmpSurface2.CopySurface(tmpSurface1);
    }
    
    
    if (retainTransparency)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int x = rect.Left; x < rect.Right; x++)
            {
                dst[x,y] = ColorBgra.FromBgra(
                tmpSurface2[x,y].B, 
                tmpSurface2[x,y].G, 
                tmpSurface2[x,y].R, src[x,y].A);
            }
        }
    }
    else 
    {
        dst.CopySurface(tmpSurface2);
    }
    
    if (showHalfEffect)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int x = width/2; x < rect.Right; x++)
            {
                dst[x,y] = src[x,y];
            }
        }
    }
}

ColorBgra Spread(ColorBgra[] colors)
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


public Tuple<ColorBgra[], ColorBgra[]> Partition(ColorBgra[] colors)
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

    public ColorBgra Average(ColorBgra[] colors)
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

    ColorBgra[] GetColorPalette(ColorBgra[] colors, int iterations)
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
    
  int GetToneBand(ColorBgra color)
{
    return (color.R + color.G + color.B) / 3;
}

int GetToneBandIndex(int band, int amountOfBands)
{
    var index = band / amountOfBands;
    
    if (index < 0)
    {
        index = 0;
    }
    else if (index >= amountOfBands)
    {
        index = amountOfBands-1;
    }

    return index;
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

public ColorBgra[] GetAndTreatColorPalette(List<ColorBgra> colorList, int colorPaletteLength, int tonePriority, bool reverseToneAssignment)
{
    var palette = GetColorPalette(colorList.ToArray(), colorPaletteLength);
    var tempPalette = palette.ToList();
    
    switch(tonePriority)
    {
        case 0:
        
        if (!reverseToneAssignment)
        {
            tempPalette.Reverse();
        }
        
        break;
        
        case 1:
        tempPalette.Sort((x,y) => 
        {
            var thisColor = HsvColor.FromColor(x.ToColor());
            var thatColor = HsvColor.FromColor(y.ToColor());
            
            return (thisColor.Saturation*thisColor.Value).CompareTo(thatColor.Saturation*thatColor.Value);
        });
        
        if (!reverseToneAssignment)
        {
            tempPalette.Reverse();
        }
        
        break;
        
        case 2: 
        tempPalette.Sort((x,y) => 
        {
            var thisColor = HsvColor.FromColor(x.ToColor());
            var thatColor = HsvColor.FromColor(y.ToColor());
            
            return (thatColor.Saturation*thatColor.Value).CompareTo(thisColor.Saturation*thisColor.Value);
        });
        
        if (!reverseToneAssignment)
        {
            tempPalette.Reverse();
        }
        
        break;
        
        case 3: 
        Shuffle(tempPalette);
        
        break;
    }
    
    palette = tempPalette.ToArray();
    
    return palette;
}

private int[] GetToneBandPaletteAssignments(bool equalizeToneGrouping, int paletteLength, Dictionary<int, int> toneBandCounts)
{
    var toneBandPaletteAssignments = new int[255];
    
    Debug.WriteLine(paletteLength);
    
    var bandSize = (255/paletteLength);
    
    if (!equalizeToneGrouping)
    {
        for (var i = 0; i < 255; i++)
        {
            var result = Convert.ToInt32(i / bandSize);
            
            if (result < 0)
            {
                result = 0;
            }
            else if (result >= paletteLength)
            {
                result = paletteLength - 1;
            }
            
            
            toneBandPaletteAssignments[i] = result;
        }
        
        foreach(var x in toneBandPaletteAssignments)
        {
            Debug.WriteLine(x);
        }
        return toneBandPaletteAssignments;
    }
    
    var sortedToneBands = toneBandCounts.ToList();
    sortedToneBands.Sort((x,y) => x.Key.CompareTo(y.Key));
    
    var idealToneBandSize = sortedToneBands.Sum(stb => stb.Value) / paletteLength;
    
    var currentPaletteIndex = 0;
    var currentToneBandSize = 0;
    
    for(var i = 0; i < sortedToneBands.Count; i++)
    {
        if(currentToneBandSize > idealToneBandSize)
        {
            currentPaletteIndex += 1;
            currentToneBandSize = 0;
        }
        
        var band = sortedToneBands[i];
        
        toneBandPaletteAssignments[band.Key] = currentPaletteIndex;
        currentToneBandSize += sortedToneBands[i].Value;
    }
    
    var currentReplacement = 0;
    
    for(var i = 0; i < toneBandPaletteAssignments.Length; i++)
    {
        if (toneBandPaletteAssignments[i] > currentReplacement)
        {
            currentReplacement = toneBandPaletteAssignments[i];
        }
        else if (toneBandPaletteAssignments[i] == 0)        
        {
            toneBandPaletteAssignments[i] = currentReplacement;
        }
    }
    
    return toneBandPaletteAssignments;
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

// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode

#endregion

void CreateSeamlessSurfaceFromOriginalSurface(Surface dst, Surface src, Rectangle rect, int blendStart, int blendDistance, BinaryPixelOp blendOp, bool fadeOriginalColorOut, bool fadeSwappedColorIn)
{
    ColorBgra CurrentPixel;
    var working = new Surface(dst.Size);
    
    var width = rect.Right - rect.Left;
    var height = rect.Bottom - rect.Top;
    
  
    var splitPointX = width/2;
    var splitPointY = height/2;
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
            
            var targetX = 0;
            var targetY = 0;
            
            if (x >= splitPointX)
            {
                targetX = x - splitPointX;
            }
            else 
            {
                targetX = x + splitPointX;
            }
            
            if (y >= splitPointY)
            {
                targetY = y - splitPointY;
            }
            else 
            {
                targetY = y + splitPointY;
            }
            
            ColorBgra originalColor = src[x, y];
            ColorBgra swappedColor = src[targetX, targetY];
                
            if (targetX < blendStart || targetY < blendStart || targetX > (width-blendStart) || targetY > (height-blendStart))
            {
                working[targetX, targetY] = originalColor;
                continue;
            }
            
           if (!(targetX <= blendStart + blendDistance || 
                targetY <= blendStart + blendDistance ||
                targetX > (width-blendStart-blendDistance) || 
                targetY > (height-blendStart-blendDistance)))
           {
                working[targetX, targetY] = swappedColor;
                continue;
           }
            
            ColorBgra blendColor = Color.Magenta;
            
            var inTop = (targetY >= blendStart && targetY <= blendStart + blendDistance);
            var inBottom = (targetY <= height - blendStart && targetY > height - blendStart - blendDistance);
            
            var inLeft = (targetX >= blendStart && targetX <= blendStart + blendDistance);
            var inRight = (targetX <= width - blendStart && targetX > width - blendStart - blendDistance);
            
            byte originalAlpha = byte.MaxValue;
            byte swappedAlpha= byte.MaxValue;
            
            if (inTop && inLeft && !inRight)
            {
                float yDistanceNumer = (targetY - blendStart);
                float yDistanceDenom = (blendDistance);
                float xDistanceNumer = (targetX - blendStart);
                float xDistanceDenom = (blendDistance);
                
                var ySwappedAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                var yOriginalAlpha = (byte)(255 - ySwappedAlpha);
                var xSwappedAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                var xOriginalAlpha = (byte)(255 - xSwappedAlpha);
                
                swappedAlpha = Math.Min(ySwappedAlpha, xSwappedAlpha);
                originalAlpha = Math.Max(yOriginalAlpha, xOriginalAlpha);
            }
            
            if (inTop && !inLeft && !inRight)
            {
                float yDistanceNumer = (targetY - blendStart);
                float yDistanceDenom = (blendDistance);
                swappedAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                originalAlpha = (byte)(255 - swappedAlpha);
            }
            
            if (inTop && !inLeft && inRight)
            {
                float yDistanceNumer = (targetY - blendStart);
                float yDistanceDenom = (blendDistance);
                float xDistanceNumer = (targetX - (width - blendDistance - blendStart));
                float xDistanceDenom = ((width - blendStart) - (width - blendDistance - blendStart));
                
                var ySwappedAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                var yOriginalAlpha = (byte)(255 - ySwappedAlpha);
                var xOriginalAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                var xSwappedAlpha = (byte)(255 - xOriginalAlpha);
                
                swappedAlpha = Math.Min(ySwappedAlpha, xSwappedAlpha);
                originalAlpha = Math.Max(yOriginalAlpha, xOriginalAlpha);
            }
            
            if (inLeft && !inTop && !inBottom)
            {
                float xDistanceNumer = (targetX - blendStart);
                float xDistanceDenom = (blendDistance);
                swappedAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                originalAlpha = (byte)(255 - swappedAlpha);
            }
            
            if (inRight && !inTop && !inBottom)
            {
                float xDistanceNumer = (targetX - (width - blendDistance - blendStart));
                float xDistanceDenom = ((width - blendStart) - (width - blendDistance - blendStart));
                originalAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                swappedAlpha = (byte)(255 - originalAlpha);
            }
            
            if (inBottom && inLeft && !inRight)
            {
                float yDistanceNumer = (targetY - (height - blendDistance - blendStart));
                float yDistanceDenom = ((height - blendStart) - (height - blendDistance - blendStart));
                float xDistanceNumer = (targetX - blendStart);
                float xDistanceDenom = (blendDistance);
                
                var yOriginalAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                var ySwappedAlpha = (byte)(255 - yOriginalAlpha);
                var xSwappedAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                var xOriginalAlpha = (byte)(255 - xSwappedAlpha);
                
                swappedAlpha = Math.Min(ySwappedAlpha, xSwappedAlpha);
                originalAlpha = Math.Max(yOriginalAlpha, xOriginalAlpha);
            }
            
            if (inBottom && !inLeft && !inRight)
            {
                float yDistanceNumer = (targetY - (height - blendDistance - blendStart));
                float yDistanceDenom = ((height - blendStart) - (height - blendDistance - blendStart));
                originalAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                swappedAlpha = (byte)(255 - originalAlpha);
            }
            
            if (inBottom && !inLeft && inRight)
            {
                float yDistanceNumer = (targetY - (height - blendDistance - blendStart));
                float yDistanceDenom = ((height - blendStart) - (height - blendDistance - blendStart));
                float xDistanceNumer = (targetX - (width - blendDistance - blendStart));
                float xDistanceDenom = ((width - blendStart) - (width - blendDistance - blendStart));
                
                var yOriginalAlpha = Convert.ToByte(255 * (yDistanceNumer/yDistanceDenom));
                var ySwappedAlpha = (byte)(255 - yOriginalAlpha);
                var xOriginalAlpha = Convert.ToByte(255 * (xDistanceNumer/xDistanceDenom));
                var xSwappedAlpha = (byte)(255 - xOriginalAlpha);
                
                swappedAlpha = Math.Min(ySwappedAlpha, xSwappedAlpha);
                originalAlpha = Math.Max(yOriginalAlpha, xOriginalAlpha);
            }
            
            if (!fadeSwappedColorIn)
            {
                originalAlpha = byte.MaxValue;
            }
            if (!fadeOriginalColorOut)
            {
                swappedAlpha = byte.MaxValue;
            }
            
            originalColor.A = originalAlpha;
            swappedColor.A = swappedAlpha;
                
            working[targetX, targetY] = blendOp.Apply(swappedColor, originalColor);
            continue;
        }
    }
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
      
        for (int x = rect.Left; x < rect.Right; x++)
        {            
            if (IsCancelRequested) return;
            
            dst[x,y] = working[x,y];
        }
    }
}
