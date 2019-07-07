// Name:Simple Map Builder
// Submenu:Chris
// Author:
// Title:Simple Map Builder
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
ListBoxControl Amount1 = 0; // Style|Ambient Occlusion|Specularity+Smoothness|Ambient Occlusion+Smoothness
ListBoxControl Amount2= 1; // Transparent Import|As Is|Transparent Black|Transparent|White|Black
ListBoxControl Amount3 = 3; // AO Source|Red|Green|Blue|Tone|Alpha
IntSliderControl Amount4 = 20; // [0,12550] AO Low Threshold
IntSliderControl Amount5 = 80; // [0,255] AO High Threshold
IntSliderControl Amount6 = 255; // [0,255] AO Max
ListBoxControl Amount7 = 3; // Specularity Source|Red|Green|Blue|Tone|Alpha
IntSliderControl Amount8 = 150; // [0,255] Specularity Low Threshold
IntSliderControl Amount9 = 200; // [0,255] Specularity High Threshold
IntSliderControl Amount10 = 60; // [0,255] Specularity Max
ListBoxControl Amount11 = 1; // Smoothness Source|Red|Green|Blue|Tone|Alpha
IntSliderControl Amount12 = 100; // [0,255] Smoothness Low Threshold
IntSliderControl Amount13 = 200; // [0,255] Smoothness High Threshold
IntSliderControl Amount14 = 60; // [0,255] Smoothness Max

#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var style = Amount1;
    var transparentImport = Amount2;
    var aoSource = Amount3;
    var aoLow = Amount4;
    var aoHigh = Amount5;
    var aoMaximum = Amount6;
    var specularitySource = Amount7;
    var specularityLow = Amount8;
    var specularityHigh = Amount9;
    var specularityMaximum = Amount10;
    var smoothnessSource = Amount11;
    var smoothnessLow = Amount12;
    var smoothnessHigh = Amount13;
    var smoothnessMaximum = Amount14;
    
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            if (CurrentPixel.A == 0 && transparentImport > 0)
            {
                switch(transparentImport)
                {
                    case 1:
                        CurrentPixel = ColorBgra.TransparentBlack;
                       break;
                    case 2:
                        CurrentPixel = ColorBgra.Transparent;
                       break;
                    case 3:
                        CurrentPixel = ColorBgra.White;
                       break;
                    case 4:
                        CurrentPixel = ColorBgra.Black;
                       break;
                }
            }
            var ao = GetValueForComparison(CurrentPixel, aoSource);
            var spec = GetValueForComparison(CurrentPixel, aoSource);
            var smoothness = GetValueForComparison(CurrentPixel, aoSource);
            
            var aoResult = GetResultForComparison(ao, aoLow, aoHigh, aoMaximum);
            var specResult = GetResultForComparison(spec, specularityLow, specularityHigh, specularityMaximum);
            var smoothnessResult = GetResultForComparison(smoothness, smoothnessLow, smoothnessHigh, smoothnessMaximum);
            
            if (style == 0)
            {
                CurrentPixel.R = aoResult;
                CurrentPixel.G = aoResult;
                CurrentPixel.B = aoResult;
                CurrentPixel.A = 255;
            }
            else if (style == 1)
            {
                CurrentPixel.R = specResult;
                CurrentPixel.G = specResult;
                CurrentPixel.B = specResult;
                CurrentPixel.A = smoothnessResult;
            }
            else if (style == 2)
            {
                CurrentPixel.R = aoResult;
                CurrentPixel.G = aoResult;
                CurrentPixel.B = aoResult;
                CurrentPixel.A = smoothnessResult;
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}

byte GetResultForComparison(byte comparisonByte, int low, int high, int maximum)
{
    
    if (comparisonByte > high)
    {
        return (byte)maximum;
    }
    else if (comparisonByte > 0 && comparisonByte > low)
    {
        var normalized = Normalized(comparisonByte, 0, maximum);
        
        return (byte)(int)(normalized * maximum);
    }
    else
    {
        return byte.MinValue;
    }                
}

byte GetValueForComparison(ColorBgra colorOne, int sourceType)
{
    //Red|Green|Blue|Tone|Alpha
    
    switch(sourceType)
    {
        case 0:
            return colorOne.R;
        case 1:
            return colorOne.G;
        case 2:
            return colorOne.B;
        case 3:
            return GetTone(colorOne);
        case 4:
            return colorOne.A;
    }
    
    return byte.MinValue;
}

byte GetTone(ColorBgra colorOne)
{    
    return (byte)(int) (((float)colorOne.R + (float)colorOne.G + (float)colorOne.B) / 3f);
}

private float Normalized(int thisValue, int minimum, int maximum)
{
    return (((float)thisValue) - ((float)minimum)) / (((float)maximum) - ((float)minimum));
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

void SurfaceBlur(Surface dst, Surface src, Rectangle rect, int radius)
{
    SurfaceBlurEffect effect = new SurfaceBlurEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(SurfaceBlurEffect.PropertyName.Radius, radius);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}