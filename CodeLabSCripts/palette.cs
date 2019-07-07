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
IntSliderControl Amount1 = 30; // [1,100] Blur Radius
IntSliderControl Amount2 = 1; // [1,100] Color Sample Efficiency
IntSliderControl Amount3 = 5; // [1,100] Color Equality
IntSliderControl Amount4 = 4; // [1,5] Color Palette Length
CheckboxControl Amount5 = false; // [0,1] ShowOriginal
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var blurRadius = (int)Amount1;
    var colorSampleEfficiency = (int)Amount2;
    var colorEquality = (int)Amount3;
    var colorPaletteLength = (int)Amount4;
    var showOriginal = (bool)Amount5;

    if (showOriginal)
    {
        dst.CopySurface(src);
        return;
    }

    var colorCounts = new Dictionary<ColorBgra, int>();

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

    var palette = GetColorPalette(colorList.ToArray(), colorPaletteLength);
    
    foreach(var color in palette)
    {
        Debug.WriteLine(color);
    }
    

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;

        for (int x = rect.Left; x < rect.Right; x++)
        {
            
            
            dst[x,y] = palette[index];
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
  