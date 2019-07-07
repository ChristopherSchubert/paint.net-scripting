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
IntSliderControl Amount1 = 24; // [0,256] Blend Start
IntSliderControl Amount2 = 24; // [0,256] Blend Distance
BinaryPixelOp Amount3 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Blend Type
CheckboxControl Amount4 = false; // [0,1] Fade Original Color Out
CheckboxControl Amount5 = true; // [0,1] Fade Swapped Color In
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    var blendStart = (int)Amount1;
    var blendDistance = (int)Amount2;
    var blendOp = (BinaryPixelOp)Amount3;
    var fadeOriginalColorOut = (bool)Amount4;
    var fadeSwappedColorIn = (bool)Amount5;
    
    CreateSeamlessSurfaceFromOriginalSurface(dst, src, rect, blendStart, blendDistance, blendOp, fadeOriginalColorOut, fadeSwappedColorIn);
}

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
