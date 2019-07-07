// Name:Soften
// Submenu:Chris
// Author:
// Title:Soften
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 3; // [0,200] Range
IntSliderControl Amount2 = 50; // [0,100] Threshold
CheckboxControl Amount3 = true; // Invert Softening
CheckboxControl Amount4 = false; // Override Blend Mode
BinaryPixelOp Amount5 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Blend Operation
IntSliderControl Amount6 = 255; // [0,255] Blend Opacity
IntSliderControl Amount7 = 10; // [0,20] Polish
CheckboxControl Amount8 = false; // Show Original
#endregion

private int savedStrength = 0;
private Surface working = null;
private object workLock = new object();

void Render(Surface dst, Surface src, Rectangle rect)
{       
    if (working == null)
    {
        lock(workLock)
        {
            if (working == null)
            {
                working = new Surface(src.Size);
            }
        }
    }
        
    var outlineThickness = (int)Amount1;
    var outlineIntensity = (int)Amount2;
    var invertSoftening = (bool)Amount3;
    var overrideBlendMode = (bool)Amount4;
    var blendOp = Amount5;
    var blendOpacity = (int)Amount6;
    var sharpenAmount = (int)Amount7;
    var showOriginal = (bool)Amount8;
    
   
    var actualBlendOp = overrideBlendMode ? blendOp : new UserBlendOps.DarkenBlendOp();
    
    Outline(working, src, rect, outlineThickness, outlineIntensity);

    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (showOriginal)
            {
                dst[x,y] = src[x,y];
                continue;
            } 
    
            var target = working[x,y];
            target.A = (byte)blendOpacity;
            
            if (target == ColorBgra.Black || 
            target == ColorBgra.Transparent ||
            target == ColorBgra.TransparentBlack ||
            target == ColorBgra.White ||
            target.A < 5 ||
            (target.R < 3 && target.G < 3 && target.B < 3) ||
            (target.R > 250 && target.G > 250 && target.B > 250))
            {
                working[x,y] = src[x,y];
            }
            else 
            {
                if (invertSoftening)
                {
                    target = ColorBgra.FromBgra(
                        (byte)(255 - target.B),
                        (byte)(255 - target.G),
                        (byte)(255 - target.R),
                        target.A);
                }
                working[x,y] = actualBlendOp.Apply(src[x,y], target);
            }
        }
    }
    
    if (showOriginal)
    {
        return;
    }
    Sharpen(dst, working, rect, sharpenAmount);
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

void Outline(Surface dst, Surface src, Rectangle rect, int thickness, int intensity)
{
    OutlineEffect effect = new OutlineEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(OutlineEffect.PropertyNames.Intensity, intensity);
    parameters.SetPropertyValue(OutlineEffect.PropertyNames.Thickness, thickness);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

void Sharpen(Surface dst, Surface src, Rectangle rect, int sharpenAmount)
{
    SharpenEffect effect = new SharpenEffect();
    PropertyCollection properties = effect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken parameters = new PropertyBasedEffectConfigToken(properties);
    
    parameters.SetPropertyValue(SharpenEffect.PropertyNames.Amount, sharpenAmount);
    effect.SetRenderInfo(parameters, new RenderArgs(dst), new RenderArgs(src));
    
    effect.Render(new Rectangle[1] {rect},0,1);
}

int GetToneDifference(ColorBgra colorOne, ColorBgra colorTwo)
{
    var diffR = (float)Math.Abs(colorOne.R - colorTwo.R);
    var diffG = (float)Math.Abs(colorOne.G - colorTwo.G);
    var diffB = (float)Math.Abs(colorOne.B - colorTwo.B);
    
    return (int) ((diffR + diffG + diffB) / 3f);
}

int GetAlphaValueFromToneDifference(int toneDifference)
{
    return (int)(((float)(toneDifference)/((float)255)));
}