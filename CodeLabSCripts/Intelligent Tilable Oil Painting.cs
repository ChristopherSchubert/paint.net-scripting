// Name:Intelligent Tilable Oil Painting
// Submenu:Chris
// Author:
// Title:Intelligent Tilable Oil Painting
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
ListBoxControl Amount1 = 0; // Tone Blend Mode|Normal|Additive|Average|Blue|Color|ColorBurn|ColorDodge|Cyan|Darken|Difference|Divide|Exclusion|Glow|GrainExtract|GrainMerge|Green|HardLight|HardMix|Hue|Lighten|LinearBurn|LinearDodge|LinearLight|Luminosity|Magenta|Multiply|Negation|Overlay|Phoenix|PinLight|Red|Reflect|Saturation|Screen|SignedDifference|SoftLight|Stamp|VividLight|Yellow
CheckboxControl Amount2 = true; // [0,1] Low Tone Blends First
ListBoxControl Amount3 = 0; // Blend Source When No High Tone|None|Source|Zoomed Source|Tilted Source|Source Saturation|Source Value|Clouds
CheckboxControl Amount4 = true; // [0,1] Finalize Blotting
CheckboxControl Amount5 = false; // [0,1] Finalize Palette
CheckboxControl Amount6 = true; // [0,1] Patch Blending Issues
IntSliderControl Amount7 = 105; // [0,200] Saturation Adjustment
IntSliderControl Amount8 = 3; // [-50,50] Contrast Adjustment
CheckboxControl Amount9 = true; // [0,1] Fully Opaque
CheckboxControl Amount10 = false; // [0,1] Show Half
CheckboxControl Amount11 = false; // [0,1] Show Original
ReseedButtonControl Amount12 = 0; // [255] Reprocess
CheckboxControl Amount13 = false; // [0,1] Retain Transparency
CheckboxControl Amount14 = true; // [0,1] {!Amount28} Use Original Transparency As Guideline
IntSliderControl Amount15 = 127; // [1,255] {Amount11} Reject Alpha Below
DoubleSliderControl Amount16 = 2; // [0.1,10,4] Main X Zoom Out
DoubleSliderControl Amount17 = 2; // [0.1,10,4] Main Y Zoom Out
IntSliderControl Amount18 = 1; // [1,100] Main Blur Radius
IntSliderControl Amount19 = 10; // [1,100] Color Sample Efficiency
IntSliderControl Amount20 = 50; // [1,100] Color Frequency Equalization
IntSliderControl Amount21 = 10; // [0,100,5] Low Color Limit
IntSliderControl Amount22 = 235; // [128,255,5] High Color Limit
IntSliderControl Amount23 = 4; // [1,5] Color Palette Length
IntSliderControl Amount24 = 12; // [2,20] Color Palette Hue Segmentation
IntSliderControl Amount25 = 1; // [1,8] Post Processing Oil Painting Brush Width
IntSliderControl Amount26 = 8; // [1,8] Tone Bunching
IntSliderControl Amount27 = 8; // [1,100] Coverage Multiplier
ListBoxControl Amount28 = 7; // Debug Mode|Show Base Overlay|Show Clean Overlay|Show Overlay Spread|Show Low Blend Source|Show High Blend Source|Show Replacement Blend Source|Show Complete Blend Source|Final
#endregion

private bool hasLayers = false;
unsafe void PreRenderInternal(Surface src)
{  
    hasLayers = false;

    working.CopySurface(src);

    if (!retainTransparency && useOriginalTransparencyAsGuideline)
    {
        for (var y = 0; y < working.Height; y++)
        {
            ColorBgra* ptr = working.GetPointAddressUnchecked(0, y);
            
            for (var x = 0; x < working.Width; x++)
            {
                if (IsCancelRequested) return;
            
                ColorBgra CurrentPixel = *ptr;

                if (CurrentPixel.A < rejectAlphaBelow)
                {
                    *ptr = ColorBgra.TransparentBlack;
                }
                
                ptr++;
            }
        }
    }
    
    RotateAndTile(working2, working, Pair.Create(0D,0D), mainXZoomFactor, mainYZoomFactor, 0, 0, 0, 1);
    RotateAndTile(working3, working, Pair.Create(0D,0D), mainXZoomFactor, mainYZoomFactor, 0, 0, 0, 4);
    working.CopySurface(working2); 

    if (mainBlurRadius > 0)
    {
        GaussianBlur(working, working, selection, mainBlurRadius);

        ApplyAlphaThresholdToSurface(working, 0, 0);
    }

    for (var y = 0; y < working.Height; y++)
    {
        ColorBgra* ptr = working.GetPointAddressUnchecked(0, y);
        ColorBgra* w3ptr = working3.GetPointAddressUnchecked(0, y);
        
        for (var x = 0; x < working.Width; x++)
        {
            if (IsCancelRequested) return;
        
            ColorBgra CurrentPixel = *w3ptr;

            if (CurrentPixel == ColorBgra.Transparent)
            {
                *ptr = ColorBgra.Transparent;
            }
            
            ptr++;
            w3ptr++;
        }
    }

    working2.Clear(ColorBgra.Transparent);
    working3.Clear(ColorBgra.Transparent);

    if (IsCancelRequested) return;
    
    palette = new IntelligentColorPalette();
    palette.InitializePalette(working, colorSampleEfficiency, colorFrequencyEqualization, lowColorLimit, highColorLimit, colorPaletteLength, colorPaletteHueSegmentation);
    palette.ReplaceSurfaceColorsWithPalette(working, working, selection);
    
    OilPainting(working, working, selection, oilBrushWidth, 50);    
    palette.ReplaceSurfaceColorsWithPalette(working, working, selection);
    
    if (IsCancelRequested) return;

    var counts = new Dictionary<ColorBgra, int>();
    for (var y = 0; y < working.Height; y++)
    {
        ColorBgra* ptr = working.GetPointAddressUnchecked(0, y);
        
        for (var x = 0; x < working.Width; x++)
        {
            if (IsCancelRequested) return;
        
            ColorBgra currentPixel = *ptr;
            
            if (currentPixel != ColorBgra.Transparent && currentPixel != ColorBgra.TransparentBlack)
            { 
                if (!counts.ContainsKey(currentPixel))
                {
                    counts.Add(currentPixel, 0);
                }
                
                counts[currentPixel] += 1;
            }
            
            ptr++;
        }
    }
    
    var colorCounts = counts.OrderByDescending(c => c.Value).ToList();
    var firstColor = colorCounts[0];
        
    // this is the good copy
    working2.CopySurface(working);
    working3.CopySurface(working);
    working4.CopySurface(working);
    
    for (var y = 0; y < working.Height; y++)
    {
        ColorBgra* wptr = working.GetPointAddressUnchecked(0, y);
        
        for (var x = 0; x < working.Width; x++)
        {
            if (IsCancelRequested) return;
        
            ColorBgra CurrentPixel = *wptr;

            if (CurrentPixel == firstColor.Key)
            {
                *wptr = ColorBgra.TransparentBlack;
            }
            
            wptr++;
        }
    }
    
    if (debugMode == 0 || IsCancelRequested)
    {
        return; 
    }

    var minX = working.Width;
    var minY = working.Height;
    var maxX = 0; 
    var maxY = 0;
    
    for (var y = 0; y < working.Height; y++)
    {
        ColorBgra* ptr = working.GetPointAddressUnchecked(0, y);
        
        for (var x = 0; x < working.Width; x++)
        {
            if (IsCancelRequested) return;
            
            var CurrentPixel = *ptr;
            
            if (CurrentPixel.A == byte.MaxValue)
            {
                if (x < minX) { minX = x; }
                if (y < minY) { minY = y; }
                if (x >= maxX) { maxX = x; }
                if (y >= maxY) { maxY = y; }
            }
        
            ptr++;
        }
    }
        
    var trimIntrusion = 24;
    
    for (var y = minY; y <= maxY; y++)
    {        
        for (var x = minX; x <= maxX; x++)
        {
            if (IsCancelRequested) return;
            
            var color = working[x,y];

            if (color.A == 0)
            {
                continue;
            }
            
            if (
                (x >= minX && x < minX + trimIntrusion) ||
                (y >= minY && y < minY + trimIntrusion) ||
                (x <= maxX && x > maxX - trimIntrusion) ||
                (y >= maxY && y < maxY - trimIntrusion)
            )
            {
                var comparison = new Func<FloodFillColorSet, bool>(set => set.currentColor == set.initialColor);
                var fill = new Func<FloodFillColorSet, ColorBgra>(set => ColorBgra.TransparentBlack);

                FloodFill(x, y, working, selection, comparison, fill);
                
                if (IsCancelRequested) return;
            }
        }
    }
    
    if (debugMode <= 1 || IsCancelRequested)
    {
        return; 
    }
    
    var maxZoomFactor = Math.Max(mainXZoomFactor, mainYZoomFactor);
    var maxEdgeFactor = (int)Math.Max(width, height);
     
    working2.CopySurface(working);
    working3.CopySurface(working);
    working4.Clear(ColorBgra.TransparentBlack);

    working.Clear(firstColor.Key);
    
    if (IsCancelRequested) return;
    
    int newX = 0;
    int newY = 0;
    int offset = 0;
    
    var colorIndexes = new Dictionary<ColorBgra,int>();
    
    var colorsByTone = colorCounts.OrderBy((c) => GetTone(c.Key)).ToList();
    for(var i = 0; i < colorCounts.Count; i++)
    {
        colorIndexes.Add(colorsByTone[i].Key, i);
    }
    
    var colorDivisionPoint = (int)(((float)colorCounts.Count) * .5f);
    
    var iterations = (int)((maxZoomFactor*maxZoomFactor)*coverageMultiplier);
    var colorOffsets = new int[iterations, colorCounts.Count];
    
    var colorDepth = colorOffsets.GetLength(1);
    
    for(var i = 0; i < colorOffsets.GetLength(0); i++)
    {
        for(var j = 0; j < colorDepth; j += toneBunching)
        {
            var random = rng.Next(-maxEdgeFactor, maxEdgeFactor);
            
            for(var k = 0; k < toneBunching; k++)
            {
                if (j+k < colorDepth)
                {
                    colorOffsets[i,j+k] = random;
                }
            }
        }
    }
    
    var comparisonOperation = new Func<FloodFillColorSet, bool>(set => set.currentColor == set.initialColor);
    
    var fillOperation = new Func<FloodFillColorSet, ColorBgra>(set => 
    {
        newX = set.currentCoordinates.First + offset;
        newY = set.currentCoordinates.Second + offset;
        
        while(newX >= width)    newX -= (int)width;
        while(newX < 0)         newX += (int)width;
        while(newY >= height)   newY -= (int)height;
        while(newY < 0)         newY += (int)height;
        
        var index = colorIndexes[set.currentColor];
        if (index < colorDivisionPoint)
        {
            working[newX,newY] = set.currentColor;
        }
        else 
        {
            working4[newX,newY] = set.currentColor;
        }
        
        return ColorBgra.TransparentBlack;
    });
    

    for(var i = 0; i < iterations; i++)
    {

        var distanceX = (double)((rng.Next(0,2) >= 1 ? 1 : -1) * ((float)rng.Next(1, 50))/100f);
        var distanceY = (double)((rng.Next(0,2) >= 1 ? 1 : -1) * ((float)rng.Next(1, 50))/100f);
        var zoomX = 1 + (double)((rng.Next(0,2) >= 1 ? 1 : -1) * ((float)rng.Next(1, 50))/100f);
        var zoomY = 1 + (double)((rng.Next(0,2) >= 1 ? 1 : -1) * ((float)rng.Next(1, 50))/100f);
        var rotation =  (rng.Next(0,2) >= 1 ? 1 : -1) * ((float)rng.Next(0, 5));
        
        RotateAndTile(working3, working2, Pair.Create(distanceX,distanceY), zoomX, zoomY, 0, 0, rotation, 1);
          
        for (var y = 0; y < working3.Height; y++)
        {        
            for (var x = 0; x < working3.Width; x++)
            {
                if (IsCancelRequested) return;
                
                var cellColor = working3[x,y];
                
                if (!colorIndexes.ContainsKey(cellColor))
                {
                    continue;
                }
                
                var colorIndex = colorIndexes[cellColor];
                
                if (colorIndex ==0)
                {
                    continue;
                }
                
                offset = colorOffsets[i, colorIndex];
                
                FloodFill(x,y, working3, selection, comparisonOperation, fillOperation);
            }
        }
    }
    
    if (debugMode <= 2 || IsCancelRequested)
    {
        return; 
    }
    
    working2.CopySurface(working4);
    
    UpdateAuxBlendSource(working4, src);
    
    hasLayers = true;
}

unsafe void UpdateAuxBlendSource(Surface dst, Surface src)
{
    
    /*
    
    // Blend Source When No High Tone|
    0 None|
    1 Source|
    2 Zoomed Source|
    3 Tilted Source|
    4 Source Saturation|
    5 Source Value|
    6 Clouds
    */
    switch(blendSourceWhenNoHighTone)
    {
        case 0:
            dst.Clear(ColorBgra.TransparentBlack);
            return;
        case 1:
            dst.CopySurface(src);
            return;
        case 2:
            RotateAndTile(dst, src, Pair.Create(0D,0D), mainXZoomFactor, mainYZoomFactor, 0, 0, 0, 0);
            return;
        case 3:
            RotateAndTile(dst, src, Pair.Create(0D,0D), mainXZoomFactor, mainYZoomFactor, 30, 1, -1, 0);
            return;
   }
    
          
    if (blendSourceWhenNoHighTone == 4 || blendSourceWhenNoHighTone == 5)
    {
        dst.CopySurface(src);        
    }
    if (blendSourceWhenNoHighTone == 6)
    {
        var rect = new Rectangle(0, 0, dst.Width, dst.Height);
        RenderClouds(dst, dst, rect, 500, 1);
    }
}

unsafe void RenderInternal(Surface dst, Surface src)
{
    /*

    Debug Mode|
    0 Show Base Overlay|
    1 Show Clean Overlay|
    2 Show Overlay Spread|
    3 Show Low Blend Source|
    4 Show High Blend Source|
    5 Show Replacement Blend Source|
    6 Show Complete Blend Source|
    7 Final
    */
    if (debugMode <= 2)
    {
        CopySurfaceRectangle(dst, working, selection);        
        return;
    }
    else if (debugMode == 3)
    {
        CopySurfaceRectangle(dst, working, selection);
        return;
    } 
    else if (debugMode == 4)
    {
        CopySurfaceRectangle(dst, working2, selection);
        return;
    }
    else if (debugMode == 5)
    {
        CopySurfaceRectangle(dst, working4, selection);
        return;
    }
    else if (debugMode == 6)
    {
        BlendSurfaces(dst, working4, working2, 1D, BlendModes.Normal);
        return;
    }
    
    for (int y = selection.Top; y < selection.Bottom; y++)
    {    
        ColorBgra* srcPtr = src.GetPointAddressUnchecked(selection.Left, y);
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(selection.Left, y);
        ColorBgra* wPtr = working.GetPointAddressUnchecked(selection.Left, y);
        ColorBgra* w2Ptr = working2.GetPointAddressUnchecked(selection.Left, y);
        ColorBgra* w4Ptr = working4.GetPointAddressUnchecked(selection.Left, y);
    
        for (int x = selection.Left; x < selection.Right; x++)
        {
            if (IsCancelRequested) return;
            
            ColorBgra lowToneColor = *wPtr;
            ColorBgra highToneColor = *w2Ptr;
            ColorBgra w4ToneColor = *w4Ptr;
            
            var lowToneColorHsv = HsvColor.FromColor(lowToneColor.ToColor());
           
            lowToneColor.A = byte.MaxValue;
            
            if (highToneColor.A != 0)
            {               
                highToneColor.A = byte.MaxValue;
                if (lowTonesBlendFirst)
                {
                    *dstPtr = BlendedPixel(lowToneColor, highToneColor, (BlendModes)toneBlendMode.Value);
                }
                else
                {
                    *dstPtr = BlendedPixel(highToneColor, lowToneColor, (BlendModes)toneBlendMode.Value);
                }               
            }
            else
            {
                if (blendSourceWhenNoHighTone != 0)
                { 
                    if (lowTonesBlendFirst)
                    {
                        *dstPtr = BlendedPixel(lowToneColor, w4ToneColor, (BlendModes)toneBlendMode.Value);
                    }
                    else
                    {
                        *dstPtr = BlendedPixel(w4ToneColor, lowToneColor, (BlendModes)toneBlendMode.Value);
                    }         
                }
                else 
                {
                    *dstPtr = lowToneColor;
                }
            }
         
            srcPtr++;
            dstPtr++;
            wPtr++;
            w2Ptr++;
            w4Ptr++;
        }
    }
}

unsafe void PostRenderInternal(Surface dst, Surface src)
{   
    var temp = new Surface(dst.Size);
    if (finalizeBlotting)
    {
        OilPainting(temp, dst, selection, oilBrushWidth, 10);
        dst.CopySurface(temp);
    }
    
    if (IsCancelRequested) return;
    
    if (finalizePalette)
    {
        palette.ReplaceSurfaceColorsWithPalette(temp, dst, selection);
        dst.CopySurface(temp);
    }
    
    if (IsCancelRequested) return;
    
    if (patchBlendingIssues)
    {
        BlendSurfaceSeams(dst, 20, 10);
    }
    
    if (IsCancelRequested) return;
    
    if (saturationAdjustment != 0)
    {
        HueAndSaturation(temp, dst, selection, 0, saturationAdjustment);
        dst.CopySurface(temp);
    }

    if (IsCancelRequested) return;
    
    if (contrastAdjustment != 0)
    {
        BrightnessAndContrast(temp, dst, selection, 0, contrastAdjustment);
        dst.CopySurface(temp);
    }

    SoftenStraightLines(dst, 10, 5);

    SoftenIslands(dst);
    
    if (retainTransparency)
    {
        for (int y = selection.Top; y < selection.Bottom; y++)
        {    
            ColorBgra* srcPtr = src.GetPointAddressUnchecked(selection.Left, y);
            ColorBgra* dstPtr = dst.GetPointAddressUnchecked(selection.Left, y);
        
            for (int x = selection.Left; x < selection.Right; x++)
            {
                if (IsCancelRequested) return;
                
                ColorBgra srcPixel = *srcPtr;
                ColorBgra dstPixel = *dstPtr;
                
                *dstPtr = ColorBgra.FromBgra(
                    dstPixel.B,
                    dstPixel.G,
                    dstPixel.R,
                    srcPixel.A);
                        
                srcPtr++;
                dstPtr++;
            }
        }
    }
    
    if (!showHalf)
    {
        return;
    }
    
    for (int y = 0; y < dst.Height; y++)
    {
        ColorBgra* srcPtr = src.GetPointAddressUnchecked(0, y);
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(0, y);
        
        for (int x = 0; x < dst.Width; x++)
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

unsafe void PreRender(Surface dst, Surface src)
{
    InternalPreRender(dst, src);
    InternalRender(dst, src);
    InternalPostRender(dst, src);
}


bool preRenderException = false;

unsafe void InternalPreRender(Surface dst, Surface src)
{
    dst = null;

    Debug.WriteLine("");
    Debug.WriteLine("--------------------------------------------------------------------------------------------");
    Debug.WriteLine("EXECUTION STARTED @ " + DateTime.Now.ToString());
    Debug.WriteLine("--------------------------------------------------------------------------------------------");
    Debug.WriteLine("");
    
    try
    {
        processStopwatch.Restart();
        
        selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
        width = selection.Width;
        height = selection.Height;

        selectionCenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
        selectionCenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

        primaryColor = (ColorBgra)EnvironmentParameters.PrimaryColor;
        secondaryColor = (ColorBgra)EnvironmentParameters.PrimaryColor;

        defaultColors = PaintDotNet.ServiceProviderExtensions.GetService<IPalettesService>(Services).DefaultPalette;
        currentColors = PaintDotNet.ServiceProviderExtensions.GetService<IPalettesService>(Services).CurrentPalette;

        if (working == null || working.Size != src.Size)
        {
            working = new Surface(src.Size);
        }
        if (working2 == null || working2.Size != src.Size)
        {
            working2 = new Surface(src.Size);
        }
        if (working3 == null || working3.Size != src.Size)
        {
            working3 = new Surface(src.Size);
        }
        if (working4 == null || working4.Size != src.Size)
        {
            working4 = new Surface(src.Size);
        }
        if (dstWorking == null || dstWorking.Size != src.Size)
        {
            dstWorking = new Surface(src.Size);
        }
        
        Debug.WriteLine("Calling AssignUIValues.");
        AssignUIValues();
        
        if (showOriginal)
        {
            return;
        }

        if (reprocess.HasChanged)
        {
            hasLayers = false;
        }
        else if (blendSourceWhenNoHighTone.HasChanged && blendSourceWhenNoHighTone != 0)
        {
            UpdateAuxBlendSource(working4, src);
        }
        
        if (!hasLayers)
        {
            Debug.WriteLine("Calling PreRenderInternal.");
            working.Clear(ColorBgra.TransparentBlack);
            working2.Clear(ColorBgra.TransparentBlack);
            working3.Clear(ColorBgra.TransparentBlack);
            working4.Clear(ColorBgra.TransparentBlack);
            dstWorking.Clear(ColorBgra.TransparentBlack);
            PreRenderInternal(src);
        }

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

void Render(Surface dst, Surface src, Rectangle rect)
{
}
void PostRender(Surface dst, Surface src)
{
}

unsafe void InternalRender(Surface dst, Surface src)
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
            return;
        }

        Debug.WriteLine("Calling RenderInternal: ");
        RenderInternal(dstWorking, src);
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

unsafe void InternalPostRender(Surface dst, Surface src)
{
    if (preRenderException)
    {
        return;
    }

    try
    {
        if (showOriginal)
        {
            dst.CopySurface(src);
            return;
        } 

        processStopwatch.Restart();

        Debug.WriteLine("Calling PostRenderInternal: ");
        PostRenderInternal(dstWorking, src);
        dst.CopySurface(dstWorking);

        for (int y = 0; y < dst.Height; y++)
        {
            ColorBgra* dstPtr = dst.GetPointAddressUnchecked(0, y);
            
            for (int x = 0; x < dst.Width; x++)
            {
                ColorBgra dstPixel = *dstPtr;
                if (opaque)
                {
                    dstPixel.A = 255;
                }

                *dstPtr = dstPixel;
            }

            dstPtr++;
        }
    }
    catch (Exception x)
    {
        Debug.WriteLine(x);
        return;
    }
    finally
    {
        processStopwatch.Stop();
        Debug.WriteLine("PostRender: " + processStopwatch.ElapsedMilliseconds.ToString() + " ms");        
    }
}

private void AssignUIValues()
{
    toneBlendMode.Value = (int)Amount1;
    lowTonesBlendFirst.Value = (bool)Amount2;
    blendSourceWhenNoHighTone.Value = (int)Amount3;
    finalizeBlotting.Value = (bool)Amount4;
    finalizePalette.Value = (bool)Amount5;
    patchBlendingIssues.Value = (bool)Amount6;
    saturationAdjustment.Value = (int)Amount7;
    contrastAdjustment.Value = (int)Amount8;
    opaque.Value = (bool)Amount9;
    showHalf.Value = Amount10;
    showOriginal.Value = Amount11;
    reprocess.Value = (int)Amount12;
    retainTransparency.Value = (bool)Amount13;
    useOriginalTransparencyAsGuideline.Value = (bool)Amount14;
    rejectAlphaBelow.Value = (int)Amount15;
    mainXZoomFactor.Value = (double)Amount16;
    mainYZoomFactor.Value = (double)Amount17;
    mainBlurRadius.Value = (int)Amount18;
    colorSampleEfficiency.Value = (int)Amount19;
    colorFrequencyEqualization.Value = (int)Amount20;
    lowColorLimit.Value = (int)Amount21;
    highColorLimit.Value = (int)Amount22;
    colorPaletteLength.Value = (int)Amount23;
    colorPaletteHueSegmentation.Value = (int)Amount24;
    oilBrushWidth.Value = (int)Amount25;
    toneBunching.Value = (int)Amount26;
    coverageMultiplier.Value = (int)Amount27;
    debugMode.Value = (int)Amount28;
}

TrackingProperty<int> toneBlendMode = new TrackingProperty<int>();
TrackingProperty<bool> lowTonesBlendFirst = new TrackingProperty<bool>();
TrackingProperty<int> blendSourceWhenNoHighTone = new TrackingProperty<int>();
TrackingProperty<bool> finalizeBlotting = new TrackingProperty<bool>();
TrackingProperty<bool> finalizePalette = new TrackingProperty<bool>();
TrackingProperty<bool> patchBlendingIssues = new TrackingProperty<bool>();
TrackingProperty<int> saturationAdjustment = new TrackingProperty<int>();
TrackingProperty<int> contrastAdjustment = new TrackingProperty<int>();
TrackingProperty<bool> opaque = new TrackingProperty<bool>();
TrackingProperty<bool> showHalf = new TrackingProperty<bool>();
TrackingProperty<bool> showOriginal = new TrackingProperty<bool>();
TrackingProperty<int> reprocess = new TrackingProperty<int>();
TrackingProperty<bool> retainTransparency = new TrackingProperty<bool>();
TrackingProperty<bool> useOriginalTransparencyAsGuideline = new TrackingProperty<bool>();
TrackingProperty<int> rejectAlphaBelow = new TrackingProperty<int>();
TrackingProperty<double> mainXZoomFactor = new TrackingProperty<double>();
TrackingProperty<double> mainYZoomFactor = new TrackingProperty<double>();
TrackingProperty<int> mainBlurRadius = new TrackingProperty<int>();
TrackingProperty<int> colorSampleEfficiency = new TrackingProperty<int>();
TrackingProperty<int> colorFrequencyEqualization = new TrackingProperty<int>();
TrackingProperty<int> lowColorLimit = new TrackingProperty<int>();
TrackingProperty<int> highColorLimit = new TrackingProperty<int>();
TrackingProperty<int> colorPaletteLength = new TrackingProperty<int>();
TrackingProperty<int> colorPaletteHueSegmentation = new TrackingProperty<int>();
TrackingProperty<int> oilBrushWidth = new TrackingProperty<int>();
TrackingProperty<int> toneBunching = new TrackingProperty<int>();
TrackingProperty<int> coverageMultiplier = new TrackingProperty<int>();
TrackingProperty<int> debugMode = new TrackingProperty<int>(); //ListBoxControl Amount24 = 0; // Debug Mode|None|Show Base Layer|Show Blend Layer|Show Blend Issues|Show Base Vs Blend

private int maximumNumberOfSamples = 2;
IntelligentColorPalette palette;

Stopwatch processStopwatch = new Stopwatch();
Stopwatch methodStopwatch = new Stopwatch();

Surface working = null;
Surface working2 = null;
Surface working3 = null;
Surface working4 = null;
Surface dstWorking = null;
Rectangle selection = default(Rectangle);
int selectionCenterX;
int selectionCenterY;
ColorBgra primaryColor;
ColorBgra secondaryColor;
IReadOnlyList<ColorBgra> defaultColors;
IReadOnlyList<ColorBgra> currentColors;
float width;
float height;

protected override void OnDispose(bool disposing)
{
    if (disposing)
    {
        // Release any surfaces or effects you've created.
        if (working != null) working.Dispose();
        if (working2 != null) working2.Dispose();
        if (working3 != null) working3.Dispose();
        if (working4 != null) working4.Dispose();
        if (dstWorking != null) dstWorking.Dispose();
        working = null;
        working2 = null;
        working3 = null;
        working4 = null;
        dstWorking = null;
    }

    base.OnDispose(disposing);
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

unsafe void CopySurfaceRectangle(Surface dst, Surface src, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {    
        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
    
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
            
            *dstPtr = *srcPtr;
            
            srcPtr++;
            dstPtr++;
        }
    }
}

unsafe void BlendSurfaceSeams(Surface dst, int softenAmount, int randomness)
{     
    for (int y = 0; y < dst.Height; y++)
    {    
        if (IsCancelRequested) return;

        if (dst[0, y] == dst[dst.Width-1, y])
        {
            continue;
        }
        
        if (dst[0, y] == dst[1, y] && dst[dst.Width-1, y] == dst[dst.Width-2, y])
        {
            var firstSet = RandomAveragePixel(dst[dst.Width-1, y], dst[0, y], softenAmount, randomness);
            var secondSet = RandomAveragePixel(firstSet.First, firstSet.Second, softenAmount, randomness);

            dst[dst.Width-2, y] = firstSet.First;
            dst[dst.Width-1, y] = secondSet.First;
            dst[0, y] = secondSet.Second;
            dst[1, y] = firstSet.Second;
        }
        else 
        {
            var firstSet = RandomAveragePixel(dst[dst.Width-1, y], dst[0, y], softenAmount, randomness);
            dst[dst.Width-1, y] = firstSet.First;
            dst[1, y] = firstSet.Second;
        }
    }

    for (int x = 0; x < dst.Width; x++)
    {    
        if (dst[x, 0] == dst[x, dst.Height-1])
        {
            continue;
        }
        
        if (dst[x, 0] == dst[x, 1] && dst[x, dst.Height-1] == dst[x, dst.Height-2])
        {
            var firstSet = RandomAveragePixel(dst[x, dst.Height-1], dst[x, 0], softenAmount, randomness);
            var secondSet = RandomAveragePixel(firstSet.First, firstSet.Second, softenAmount, randomness);

            dst[x, dst.Height-2] = firstSet.First;
            dst[x, dst.Height-1] = secondSet.First;
            dst[x, 0] = secondSet.Second;
            dst[x, 1] = firstSet.Second;
        }
        else 
        {
            var firstSet = RandomAveragePixel(dst[x, dst.Height-1], dst[x, 0], softenAmount, randomness);
            dst[x, dst.Height-1] = firstSet.First;
            dst[x, 1] = firstSet.Second;
        }
    }
}


unsafe void SoftenStraightLines(Surface dst, int softenAmount, int randomness)
{     
    for (int y = 0; y < dst.Height; y++)
    {    
        for (int x = 0; x < dst.Width; x++)
        {  
            if (IsCancelRequested) return;

            if (x > 1 && y > 1)
            {
                if (dst[x-1,y] == dst[x-1, y-1] && 
                    dst[x-1,y] == dst[x-1, y-2] && 
                    dst[x,  y] == dst[x,   y-1] && 
                    dst[x,  y] == dst[x,   y-2] && 
                    dst[x,  y] != dst[x-1, y])
                {
                    var firstSet = RandomAveragePixel( dst[x-1, y],   dst[x,y], softenAmount, randomness);
                    var secondSet = RandomAveragePixel(dst[x-1, y-1], dst[x,y-1], softenAmount, randomness);
                    var thirdSet = RandomAveragePixel( dst[x-1, y-2], dst[x,y-2], softenAmount, randomness);

                    dst[x-1, y] =   firstSet.First;
                    dst[x-1, y-1] = secondSet.First;
                    dst[x-1, y-2] = thirdSet.First;

                    dst[x, y] =   firstSet.Second;
                    dst[x, y-1] = secondSet.Second;
                    dst[x, y-2] = thirdSet.Second;
                }

                
                if (dst[x-2,y-1] == dst[x-1, y-1] && 
                    dst[x-2,y-1] == dst[x, y-1] && 
                    dst[x-2,y]   == dst[x-1, y] && 
                    dst[x-2,y]   == dst[x,   y] && 
                    dst[x-2,y-1] != dst[x-2, y])
                {
                    var firstSet = RandomAveragePixel( dst[x-2,y-1],  dst[x-2,y], softenAmount, randomness);
                    var secondSet = RandomAveragePixel(dst[x-1, y-1], dst[x-1,y], softenAmount, randomness);
                    var thirdSet = RandomAveragePixel( dst[x, y-1],   dst[x,y], softenAmount, randomness);

                    dst[x-2,y-1] =   firstSet.First;
                    dst[x-1, y-1] = secondSet.First;
                    dst[x, y-1] = thirdSet.First;

                    dst[x-2,y] =   firstSet.Second;
                    dst[x-1,y] = secondSet.Second;
                    dst[x,y] = thirdSet.Second;
                }
            }
        }
    }
}

unsafe void SoftenIslands(Surface dst)
{     
    for (int y = 0; y < dst.Height; y++)
    {    
        for (int x = 0; x < dst.Width; x++)
        {  
            if (IsCancelRequested) return;

            if (x > 0 && y > 0 && x < dst.Width-1 && y < dst.Height-1)
            {
                var center = dst[x,y];
                var up = dst[x,y-1];
                var down = dst[x,y+1];
                var left = dst[x-1,y];
                var right = dst[x+1,y];

                var similarities = 0;

                similarities += (center == up) ? 1 : 0;
                similarities += (center == left) ? 1 : 0;
                similarities += (center == right) ? 1 : 0;
                similarities += (center == down) ? 1 : 0;

                if (similarities < 3)
                {
                    dst[x,y] = ColorBgra.FromBgr(
                        (byte)(.25f * ((float)up.B + (float)left.B + (float)right.B + (float)down.B)), 
                        (byte)(.25f * ((float)up.G + (float)left.G + (float)right.G + (float)down.G)),
                        (byte)(.25f * ((float)up.R + (float)left.R + (float)right.R + (float)down.R)));
                }
            }
        }
    }
}

Pair<ColorBgra,ColorBgra> RandomAveragePixel(ColorBgra lhs, ColorBgra rhs, int softenAmount, int randomness)
{
    softenAmount = Int32Util.Clamp(softenAmount, 0, 25);
    randomness = Int32Util.Clamp(randomness, 0, 15);

    var diffRng = .25f + (rng.Next(0, randomness) * ((rng.Next(0, 2) < 1) ? -.01f : .01f));
    var diff = new Func<byte, byte, byte>((l,r) => (byte)(l == r ? 0 : diffRng * (Math.Max(l, r) - Math.Min(l, r))));

    var newLhs = ColorBgra.FromBgr(
        (byte)(lhs.B + ((lhs.B > rhs.B ? -1 : 1) * diff(lhs.B, rhs.B))),
        (byte)(lhs.G + ((lhs.G > rhs.G ? -1 : 1) * diff(lhs.G, rhs.G))),
        (byte)(lhs.R + ((lhs.R > rhs.R ? -1 : 1) * diff(lhs.R, rhs.R)))
    );

     var newRhs = ColorBgra.FromBgr(
        (byte)(rhs.B + ((lhs.B < rhs.B ? -1 : 1) * diff(lhs.B, rhs.B))),
        (byte)(rhs.G + ((lhs.G < rhs.G ? -1 : 1) * diff(lhs.G, rhs.G))),
        (byte)(rhs.R + ((lhs.R < rhs.R ? -1 : 1) * diff(lhs.R, rhs.R)))
    );

    return Pair.Create(newLhs, newRhs);
}

private void BlendSurfaces(Surface destination, Surface leftHand, Surface rightHand, double opacity, params BlendModes[] args)
{
    BlendSurfaces(destination, leftHand, rightHand, (float)opacity, args);
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

CloudsEffect cloudEffect = null;
PropertyCollection cloudEffectProperties = null;
PropertyBasedEffectConfigToken cloudEffectConfigToken = null;

void RenderClouds(Surface dst, Surface src, Rectangle rect, int scale, double power)
{ 
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (gaussianBlurEffect == null)
        {
            cloudEffect = new CloudsEffect();
            cloudEffectProperties = cloudEffect.CreatePropertyCollection();
            cloudEffectConfigToken = new PropertyBasedEffectConfigToken(cloudEffectProperties);
            cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Seed, rng.Next(1, 10));
            cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.BlendMode, new UserBlendOps.NormalBlendOp());
        }

        cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Scale, scale);
        cloudEffectConfigToken.SetPropertyValue(CloudsEffect.PropertyNames.Power, power);
        cloudEffect.SetRenderInfo(cloudEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        cloudEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("RenderClouds: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
    
}
      
RotateZoomEffect rotateZoomEffect = null;
PropertyCollection rotateZoomEffectProperties = null;
PropertyBasedEffectConfigToken rotateZoomEffectConfigToken = null;
void RotateZoom(Surface dst, Surface src, Rectangle rect, double tileSize)
{
    if (IsCancelRequested) return;
    
    try
    {
        methodStopwatch.Restart();
        
        if (rotateZoomEffect == null)
        {
            rotateZoomEffect = new RotateZoomEffect();
            rotateZoomEffectProperties = rotateZoomEffect.CreatePropertyCollection();
            rotateZoomEffectConfigToken = new PropertyBasedEffectConfigToken(rotateZoomEffectProperties);
        }
        
        rotateZoomEffectConfigToken.SetPropertyValue(RotateZoomEffect.PropertyNames.Tiling, true);
        rotateZoomEffectConfigToken.SetPropertyValue(RotateZoomEffect.PropertyNames.Zoom, tileSize);
        rotateZoomEffect.SetRenderInfo(rotateZoomEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        rotateZoomEffect.Render(new Rectangle[1] { rect }, 0, 1);    
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("RotateZoom: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

GaussianBlurEffect gaussianBlurEffect = null;
PropertyCollection gaussianBlurEffectProperties = null;
PropertyBasedEffectConfigToken gaussianBlurEffectConfigToken = null;
void GaussianBlur(Surface dst, Surface src, Rectangle rect, int radius)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (gaussianBlurEffect == null)
        {
            gaussianBlurEffect = new GaussianBlurEffect();
            gaussianBlurEffectProperties = gaussianBlurEffect.CreatePropertyCollection();
            gaussianBlurEffectConfigToken = new PropertyBasedEffectConfigToken(gaussianBlurEffectProperties);
        }

        gaussianBlurEffectConfigToken.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, radius);
        gaussianBlurEffect.SetRenderInfo(gaussianBlurEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        gaussianBlurEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("GaussianBlur: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

ReliefEffect reliefEffect = null;
PropertyCollection reliefEffectProperties = null;
PropertyBasedEffectConfigToken reliefEffectConfigToken = null;
void Relief(Surface dst, Surface src, Rectangle rect, double angle)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (reliefEffect == null)
        {
            reliefEffect = new ReliefEffect();
            reliefEffectProperties = reliefEffect.CreatePropertyCollection();
            reliefEffectConfigToken = new PropertyBasedEffectConfigToken(reliefEffectProperties);
        }
        
        reliefEffectConfigToken.SetPropertyValue(ReliefEffect.PropertyNames.Angle, angle);
        reliefEffect.SetRenderInfo(reliefEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        reliefEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("Relief: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

OilPaintingEffect oilPaintingEffect = null;
PropertyCollection oilPaintingEffectProperties = null;
PropertyBasedEffectConfigToken oilPaintingEffectConfigToken = null;
void OilPainting(Surface dst, Surface src, Rectangle rect, int brushSize, int coarseness)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (oilPaintingEffect == null)
        {
            oilPaintingEffect = new OilPaintingEffect();
            oilPaintingEffectProperties = oilPaintingEffect.CreatePropertyCollection();
            oilPaintingEffectConfigToken = new PropertyBasedEffectConfigToken(oilPaintingEffectProperties);
        }

        oilPaintingEffectConfigToken.SetPropertyValue(OilPaintingEffect.PropertyNames.BrushSize, brushSize);
        oilPaintingEffectConfigToken.SetPropertyValue(OilPaintingEffect.PropertyNames.Coarseness, coarseness);
        oilPaintingEffect.SetRenderInfo(oilPaintingEffectConfigToken, new RenderArgs(dst), new RenderArgs(src));

        oilPaintingEffect.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("OilPainting: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

BrightnessAndContrastAdjustment brightnessAndContrastAdjustment = null;
PropertyCollection brightnessAndContrastAdjustmentProperties = null;
PropertyBasedEffectConfigToken brightnessAndContrastAdjustmentConfigToken = null;
void BrightnessAndContrast(Surface dst, Surface src, Rectangle rect, int brightness, int contrast)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (brightnessAndContrastAdjustment == null)
        {
            brightnessAndContrastAdjustment = new BrightnessAndContrastAdjustment();
            brightnessAndContrastAdjustmentProperties = brightnessAndContrastAdjustment.CreatePropertyCollection();
            brightnessAndContrastAdjustmentConfigToken = new PropertyBasedEffectConfigToken(brightnessAndContrastAdjustmentProperties);
        }

        brightnessAndContrastAdjustmentConfigToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Brightness, brightness);
        brightnessAndContrastAdjustmentConfigToken.SetPropertyValue(BrightnessAndContrastAdjustment.PropertyNames.Contrast, contrast);
        brightnessAndContrastAdjustment.SetRenderInfo(brightnessAndContrastAdjustmentConfigToken, new RenderArgs(dst), new RenderArgs(src));

        brightnessAndContrastAdjustment.Render(new Rectangle[1] { rect }, 0, 1);
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("BrightnessAndContrast: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
    }
}

HueAndSaturationAdjustment hueAndSaturationAdjustment = null;
PropertyCollection hueAndSaturationAdjustmentProperties = null;
PropertyBasedEffectConfigToken hueAndSaturationAdjustmentConfigToken = null;
void HueAndSaturation(Surface dst, Surface src, Rectangle rect, int hue, int saturation)
{
    if (IsCancelRequested) return;

    try
    {
        methodStopwatch.Restart();
        
        if (hueAndSaturationAdjustment == null)
        {
            hueAndSaturationAdjustment = new HueAndSaturationAdjustment();
            hueAndSaturationAdjustmentProperties = hueAndSaturationAdjustment.CreatePropertyCollection();
            hueAndSaturationAdjustmentConfigToken = new PropertyBasedEffectConfigToken(hueAndSaturationAdjustmentProperties);
        }

        hueAndSaturationAdjustmentConfigToken.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Hue, hue);
        hueAndSaturationAdjustmentConfigToken.SetPropertyValue(HueAndSaturationAdjustment.PropertyNames.Saturation, saturation);
        hueAndSaturationAdjustment.SetRenderInfo(hueAndSaturationAdjustmentConfigToken, new RenderArgs(dst), new RenderArgs(src));

        hueAndSaturationAdjustment.Render(new Rectangle[1] { rect }, 0, 1);
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
    
    public int GetToneSegmentIdFromColor(ColorBgra color)
    {
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);

        var toneBand = (color.R + color.G + color.B) / 3f;

        var toneBandPercentage = toneBand / 255f;

        if (hueSegment == colorPaletteByHueSegment.Length)
        {
            hueSegment = colorPaletteByHueSegment.Length - 1;
        }

        var palette = colorPaletteByHueSegment[hueSegment];

        var colorIndex = (int)(toneBandPercentage * palette.Length);
         
        if (colorIndex == palette.Length)
        {
            colorIndex = palette.Length - 1;
        }
        
        return colorIndex;
    }
    
    public int GetHueSegmentIdFromColor(ColorBgra color)
    {        
        var hsv = HsvColor.FromColor(color.ToColor());

        var hueSegment = GetHueSegmentFromHsvColor(hsv, colorPaletteByHueSegment.Length);
        
        return hueSegment;
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
    public Pair<int,int> currentCoordinates;
}

void FloodFill(int x, int y, Surface dst, Rectangle rect, Func<FloodFillColorSet, bool> comparison, Func<FloodFillColorSet, ColorBgra> fillOperation)
{
    var stack = new Stack<Tuple<int, int>>();

    stack.Push(new Tuple<int, int>(x, y));

    Tuple<int, int> next;

    var set = new FloodFillColorSet();
    set.initialColor = dst[x,y];
    
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
        set.currentCoordinates = Pair.Create(x,y);

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

void SmoothEdges(Surface dst, Surface src, int radius)
{
    try
    {
        methodStopwatch.Restart();
        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (IsCancelRequested) return;

                int sum = 0;
                var count = 0f;

                var rangeStartX = x - radius;
                var rangeEndX = x + radius;
                var rangeStartY = y - radius;
                var rangeEndY = y + radius;

                if (x < radius)
                {
                    rangeStartX = 0;
                }
                if (x > src.Width - 1 - radius)
                {
                    rangeEndX = src.Width - 1;
                }
                if (y < radius)
                {
                    rangeStartY = 0;
                }
                if (y > src.Height - 1 - radius)
                {
                    rangeEndY = src.Height - 1;
                }

                for (var yIndex = rangeStartY; yIndex <= rangeEndY; yIndex++)
                {
                    for (var xIndex = rangeStartX; xIndex <= rangeEndX; xIndex++)
                    {
                        sum += src[xIndex, yIndex].A;
                        count += 1;
                    }
                }

                var newA = (byte)(int)(((float)sum) / ((float)count));

                if (newA > 10 && newA < 240)
                {
                    count += 1;
                    sum += rng.Next(0, 255);
                    newA = (byte)(int)(((float)sum) / ((float)count));
                }

                var newColor = src[x, y];
                newColor.A = newA;
                dst[x, y] = newColor;
            }
        }
    }
    finally
    {
        methodStopwatch.Stop();
        Debug.WriteLine("SmoothEdges: " + methodStopwatch.ElapsedMilliseconds.ToString() + " ms");
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
            byte r33 = Int32Util.ClampToByte(lhs.R + (2 * rhs.R) - byte.MaxValue);
            byte g33 = Int32Util.ClampToByte(lhs.G + (2 * rhs.G) - byte.MaxValue);
            byte b33 = Int32Util.ClampToByte(lhs.B + (2 * rhs.B) - byte.MaxValue);
            byte a42 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(b33, g33, r33, a42);

        case BlendModes.VividLight:
            byte a43 = normalOp.Apply(lhs, rhs).A;
            byte r34 = rhs.R <= (byte)128 ? Int32Util.ClampToByte(lhs.R + (2 * rhs.R) - byte.MaxValue) : Int32Util.ClampToByte(lhs.R + (2 * (rhs.R - 128)));
            byte g34 = rhs.G <= (byte)128 ? Int32Util.ClampToByte(lhs.G + (2 * rhs.G) - byte.MaxValue) : Int32Util.ClampToByte(lhs.G + (2 * (rhs.G - 128)));
            byte b34 = rhs.B <= (byte)128 ? Int32Util.ClampToByte(lhs.B + (2 * rhs.B) - byte.MaxValue) : Int32Util.ClampToByte(lhs.B + (2 * (rhs.B - 128)));
            return ColorBgra.FromBgra(b34, g34, r34, a43);

        case BlendModes.Yellow:
            byte a44 = normalOp.Apply(lhs, rhs).A;
            return ColorBgra.FromBgra(lhs.B, rhs.G, rhs.R, a44);

        default:
            return normalOp.Apply(lhs, rhs);
    }
}

private unsafe void RotateAndTile(Surface dst, Surface src, Pair<double, double> tileCenter, double xZoom, double yZoom, 
double tiltAngle, double surfaceAngle, double imageAngle,
    int tilingMode)
{
    float midpointX = width / 2f;  //num1
    float midpointY = height / 2f;  // num2
    float tileCenterX = (float)(tileCenter.First * midpointX);  //num3
    float tileCenterY = (float)(tileCenter.Second * midpointY);  //num4

    double tiltAngleRadians = Math.PI * tiltAngle / 180.0; // num7
    double tiltAngleTangent = Math.Tan(tiltAngleRadians); // num8
    float tiltAngleSecant = (float)(1.0 / Math.Cos(tiltAngleRadians)); // num9
    double newWidth = xZoom * width; //num10
    double newHeight = yZoom * height; //num11
    float rotationFactor = 1f / (float)Math.Sqrt((double)newHeight * (double)newHeight + (double)newWidth * (double)newWidth) * (float)tiltAngleTangent; //num12
    double tileSurfaceRadians = Math.PI * surfaceAngle / 180.0; //num13

    if (tiltAngle == 0.0)
    {
        tileSurfaceRadians = 0.0;
    }
    
    float oppositeCosTileSurface = (float)Math.Cos(-tileSurfaceRadians); //num14
    float oppositeSineTileSurface = (float)Math.Sin(-tileSurfaceRadians); //num15
    double imageSurfaceRadians = Math.PI * imageAngle / 180.0; //num16
    float oppositeCosImageSurface = (float)Math.Cos(-imageSurfaceRadians); //num17
    float oppositeSineImageSurface = (float)Math.Sin(-imageSurfaceRadians); //num18

    int xZoomFactorInt = (int)xZoom;
    int yZoomFactorInt = (int)yZoom;

    if ((double)xZoom < 1.0)
    {
        xZoomFactorInt = (int)(1.0 / (double)xZoom);
    }
    if ((double)yZoom < 1.0)
    {
        yZoomFactorInt = (int)(1.0 / (double)yZoom);
    }

    var xSampleCount = Math.Min(xZoomFactorInt, maximumNumberOfSamples);
    var ySampleCount = Math.Min(yZoomFactorInt, maximumNumberOfSamples);

    var xStepSize = 1f / (float)xSampleCount;
    var yStepSize = 1f / (float)ySampleCount;

    var xStepStart = (float)(-0.5 * (1.0 - (double)xStepSize));
    var yStepStart = (float)(-0.5 * (1.0 - (double)yStepSize));

    var Mxw = (float)-((double)tileCenterX + (double)midpointX);
    var Myw = (float)-((double)tileCenterY + (double)midpointY);
    var Mxx = oppositeCosImageSurface;
    var Mxy = -oppositeSineImageSurface;
    float xRotation = (float)((double)oppositeCosImageSurface * (double)Mxw - (double)oppositeSineImageSurface * (double)Myw); //num19
    var Myx = oppositeSineImageSurface;
    var Myy = oppositeCosImageSurface;
    Myw = (float)((double)oppositeSineImageSurface * (double)Mxw + (double)oppositeCosImageSurface * (double)Myw);
    Mxw = xRotation;
    Mxx *= (float)xZoom;
    Mxy *= (float)xZoom;
    Mxw *= (float)xZoom;
    Myx *= (float)yZoom;
    Myy *= (float)yZoom;
    Myw *= (float)yZoom;
    var Mwx = rotationFactor * Myx;
    var Mwy = rotationFactor * Myy;
    var Mww = (float)((double)rotationFactor * (double)Myw + 1.0);
    Myx = tiltAngleSecant * Myx;
    Myy = tiltAngleSecant * Myy;
    Myw = tiltAngleSecant * Myw;
    float xShift = (float)((double)oppositeCosTileSurface * (double)Mxx - (double)oppositeSineTileSurface * (double)Myx); //num20
    float yShift = (float)((double)oppositeCosTileSurface * (double)Mxy - (double)oppositeSineTileSurface * (double)Myy); //num21
    float wShift = (float)((double)oppositeCosTileSurface * (double)Mxw - (double)oppositeSineTileSurface * (double)Myw); //num22
    Myx = (float)((double)oppositeSineTileSurface * (double)Mxx + (double)oppositeCosTileSurface * (double)Myx);
    Myy = (float)((double)oppositeSineTileSurface * (double)Mxy + (double)oppositeCosTileSurface * (double)Myy);
    Myw = (float)((double)oppositeSineTileSurface * (double)Mxw + (double)oppositeCosTileSurface * (double)Myw);
    Mxx = xShift;
    Mxy = yShift;
    Mxw = wShift;
    Mxx += midpointX * Mwx;
    Mxy += midpointX * Mwy;
    Mxw += midpointX * Mww;
    Myx += midpointY * Mwx;
    Myy += midpointY * Mwy;
    Myw += midpointY * Mww;
    
    for (int y = 0; y < src.Height; y++)
    {
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(0, y);
        
        for (int x = 0; x < src.Width; x++)
        {
            if (IsCancelRequested) return;
        
             *dstPtr = SSpix(src, x, y, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx, Mwy, Mww, xSampleCount, ySampleCount, xStepSize, yStepSize, xStepStart, yStepStart, tilingMode);
             
             dstPtr++;
        }
    }
}

private ColorBgra SSpix(Surface src, int x, int y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww,
    int xSampleCount, int ySampleCount, float xStepSize, float yStepSize, float xStepStart, float yStepStart, int tilingMode)
{
    ColorBgra colorBgra = ColorBgra.FromBgra(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
    int sampleCount = xSampleCount * ySampleCount;
    float sumBlue = 0.0f;
    float sumGreen = 0.0f;
    float sumRed = 0.0f;
    float sumAlpha = 0.0f;

    for (int ySampleIndex = 0; ySampleIndex < ySampleCount; ++ySampleIndex)
    {
        float sampleY = (float)((double)(y - selection.Top) + (double)yStepStart + (double)ySampleIndex * (double)yStepSize);

        for (int xSampleIndex = 0; xSampleIndex < xSampleCount; ++xSampleIndex)
        {
            if (IsCancelRequested) return ColorBgra.Black;
            
            float sampleX = (float)((double)(x - selection.Left) + (double)xStepStart + (double)xSampleIndex * (double)xStepSize);
            colorBgra = Move(src, sampleX, sampleY, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx, Mwy, Mww, tilingMode);
            sumBlue += (float)((int)colorBgra.B * (int)colorBgra.A);
            sumGreen += (float)((int)colorBgra.G * (int)colorBgra.A);
            sumRed += (float)((int)colorBgra.R * (int)colorBgra.A);
            sumAlpha += (float)colorBgra.A;
        }
    }
    if ((double)sumAlpha == 0.0)
    {
        return colorBgra;
    }

    return ColorBgra.FromBgra(
          (byte)(int)((double)sumBlue / (double)sumAlpha),
          (byte)(int)((double)sumGreen / (double)sumAlpha),
          (byte)(int)((double)sumRed / (double)sumAlpha),
          (byte)(int)((double)sumAlpha / (double)sampleCount));
}

private ColorBgra Move(Surface src, float x, float y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww, int tilingMode)
{
    if (IsCancelRequested) return ColorBgra.Black;
    
    float shiftXAmount = (float)((double)Mxx * (double)x + (double)Mxy * (double)y) + Mxw; //num1
    float shiftYAmount = (float)((double)Myx * (double)x + (double)Myy * (double)y) + Myw; //num2
    float rotationAmount = (float)((double)Mwx * (double)x + (double)Mwy * (double)y) + Mww; //num3

    ColorBgra colorBgra;
    if ((double)rotationAmount <= 0.0)
    {
        colorBgra = ColorBgra.Transparent;
    }
    else
    {
        float inverseRotationAmount = 1f / rotationAmount; //num4
        float xRotation = inverseRotationAmount * shiftXAmount; //num5
        float yRotation = inverseRotationAmount * shiftYAmount; //num6

        bool flag = false;

        float absoluteXShift = (float)(int)Math.Abs(xRotation / width); // num7
        float absoluteYShift = (float)(int)Math.Abs(yRotation / height); // num8
        float halfRotation = width / 2f; //num9

        switch (tilingMode)
        {
            case 0: // reflect
                if ((double)xRotation < 0.0)
                    xRotation = -xRotation;
                if ((double)xRotation >= (double)width)
                    xRotation = (double)absoluteXShift % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                if ((double)yRotation < 0.0)
                    yRotation = -yRotation;
                if ((double)yRotation >= (double)height)
                {
                    yRotation = (double)absoluteYShift % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                    break;
                }
                break;
            case 1: // repeat
                if ((double)xRotation < 0.0)
                    xRotation = width - Math.Abs(xRotation % width);
                if ((double)xRotation >= (double)width)
                    xRotation %= width;
                if ((double)yRotation < 0.0)
                    yRotation = height - Math.Abs(yRotation % height);
                if ((double)yRotation >= (double)height)
                {
                    yRotation %= height;
                    break;
                }
                break;
            case 2: // reflect brick
                if ((double)yRotation >= 0.0)
                {
                    ++absoluteYShift;
                }
                if ((double)absoluteYShift % 2.0 == 0.0)
                {
                    xRotation = halfRotation + xRotation;
                }
                float yBrick = (float)(int)Math.Abs(yRotation / height); // num10
                float xBrick = (float)(int)Math.Abs(xRotation / width); // num11
                if ((double)xRotation < 0.0)
                {
                    xRotation = -xRotation;
                }
                if ((double)xRotation >= (double)width)
                {
                    xRotation = (double)xBrick % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                }
                if ((double)yRotation < 0.0)
                {
                    yRotation = -yRotation;
                }
                if ((double)yRotation >= (double)height)
                {
                    yRotation = (double)yBrick % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                    break;
                }
                break;
            case 3: // repeat brick
                if ((double)yRotation >= 0.0)
                {
                    ++absoluteYShift;
                }
                if ((double)absoluteYShift % 2.0 == 0.0)
                {
                    xRotation = halfRotation + xRotation;
                }
                if ((double)xRotation < 0.0)
                {
                    xRotation = width - Math.Abs(xRotation % width);
                }
                xRotation %= width;
                if ((double)yRotation < 0.0)
                {
                    yRotation = height - Math.Abs(yRotation % height);
                }
                yRotation %= height;
                break;
            case 4: // none
                if ((double) xRotation < (double) selection.Left || (double) xRotation > (double) selection.Right || ((double) yRotation < (double) selection.Top || (double) yRotation > (double) selection.Bottom))
                {
                    flag = true;
                    break;
                }
                break;
        }

        colorBgra = src.GetBilinearSampleClamped(xRotation + (float)selection.Left, yRotation + (float)selection.Top);

        if (flag)
        {
            colorBgra = ColorBgra.Transparent;
        }
    }
    return colorBgra;
}

public class PixelRef
{
    public int x;
    public int y;
    public ColorBgra color;
}