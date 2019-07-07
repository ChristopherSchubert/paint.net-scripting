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
IntSliderControl Amount1 = 10; // [1,100] Blue Radius
IntSliderControl Amount2 = 20; // [1,100] Color Sample Efficiency
IntSliderControl Amount3 = 5; // [1,100] Color Equality
IntSliderControl Amount4 = 3; // [1,5] Color Palette Length
CheckboxControl Amount5 = false; // [0,1] Show Palette
CheckboxControl Amount6 = false; // [0,1] Show Original
CheckboxControl Amount7 = false; // [0,1] Equalize Tone Grouping
CheckboxControl Amount8 = false; // [0,1] Reverse Tone Assignment
ListBoxControl Amount9 = 0; // Tone Priority|None | Darken | Light | Random
IntSliderControl Amount10 = 3; // [1,30] Oil Painting Brush Size
DoubleSliderControl Amount11 = 45; // [-180,180] Relief Angle 1
DoubleSliderControl Amount12 = 135; // [-180,180] Relief Angle 2
CheckboxControl Amount13 = true; // [0,1] Use Relief
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var tmpSurface1 = new Surface(src.Size);
    var tmpSurface2 = new Surface(src.Size);
    
    var blurRadius = (int)Amount1;
    var colorSampleEfficiency = (int)Amount2;
    var colorEquality = (int)Amount3;
    var colorPaletteLength = (int)Amount4;
    var showPalette = (bool)Amount5;
    var showOriginal = (bool)Amount6;
    var equalizeToneGrouping = (bool)Amount7;
    var reverseToneAssignment = (bool)Amount8;
    var tonePriority = (int)Amount9;
    var oilBrushSize = (int)Amount10;
    var reliefAngle1 = (double)Amount11;
    var reliefAngle2 = (double)Amount12;
    var useRelief = (bool)Amount13;
    
    if (showOriginal)
    {
        dst.CopySurface(src);
        return;
    }
    
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

            if (!colorCounts.ContainsKey(color))
            {
                colorCounts.Add(color, 1);
            }
            else
            {
                colorCounts[color] += 1;
            }
            
            var toneBand = GetToneBand(src[x,y]);
            
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
    
    foreach(var toneBand in toneBandPaletteAssignments)
    {
        Debug.WriteLine(toneBand);
    }
      
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
    
    if (!useRelief)
    {
        dst.CopySurface(tmpSurface1);
        return;
    }
    
    if (IsCancelRequested) return;
    
    Relief(tmpSurface2, tmpSurface1, rect, reliefAngle1);
    
    if (IsCancelRequested) return;
    
    Relief(tmpSurface1, tmpSurface2, rect, reliefAngle2);

    FixReliefBorders(tmpSurface1, rect);
    
    dst.CopySurface(tmpSurface1);
    //dst.CopySurface(tmpSurface2);
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

