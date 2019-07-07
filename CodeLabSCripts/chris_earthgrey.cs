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
IntSliderControl Amount1 = 16; // [1,96] Color Bands
IntSliderControl Amount2 = 1024; // [24,2048] Band Samples
IntSliderControl Amount3 = 24; // [1,60] Blur Amount
IntSliderControl Amount4 = 10; // [0,50] SV Difference Between Bands
IntSliderControl Amount5 = 3; // [1,20] Oil Brush Size
IntSliderControl Amount6 = 50; // [1,100] Oil Brush Coarseness
DoubleSliderControl Amount7 = 45; // [-180,180] Relief Angle 1
DoubleSliderControl Amount8 = 135; // [-180,180] Relief Angle 2
CheckboxControl Amount9 = false; // [0,1] Show Tone Band Selections
#endregion

public class HsvCount
{
    public HsvColor hsvColor;
    public ColorBgra colorBgra;
    public int count;
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    int colorBands = Amount1;
    int bandSamples = Amount2;
    int blurAmount = Amount3;
    int svDifferenceBetweenBands = Amount4;
    int oilBrushSize = Amount5;
    int oilBrushCoarseness = Amount6;
    
    double reliefAngle1 = Amount7;
    double reliefAngle2 = Amount8;
    
    bool showToneBandSelections = Amount9;

    Surface intermediary1 = new Surface(dst.Size);
    Surface intermediary2 = new Surface(dst.Size);

    GaussianBlur(intermediary1, src, rect, blurAmount);

    ColorBgra CurrentPixel;

    Dictionary<ColorBgra, int> colors = new Dictionary<ColorBgra, int>();

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
            for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = intermediary1[x,y];

            if (!colors.ContainsKey(CurrentPixel))
            {
                colors.Add(CurrentPixel, 1);
            }
            else
            {
                colors[CurrentPixel] += 1;
            }
        }
    }


    List<HsvCount> sortedColorResults = new List<HsvCount>();

    foreach(var color in colors)
    {
        sortedColorResults.Add
        (
            new HsvCount(){
                hsvColor = HsvColor.FromColor(color.Key.ToColor()),
                colorBgra = color.Key,
                count = color.Value}
        );
    }

    Debug.WriteLine("SortedColors: " + sortedColorResults.Count);
    sortedColorResults.Sort((x,y) => y.count.CompareTo(x.count));

    var width = rect.Right - rect.Left;
    var bandWidth = width / colorBands;

    var minimumBand = 255;
    var maximumBand = 0;

    if (IsCancelRequested) return;

    for(var i = 0; i < bandSamples; i++)
    {
        var color = sortedColorResults[i];

        var average = GetToneBand(color.colorBgra);

        if (average > maximumBand)
        {
            maximumBand = average;
        }

        if (average < minimumBand)
        {
            minimumBand = average;
        }
    }

    var bandRange = maximumBand - minimumBand;

    var segmentWidth = bandRange / colorBands;

    var bandColors = new ColorBgra[colorBands];

    var unassignedCount = colorBands;
    var iteration = 0;
    var maxIteration = 20;

    do
    {
        for(var i = 0; i < sortedColorResults.Count; i++)
        {
            var color = sortedColorResults[i].colorBgra;
            var hsv = HsvColor.FromColor(color.ToColor());
            var band = GetToneBand(color);

            var index = GetToneBandIndex(band, minimumBand, maximumBand, colorBands);

            if (bandColors[index].R == 0 && bandColors[index].G == 0 && bandColors[index].B == 0 && bandColors[index].A == 0)
            {
                if (IsColorAllowedBasedOnSvRequirements(bandColors, hsv, index, svDifferenceBetweenBands, colorBands))
                {
                    bandColors[index] = color;
                    unassignedCount -= 1;
                }
            }
        }

        if (IsCancelRequested) return;

        iteration += 1;

        svDifferenceBetweenBands /= 2;

        if (iteration > maxIteration)
        {
            Debug.WriteLine("Ending after " + iteration + " iterations.");
            break;
        }
    }
    while (unassignedCount > 0);

    if (IsCancelRequested) return;

    foreach(var bandColor in bandColors)
    {
        PrintColorInformation(bandColor);
    }

    if (showToneBandSelections)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int x = rect.Left; x < rect.Right; x++)
            {
                var index = x / (width / bandColors.Length);

                if (index < 0)
                {
                    index = 0;
                }
                else if (index >= bandColors.Length)
                {
                    index = bandColors.Length-1;
                }

                dst[x,y] = bandColors[index];
            }
        }

        return;
    }

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;

        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = intermediary1[x,y];

            var band = GetToneBand(CurrentPixel);
            var index = GetToneBandIndex(band, minimumBand, maximumBand, colorBands);

            intermediary1[x,y] = bandColors[index];
        }
    }

    if (IsCancelRequested) return;
    
    OilPainting(intermediary2, intermediary1, rect, oilBrushSize, oilBrushCoarseness);

    if (IsCancelRequested) return;
    
    Relief(intermediary1, intermediary2, rect, reliefAngle1);
    
    if (IsCancelRequested) return;
    
    Relief(dst, intermediary1, rect, reliefAngle2);
    
    if (IsCancelRequested) return;
    
 
}



int GetToneBand(ColorBgra color)
{
    return (color.R + color.G + color.B) / 3;
}

int GetToneBandIndex(int band, int minimumBand, int maximumBand, int amountOfBands)
{
    var normalized = (float)(band - minimumBand) / (float)(maximumBand - minimumBand);
    var index = Convert.ToInt32(amountOfBands * normalized);

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

bool IsColorAllowedBasedOnSvRequirements(ColorBgra[] bandColors, HsvColor thisColor, int currentIndex, int svDifference, int amountOfBands)
{

    if (currentIndex == amountOfBands-1)
    {
        return CheckBandForSvRequirements(bandColors, thisColor, currentIndex-1, svDifference);
    }
    else if (currentIndex == 0)
    {
        return CheckBandForSvRequirements(bandColors, thisColor, currentIndex+1, svDifference);
    }
    else
    {
        return CheckBandForSvRequirements(bandColors, thisColor, currentIndex-1, svDifference) && CheckBandForSvRequirements(bandColors, thisColor, currentIndex+1, svDifference);
    }
}

bool CheckBandForSvRequirements(ColorBgra[] bandColors, HsvColor thisColor, int targetIndex, int svDifference)
{
    if (bandColors[targetIndex].R == 0 && bandColors[targetIndex].G == 0 && bandColors[targetIndex].B == 0 && bandColors[targetIndex].A == 0)
    {
        return true;
    }
    else
    {
        var otherHsv = HsvColor.FromColor(bandColors[targetIndex].ToColor());

        var difference = Math.Abs(((otherHsv.Saturation + otherHsv.Value) / 2) - ((thisColor.Saturation + thisColor.Value)/2));

        if (difference >
         svDifference)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

void PrintColorInformation(HsvColor color)
{
    Debug.WriteLine(ColorBgra.FromColor(color.ToColor()).ToString() + " | " + color.ToString());
}

void PrintColorInformation(ColorBgra color)
{
    Debug.WriteLine(color.ToString() + " | " + HsvColor.FromColor(color.ToColor()).ToString());
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

