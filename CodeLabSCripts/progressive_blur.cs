// Name:Progressive Blur
// Submenu:Chris
// Author:
// Title:Progressive Blur
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
ListBoxControl Amount1 = 0; // Direction|Left|Right|
IntSliderControl Amount2 = 20; // [0,1024] Fade Start
IntSliderControl Amount3 = 20; // [1,1024] Fade Length
IntSliderControl Amount4 = 1024; // [0,1024] Streak Length
IntSliderControl Amount5 = 1; // [1,10] Fade Strength
CheckboxControl Amount6 = false; // Show Original
CheckboxControl Amount7 = false; // Show Lines
IntSliderControl Amount8 = 2; // [2,10] Debug Line Size
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
   
    var direction = (int)Amount1;
    var fadeStart = (int)Amount2;
    var fadeLength = (int)Amount3;
    var streakLength = (int)Amount4;
    var fadeStrength = (int)Amount5;
    var showOriginal = (bool)Amount6;
    var showLines = (bool)Amount7;
    var debugLineSize = (int)Amount8;
    
    // Delete any of these lines you don't need
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    var realFadeStart = selection.Left + fadeStart;
    var realFadeEnd = realFadeStart + fadeLength;
    var realStreakStart = realFadeEnd;
    var realStreakEnd = realStreakStart + streakLength;
    
    if (showOriginal)
    {
        dst.CopySurface(src);
        
        if (showLines)
        {
            if (direction == 0)
            {
                DrawLinesOnSurface(selection, selection.Right-1-realFadeStart, selection.Right-1-realFadeEnd, selection.Right-1-realStreakStart, debugLineSize, dst);
            }
            else 
            {
                DrawLinesOnSurface(selection, realFadeStart, realFadeEnd, realStreakStart, debugLineSize, dst);
            }
        }
        
        return;
    }
    
    
    var working = new Surface(src.Size);
    working.CopySurface(src);
    
    if (direction == 0)
    {
        for(var x = selection.Left; x < selection.Right; x++)
        {
            for(var y = selection.Top; y < selection.Bottom; y++)
            {
                var match = selection.Left+(selection.Right-1-x);
                
                working[x,y] = src[match,y];
            }
        }
    }
    
    var clean = new Surface(src.Size);
    clean.CopySurface(working);
    var staging = new Surface(src.Size);
    staging.CopySurface(working);

    ColorBgra CurrentPixel;

    var count = 0;
    
    for (int x = rect.Left; x < rect.Right; x++)
    {
        if (IsCancelRequested) return;
        
        if (x >= realFadeStart && x < realFadeEnd)
        {
            var surfRect = new Rectangle(x, selection.Top, realFadeEnd-x, selection.Bottom-selection.Top);
            var tempSurf = new Surface(surfRect.Size);
            var tempRect = new Rectangle(0, 0, tempSurf.Width, tempSurf.Height);
            
            MapSurface(tempSurf, working, x, selection.Top);
            
            GaussianBlur(tempSurf, tempSurf, tempRect, fadeStrength > (count) ? count : fadeStrength);
            count += 1;
            
            UnmapSurface(working, tempSurf, x, selection.Top);
        }
    }
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        for (int x = rect.Left; x < rect.Right; x++)
        {  
            if (IsCancelRequested) return;
            
            if (x < realFadeStart)
            {
                staging[x,y] = ColorBgra.Black;
                staging[x,y] = clean[x,y];
            }
            if (x >= realFadeStart && x < realFadeEnd)
            {
                staging[x,y] = ColorBgra.Black;
                staging[x,y] = working[x,y];
            }
            else if (x >= realStreakStart && x < realStreakEnd)
            {
                staging[x,y] = ColorBgra.Black;
                staging[x,y] = working[realFadeEnd-1, y];
            }
            else 
            {   staging[x,y] = ColorBgra.Black;
                staging[x,y] = clean[x,y];
                
            }
        }
    }
    
    dst.CopySurface(staging);
    
    if(showLines)
    {
        DrawLinesOnSurface(selection, realFadeStart, realFadeEnd, realStreakStart, debugLineSize, staging);           
        DrawLinesOnSurface(selection, realFadeStart, realFadeEnd, realStreakStart, debugLineSize, dst);
    }
    
     if (direction == 0)
     {
        for(var x = selection.Left; x < selection.Right; x++)
        {
            for(var y = selection.Top; y < selection.Bottom; y++)
            {
                var match = selection.Left+(selection.Right-1-x);
                
                dst[x,y] = staging[match,y];
            }
        }
    }
   
   
}

private void MapSurface(Surface dst, Surface src, int srcX, int srcY)
{
    for(var y = 0; y < dst.Height; y++)
    {
        for(var x = 0; x < dst.Width; x++)
        {
            dst[x,y] = src[srcX+x,srcY+y];
        }
    }
}

private void UnmapSurface(Surface dst, Surface src, int dstX, int dstY)
{
    for(var y = 0; y < src.Height; y++)
    {
        for(var x = 0; x < src.Width; x++)
        {
            dst[dstX+x,dstY+y] = src[x,y];
        }
    }
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

void DrawLinesOnSurface(Rectangle selection, int realFadeStart, int realFadeEnd, int realStreakEnd, int debugLineSize, Surface dst)
{    
    for(var x = selection.Left; x < selection.Right; x++)
    {
        for(var y = selection.Top; y < selection.Bottom; y++)
        {
            if (x == realFadeStart || x == realFadeEnd || x == realStreakEnd)
            {
                var lineSize = (int)(((float)debugLineSize)/2f);
                
                for(var i = -lineSize; i < lineSize; i++)
                {
                    var lineX = x+i;
                    
                    if (lineX < selection.Left || lineX > selection.Right-1)
                    {
                        continue;
                    } 
                    
                    dst[lineX,y] = ColorBgra.Magenta;
                }
            }
        }
    }
    
}
