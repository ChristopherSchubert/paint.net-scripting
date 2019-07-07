// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
// Force Single Render Call
#region UICode
IntSliderControl Amount1 = 1; // [1,100] TrimAmount
IntSliderControl Amount2 = 3; // [1,100] BlurAmount
IntSliderControl Amount3 = 1; // [1,5] EscapeRouteThickness
#endregion

void PreRender(Surface dst, Surface src)
{
}

// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
   Surface working = new Surface(dst.Size);
   Surface working2 = new Surface(dst.Size);
   
   // Setup for calling Gaussian Blur
    GaussianBlurEffect blurEffect = new GaussianBlurEffect();
    PropertyCollection bProps = blurEffect.CreatePropertyCollection();
    PropertyBasedEffectConfigToken bParameters = new PropertyBasedEffectConfigToken(bProps);
    bParameters.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, Amount2); // fix
    blurEffect.SetRenderInfo(bParameters, new RenderArgs(working), new RenderArgs(src));
    // Call Gaussian Blur
    blurEffect.Render(new Rectangle[1] {rect},0,1);
   
   
    ColorBgra CurrentPixel;
    
    var borderAmount = Amount1;

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
        
            CurrentPixel = working[x,y];

            if (x < borderAmount || y < borderAmount || x > rect.Right-borderAmount-1 || y > rect.Bottom-borderAmount-1)
            {
                CurrentPixel = ColorBgra.FromBgra(000,000,000,000);
            }
            else if (CurrentPixel.R > 000 && CurrentPixel.G > 000 && CurrentPixel.B > 000)
            {
                CurrentPixel = ColorBgra.FromBgra(255,255,255,255);
            }
            else if (CurrentPixel.R > 000)
            {
                CurrentPixel = ColorBgra.FromBgra(000,000,255,255);
            }
            else if (CurrentPixel.G > 000)
            {
                CurrentPixel = ColorBgra.FromBgra(000,255,000,255);
            }
            else if (CurrentPixel.B > 000)
            {
                CurrentPixel = ColorBgra.FromBgra(255,000,000,255);
            }
            else
            {
                CurrentPixel = ColorBgra.FromBgra(000,000,000,255);
            } 
            
            working2[x,y] = CurrentPixel;
        }
    }
    
    FloodFill(0, 0, working2, rect, 
    (color) => color == ColorBgra.Black || color == ColorBgra.TransparentBlack,
    (color) => ColorBgra.Orange
    );
    
    FloodFill(0, 0, working2, rect, 
    (color) => color == ColorBgra.Orange,
    (color) => ColorBgra.TransparentBlack
    );
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;
        
            CurrentPixel = working2[x,y];
            
            if (CurrentPixel == ColorBgra.Black)
            {
                var nearestTransparent = FindNearestTransparentPixel(x,y,working2, rect);                
                
                if (IsCancelRequested) return;
                
                var pathCoordinates = GetStraightPathBetweenTwoPoints(x, y, nearestTransparent.Item1, nearestTransparent.Item2, Amount3);
                
                if (IsCancelRequested) return;
                
                foreach(var pathCoordinate in pathCoordinates)
                {
                    if (pathCoordinate.Item1 == x && pathCoordinate.Item2 == y)
                    {
                        continue;
                    }
                    
                    working2[pathCoordinate.Item1, pathCoordinate.Item2] = ColorBgra.TransparentBlack;
                }
                
                FloodFill(x, y, working2, rect, (color) => color == ColorBgra.Black, (color) => ColorBgra.TransparentBlack);
            }
        }
    }
    
    RemoveIslandPixels(working2, rect);
    
    dst.CopySurface(working2);
}


void FloodFill(int x, int y, Surface dst, Rectangle rect, Func<ColorBgra, bool> comparison, Func<ColorBgra, ColorBgra> fillOperation)
{
    var stack = new Stack<Tuple<int,int>>();
    
    stack.Push(new Tuple<int,int>(x,y));
    
    Tuple<int,int> next;
    
    //while(false)
    while(stack.Count > 0)
    {
        if (IsCancelRequested) return;
        
        next = stack.Pop();
        
        x = next.Item1;
        y = next.Item2;
    
        if ((x < 0) || (x >= rect.Right)) continue;
        if ((y < 0) || (y >= rect.Bottom)) continue;    
        
        if (comparison(dst[x,y]))
        {
            if (IsCancelRequested) return;
        
            dst[x,y] = fillOperation(dst[x,y]);
            
            stack.Push(new Tuple<int,int>(x+1,y));
            stack.Push(new Tuple<int,int>(x,y+1));
            stack.Push(new Tuple<int,int>(x-1,y));
            stack.Push(new Tuple<int,int>(x,y-1));
            
        }
    }
}

Tuple<int,int> FindNearestTransparentPixel(int x, int y, Surface dst, Rectangle rect)
{
    var minDirectionIndex = -1;
    var minDistance = Int32.MaxValue;
    
    int currentDistance, currentX, currentY;

    for(var currentDirectionIndex = 0; currentDirectionIndex < 8; currentDirectionIndex++)
    {
        currentX = x;
        currentY = y;
        currentDistance = 0;
        
        var movement = GetXYForMovementIndex(currentDirectionIndex);
        var found = false;
        
        while(currentX >= 0 && currentX <= rect.Right-1 && currentY >= 0 && currentY <= rect.Bottom-1 && currentDistance < minDistance)
        {
            
            if (IsCancelRequested) return new Tuple<int,int>(-1,-1);
    
            currentX += movement.Item1;
            currentY += movement.Item2;
            
            currentDistance += 1;
        
            if (dst[currentX,currentY] == ColorBgra.TransparentBlack)
            {
                found = true;
                break;
            }
        }    
        
        if (found && currentDistance < minDistance)
        {
            minDistance = currentDistance;
            minDirectionIndex = currentDirectionIndex;
        }
    }
    
    if (minDirectionIndex == -1)
    {
        return new Tuple<int,int>(-1,-1);
    }
    
    var finalMovementDirection = GetXYForMovementIndex(minDirectionIndex);
    return new Tuple<int,int>(x + (finalMovementDirection.Item1*minDistance), y + (finalMovementDirection.Item2*minDistance));
}

Tuple<int, int> GetXYForMovementIndex(int movementIndex)
{
    int x = 0, y = 0;
    
    if (movementIndex == 0 || movementIndex == 1 || movementIndex == 7)
    {
        x = 1;
    }
    else if (movementIndex == 3 || movementIndex == 4 || movementIndex == 5)
    {
        x = -1;
    }
    
    if (movementIndex == 1 || movementIndex == 2 || movementIndex == 3)
    {
        y = 1;
    }
    else if (movementIndex == 5 || movementIndex == 6 || movementIndex == 7)
    {
        y = -1;
    }
    
    return new Tuple<int,int>(x, y);
}

List<Tuple<int,int>> GetStraightPathBetweenTwoPoints(int startingX, int startingY, int endingX, int endingY, int thickness)
{
    Debug.WriteLine("Getting path from (" + startingX.ToString() + "," +startingY.ToString() + ") -to- (" + endingX.ToString() + "," + endingY.ToString() + ")");
    
    var results = new List<Tuple<int,int>>();
    if (IsCancelRequested) return results;
    
    int[] xValues;
    
    if (startingX == endingX)
    {
        xValues = new int[0];
    }
    else
    {
        xValues = new int[Math.Abs(startingX-endingX)+1];
        
        if (startingX < endingX)
        {
            for(var i = 0; i < xValues.Length; i++)
            {
                xValues[i] = startingX+i;
            }
        }
        else
        {
            for(var i = 0; i < xValues.Length; i++)
            {
                xValues[i] = startingX-i;
            }
        }
    }
    
    int[] yValues;
    
    if (startingY == endingY)
    {
        yValues = new int[0];
    }
    else
    {
        yValues = new int[Math.Abs(startingY-endingY)+1];
        
        if (startingY < endingY)
        {
            for(var i = 0; i < yValues.Length; i++)
            {
                yValues[i] = startingY+i;
            }
        }
        else
        {
            for(var i = 0; i < yValues.Length; i++)
            {
                yValues[i] = startingY-i;
            }
        }
    }
    
    var maxSteps = Math.Max(yValues.Length, xValues.Length);
    
    for(var i = 0; i < maxSteps; i++)
    {
        if (xValues.Length == 0)
        {
            results.Add(new Tuple<int,int>(endingX, yValues[i]));
        }
        else if (yValues.Length == 0)
        {
            results.Add(new Tuple<int,int>(xValues[i], endingY));
        }
        else
        {
            results.Add(new Tuple<int,int>(xValues[i], yValues[i]));
        }
    }
    
    for(var i = results.Count-1; i >= 0; i--)    
    {  
        for(var thicknessIterator = 1; thicknessIterator <= thickness; thicknessIterator++)
        {
            if (yValues.Length == 0)
            {
                results.Add(new Tuple<int,int>(xValues[i], endingY+thicknessIterator));
            }
            else if (xValues.Length == 0)
            {
                results.Add(new Tuple<int,int>(endingX+thicknessIterator, yValues[i]));
            }
            else if (startingX < endingX)
            {
                results.Add(new Tuple<int,int>(xValues[i]-thicknessIterator, yValues[i]));
            }
            else
            {
                results.Add(new Tuple<int,int>(xValues[i]+thicknessIterator, yValues[i]));
            }
        }
    }
    
    return results;
}


void RemoveIslandPixels (Surface dst, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            if (IsCancelRequested) return;

            if (dst[x,y] == ColorBgra.TransparentBlack)
            {
                continue;
            }
            
            if (
                dst[x+1,y+0] == ColorBgra.TransparentBlack &&
                dst[x+1,y+1] == ColorBgra.TransparentBlack &&
                dst[x+0,y+1] == ColorBgra.TransparentBlack &&
                dst[x-1,y+1] == ColorBgra.TransparentBlack &&
                dst[x-1,y+0] == ColorBgra.TransparentBlack &&
                dst[x-1,y-1] == ColorBgra.TransparentBlack &&
                dst[x+0,y-1] == ColorBgra.TransparentBlack &&
                dst[x+1,y-1] == ColorBgra.TransparentBlack)
            {
                Debug.WriteLine("Removing island at (" + x.ToString() + "," + y.ToString() + ").");
                dst[x,y] = ColorBgra.TransparentBlack;
            }
            
        }
    }
}